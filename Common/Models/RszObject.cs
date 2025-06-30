using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models.List_Wrappers;
using RE_Editor.Common.Structs;

#pragma warning disable CS8600
#pragma warning disable CS8618

namespace RE_Editor.Common.Models;

[SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public class RszObject : OnPropertyChangedBase {
    public                   StructJson structInfo;
    [DisplayName("")] public RSZ        rsz { get; protected set; }
    protected                int        userDataRef = -1;
    public                   int        objectInstanceIndex; // 0 is invalid, field is one based. This is assigned during write so the order is correct.
    private                  long       pos; // Used during testing to make sure read/write without altering anything is written in the same spot.

    [SortOrder(int.MaxValue - 1000)]
    public int Index { [UsedImplicitly] get; set; }

    public virtual T Copy<T>() where T : RszObject {
        var obj = (T) Activator.CreateInstance(typeof(T), null)!;
        obj.rsz         = rsz;
        obj.userDataRef = userDataRef;
        obj.structInfo  = structInfo;
        return obj;
    }

    /// <summary>
    /// To be used to init required fields when manually instancing a generated class.
    /// </summary>
    [SuppressMessage("ReSharper", "ParameterHidesMember")]
    public static T Create<T>(RSZ rsz, uint hash) where T : RszObject {
        var structInfo = DataHelper.STRUCT_INFO[hash];
        var rszObject  = CreateRszObjectInstance(hash, structInfo);
        rszObject.structInfo = structInfo;
        rszObject.rsz        = rsz;
        return (T) rszObject;
    }

    public static RszObject Read(BinaryReader reader, uint hash, RSZ rsz, int userDataRef) {
        if (!DataHelper.STRUCT_INFO.TryGetValue(hash, out var structInfo)) {
            Debug.WriteLine($"Unknown hash: {hash:X}");
            throw new FileNotSupported();
        }

        if (userDataRef > -1) {
            return new UserDataShell(hash, rsz) {
                userDataRef = userDataRef
            };
        }

        var rszObject = CreateRszObjectInstance(hash, structInfo);
        rszObject.structInfo = structInfo;
        rszObject.rsz        = rsz;
        rszObject.pos        = reader.BaseStream.Position;

        switch (rszObject) {
            // ReSharper disable once SuspiciousTypeConversion.Global
            case ICustomReadWrite customReadWrite:
                customReadWrite.Read(reader);
                return rszObject;
            case IViaType viaTypeAsObject:
                viaTypeAsObject.Read(reader);
                return rszObject;
        }

        for (var i = 0; i < structInfo.fields!.Count; i++) {
            var field            = structInfo.fields[i];
            var fieldName        = field.name?.ToConvertedFieldName()!;
            var primitiveName    = field.GetCSharpType();
            var viaType          = field.type?.GetViaType().AsType();
            var isNonPrimitive   = primitiveName == null;
            var isUserData       = field.type == "UserData";
            var isStruct         = field.type == "Struct";
            var isObjectType     = field.type == nameof(Object);
            var fieldInfo        = rszObject.GetType().GetProperty(fieldName)!;
            var fieldType        = fieldInfo.PropertyType;
            var isStringType     = field.type == "String" || fieldType == typeof(string) || fieldType == typeof(ObservableCollection<GenericWrapper<string>>);
            var fieldGenericType = fieldType.IsGenericType ? fieldType.GenericTypeArguments[0] : null; // GetInnermostGenericType(fieldType);
            var fieldSetMethod   = fieldInfo.SetMethod!;

            if (isUserData) fieldGenericType = typeof(UserDataShell);

            // Be careful with lists. The 'align' in them refers to their contents, not their count themselves, which is always a 4-aligned int.
            var align = field.GetAlign();
            reader.BaseStream.Align(align);

            if (field.array) {
                var arrayCount = reader.ReadInt32();
                if (arrayCount > 100000) {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    Debug.Assert(arrayCount < 100000, "`arrayCount` over 100k. This is probably reading the count from the wrong spot.");
                    throw new FileNotSupported();
                }

                if (field.originalType!.StartsWith("System.ValueTuple")) {
                    var objects = new List<object>();
                    for (var s = 0; s < arrayCount; s++) {
                        var data = ReadTuple(reader, field);
                        objects.Add(data);
                    }
                    var items = objects.GetGenericItemsOfType(fieldGenericType!, true);
                    SetList(items, fieldSetMethod, rszObject);
                } else if (fieldName == Global.BITSET_FIELD_NAME && structInfo.parent?.ToConvertedTypeName() == Global.BITSET_NAME) {
                    var bytes               = reader.ReadBytes(field.size * arrayCount);
                    var bitset              = new BitArray(bytes);
                    var bitarrayLengthField = rszObject.GetType().GetProperty(fieldName + "_Length")!.GetSetMethod()!;
                    bitarrayLengthField.Invoke(rszObject, [bytes.Length]);
                    fieldSetMethod.Invoke(rszObject, [bitset]);
                } else if (field.type is nameof(UIntArray) or "OBB") {
                    Debug.Assert((float) field.size % 4 == 0, $"Error: `Data` field size is not a multiple of {UIntArray.DATA_WIDTH}.");
                    var dataWidth = (uint) (field.size / UIntArray.DATA_WIDTH);
                    var objects   = new ObservableCollection<RszObject>();
                    for (var s = 0; s < arrayCount; s++) {
                        reader.BaseStream.Align(field.align);
                        var data = new UIntArray(dataWidth);
                        data.Read(reader);
                        objects.Add(data);
                    }
                    var items = objects.GetGenericItemsOfType(fieldGenericType!, true);
                    SetList(items, fieldSetMethod, rszObject);
                } else if (isStruct) { // Array of structs.
                    var objects        = new List<RszObject>();
                    var structTypeHash = DataHelper.STRUCT_HASH_BY_NAME[field.originalType!];
                    for (var index = 0; index < arrayCount; index++) {
                        var instance = Read(reader, structTypeHash, rsz, -1);
                        objects.Add(instance);
                    }
                    var items = objects.GetGenericItemsOfType(fieldGenericType!, true);
                    SetList(items, fieldSetMethod, rszObject);
                } else if (isObjectType || isUserData) { // Array of pointers.
                    var objects = new List<RszObject>();
                    for (var index = 0; index < arrayCount; index++) {
                        objects.Add(rsz.objectData[reader.ReadInt32() - 1]);
                    }
                    var items = objects.GetGenericItemsOfType(fieldGenericType!, true);
                    SetList(items, fieldSetMethod, rszObject);
                } else if (isStringType) { // Array of strings.
                    var strings = new List<GenericWrapper<string?>>(arrayCount);
                    for (var s = 0; s < arrayCount; s++) {
                        reader.BaseStream.Align(field.align);
                        strings.Add(new(s, reader.ReadWString()));
                    }
                    SetList(strings, fieldSetMethod, rszObject);
                } else if (isNonPrimitive) { // Array of embedded objects. (Built-in types like via.vec2.)
                    var objects = new List<IViaType>(arrayCount);
                    for (var s = 0; s < arrayCount; s++) {
                        reader.BaseStream.Align(field.align);
                        var instance = (IViaType) Activator.CreateInstance(viaType!)!;
                        instance.Read(reader);
                        objects.Add(instance);
                    }
                    var items = objects.GetGenericItemsOfType(fieldGenericType!, true);
                    SetList(items, fieldSetMethod, rszObject);
                } else { // Primitive array.
                    var bytes         = reader.ReadBytes(field.size * arrayCount);
                    var genericMethod = typeof(RszObjectExtensions).GetMethod(nameof(RszObjectExtensions.GetDataAsList))!.MakeGenericMethod(fieldGenericType!);
                    var items         = genericMethod.Invoke(null, [bytes, field.size, arrayCount, field])!;
                    SetList(items, fieldSetMethod, rszObject);
                }
            } else {
                if (field.originalType!.StartsWith("System.ValueTuple")) {
                    List<object> objects = [ReadTuple(reader, field)];
                    var          items   = objects.GetGenericItemsOfType(fieldGenericType!, true);
                    SetList(items, fieldSetMethod, rszObject);
                } else if (field.type is nameof(UIntArray) or "OBB") {
                    Debug.Assert((float) field.size % 4 == 0, $"Error: `Data` field size is not a multiple of {UIntArray.DATA_WIDTH}.");
                    var data = new UIntArray((uint) (field.size / UIntArray.DATA_WIDTH));
                    data.Read(reader);
                    var items = new ObservableCollection<UIntArray> {data};
                    SetList(items, fieldSetMethod, rszObject);
                } else if (isObjectType || isUserData) { // Pointer to object.
                    var objectIndex = reader.ReadInt32() - 1; // Will be `0` for some `UserData` entries with no data in them.
                    // In which case just move onto the next field.
                    // But this still needs to not be null, so it doesn't break on write.
                    var objects = objectIndex == -1 ? [] : new List<RszObject> {rsz.objectData[objectIndex]};
                    var items   = objects.GetGenericItemsOfType(fieldGenericType!, true);
                    SetList(items, fieldSetMethod, rszObject);
                } else if (isStringType) { // A string.
                    var str = reader.ReadWString();
                    fieldSetMethod.Invoke(rszObject, [str]);
                } else if (isNonPrimitive) { // Embedded object. (A built-in type like via.vec2.)
                    var instance = (IViaType) Activator.CreateInstance(viaType!)!;
                    instance.Read(reader);
                    if (viaType?.Is(typeof(ISimpleViaType)) == true) {
                        SetDirect(instance, fieldSetMethod, rszObject);
                    } else {
                        var items = new List<IViaType> {instance}.GetGenericItemsOfType(fieldGenericType!, true);
                        SetList(items, fieldSetMethod, rszObject); // Treated as a list, so we have an 'open' button.
                    }
                } else { // A primitive.
                    var bytes = reader.ReadBytes(field.size);
                    var data  = bytes.GetDataAs(fieldType);
                    fieldSetMethod.Invoke(rszObject, [data]);
                }
            }
        }
        return rszObject;
    }

    private static void SetDirect(object item, MethodBase fieldSetMethod, RszObject rszObject) {
        fieldSetMethod.Invoke(rszObject, [item]);
    }

    private static void SetList(object items, MethodBase fieldSetMethod, RszObject rszObject) {
        var data = MakeGenericObservableCollection((dynamic) items);
        fieldSetMethod.Invoke(rszObject, [data]);
    }

    public static ObservableCollection<T> MakeGenericObservableCollection<T>(IEnumerable<T> itemSource) {
        return itemSource as ObservableCollection<T> ?? new(itemSource);
    }

    /**
     * If the hash isn't found it'll just return the base `RszObject`.
     */
    private static RszObject CreateRszObjectInstance(uint hash, StructJson structInfo) {
        var viaType = structInfo.name?.GetViaType();
        var rszType = viaType == null ? DataHelper.RE_STRUCTS.TryGet(hash, typeof(RszObject)) : Type.GetType($"RE_Editor.Common.Structs.{viaType}");
        if (rszType == typeof(Prefab)) {
            return (RszObject) Activator.CreateInstance(rszType, hash)!;
        }
        var rszObject = (RszObject) Activator.CreateInstance(rszType!) ?? new RszObject();
        return rszObject;
    }

    /**
     * Run before writing to set up all the instance info / indexes, so we know exactly where an object is being written.
     * This is how we know what to point an 'object' field to.
     */
    public void SetupInstanceInfo(List<InstanceInfo> instanceInfoList, bool forGp) {
        if (this is not UserDataShell) {
            for (var i = 0; i < structInfo.fields!.Count; i++) {
                var field          = structInfo.fields[i];
                var fieldName      = field.name?.ToConvertedFieldName()!;
                var fieldInfo      = GetType().GetProperty(fieldName)!;
                var isUserData     = field.type == "UserData";
                var isObjectType   = field.type == nameof(Object);
                var fieldGetMethod = fieldInfo.GetMethod!;

                if (isObjectType || isUserData) {
                    if (field.array) { // Array of pointers.
                        var list = (IList) fieldGetMethod.Invoke(this, null)!;
                        foreach (RszObject obj in list) {
                            obj.SetupInstanceInfo(instanceInfoList, forGp);
                        }
                    } else { // Pointer to object.
                        // So it works in the UI, we always put the object in a list. Thus, even if not an array, we need to extract from a list.
                        var list = (IList) fieldGetMethod.Invoke(this, null)!;
                        if (list.Count > 0) {
                            ((RszObject) list[0]!).SetupInstanceInfo(instanceInfoList, forGp);
                        }
                    }
                }
            }
        }

        if (TryGetMatchingInstanceInfoEntry(instanceInfoList, out var userDataIndex)) {
            objectInstanceIndex = userDataIndex;
            return;
        }

        var hash = this switch {
            UserDataShell udc => udc.hash,
            Prefab prefab => prefab.hash,
            _ => (uint) GetType().GetField("HASH")!.GetValue(null)!
        };
        var crc = structInfo.crc;

        if (forGp && DataHelper.GP_CRC_OVERRIDE_INFO.TryGetValue(hash, out var value)) {
            crc = value;
        }

        var instanceInfo = new InstanceInfo {
            hash = hash,
            crc  = crc,
        };
        if (this is UserDataShell) {
            instanceInfo.userDataRef = userDataRef;
            // Make sure the userDataInfo entry points to this instance info.
            rsz.userDataInfo[userDataRef].instanceId = instanceInfoList.Count;
        }
        instanceInfoList.Add(instanceInfo);

        objectInstanceIndex = instanceInfoList.Count - 1;
    }

    private bool TryGetMatchingInstanceInfoEntry(IList<InstanceInfo> instanceInfoList, out int index) {
        if (this is not UserDataShell) {
            index = -1;
            return false;
        }
        for (var i = 0; i < instanceInfoList.Count; i++) {
            if (instanceInfoList[i].userDataRef == userDataRef) {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }

    public void Write(BinaryWriter writer, bool testWritePosition) {
        // Do once to write all child objects first.
        for (var i = 0; i < structInfo.fields!.Count; i++) {
            var field          = structInfo.fields[i];
            var fieldName      = field.name?.ToConvertedFieldName()!;
            var fieldInfo      = GetType().GetProperty(fieldName)!;
            var isObjectType   = field.type == nameof(Object);
            var fieldGetMethod = fieldInfo.GetMethod!;

            if (isObjectType) {
                if (field.array) { // Array of pointers.
                    var list = (IList) fieldGetMethod.Invoke(this, null)!;
                    foreach (var obj in list) {
                        ((RszObject) obj).Write(writer, testWritePosition);
                    }
                } else { // Pointer to object.
                    // So it works in the UI, we always put the object in a list. Thus, even if not an array, we need to extract from a list.
                    var list = (IList) fieldGetMethod.Invoke(this, null)!;
                    ((RszObject) list[0]!).Write(writer, testWritePosition);
                }
            }
        }

        if (testWritePosition) {
            Debug.Assert(pos == writer.BaseStream.Position, $"Position Mismatch: Expected {pos}, found {writer.BaseStream.Position}.\n" +
                                                            $"After Writing: {structInfo.name}");
        }

        switch (this) {
            // ReSharper disable once SuspiciousTypeConversion.Global
            case ICustomReadWrite customReadWrite:
                customReadWrite.Write(writer);
                return;
            case IViaType viaTypeAsObject:
                viaTypeAsObject.Write(writer);
                return;
        }

        for (var i = 0; i < structInfo.fields!.Count; i++) {
            var field          = structInfo.fields[i];
            var fieldName      = field.name?.ToConvertedFieldName()!;
            var primitiveName  = field.GetCSharpType();
            var viaType        = field.type?.GetViaType().AsType();
            var isNonPrimitive = primitiveName == null;
            var isUserData     = field.type == "UserData";
            var isStruct       = field.type == "Struct";
            var isObjectType   = field.type == nameof(Object);
            var fieldInfo      = GetType().GetProperty(fieldName)!;
            var fieldType      = fieldInfo.PropertyType;
            var isStringType   = field.type == "String" || fieldType == typeof(string) || fieldType == typeof(ObservableCollection<GenericWrapper<string>>);
            var fieldGetMethod = fieldInfo.GetMethod!;

            // Be careful with lists. The 'align' in them refers to their contents, not their count themselves, which is always a 4-aligned int.
            var align = field.GetAlign();
            writer.PadTill(() => writer.BaseStream.Position % align != 0);

            if (field.array) {
                if (field.originalType!.StartsWith("System.ValueTuple")) {
                    var list = (IList) fieldGetMethod.Invoke(this, null)!;
                    writer.Write(list.Count);
                    foreach (var obj in list) {
                        WriteTuple(writer, field, (ITuple) obj);
                    }
                } else if (fieldName == Global.BITSET_FIELD_NAME && structInfo.parent?.ToConvertedTypeName() == Global.BITSET_NAME) {
                    var bitset              = (BitArray) fieldGetMethod.Invoke(this, null)!;
                    var bitarrayLengthField = GetType().GetProperty(fieldName + "_Length")!.GetGetMethod()!;
                    var byteCount           = (int) bitarrayLengthField.Invoke(this, null)!;
                    var bytes               = new byte[byteCount];
                    bitset.CopyTo(bytes, 0);
                    writer.Write(byteCount / field.size); // Because the count treats it as ints and we're handling it as bytes.
                    writer.Write(bytes);
                } else if (field.type is nameof(UIntArray) or "OBB") {
                    var list = (ObservableCollection<UIntArray>) fieldGetMethod.Invoke(this, null)!;
                    writer.Write(list.Count);
                    foreach (var obj in list) {
                        writer.BaseStream.Align(field.align);
                        obj.Write(writer);
                    }
                } else if (isStruct) { // Array of structs.
                    var list = (IList) fieldGetMethod.Invoke(this, null)!;
                    writer.Write(list.Count);
                    foreach (var obj in list) {
                        ((RszObject) obj).Write(writer, testWritePosition);
                    }
                } else if (isObjectType || isUserData) { // Array of pointers.
                    var list = (IList) fieldGetMethod.Invoke(this, null)!;
                    writer.Write(list.Count);
                    foreach (RszObject obj in list) {
                        writer.Write(obj.objectInstanceIndex);
                    }
                } else if (isStringType) { // Array of strings.
                    var list = (IList) fieldGetMethod.Invoke(this, null)!;
                    writer.Write(list.Count);
                    foreach (GenericWrapper<string> obj in list) {
                        writer.BaseStream.Align(field.align);
                        writer.WriteWString(obj.Value);
                    }
                } else if (isNonPrimitive) { // Array of embedded objects. (Built-in types like via.vec2.)
                    var list = (IList) fieldGetMethod.Invoke(this, null)!;
                    writer.Write(list.Count);
                    foreach (var obj in list) {
                        writer.BaseStream.Align(field.align);
                        ((IViaType) obj).Write(writer);
                    }
                } else { // Primitive array.
                    var list = (IList) fieldGetMethod.Invoke(this, null)!;
                    writer.Write(list.Count);
                    foreach (var obj in list) {
                        byte[] bytes;
                        if (obj.GetType().IsGeneric(typeof(IListWrapper<>))) {
                            var value = ((dynamic) obj).Value;
                            bytes = Extensions.GetBytes(value);
                        } else {
                            bytes = obj.GetBytes();
                        }
                        writer.Write(bytes);
                    }
                }
            } else {
                if (field.originalType!.StartsWith("System.ValueTuple")) {
                    var data = (IList) fieldGetMethod.Invoke(this, null)!;
                    var obj  = (ITuple) data[0]!;
                    WriteTuple(writer, field, obj);
                } else if (field.type is nameof(UIntArray) or "OBB") {
                    var list = (ObservableCollection<UIntArray>) fieldGetMethod.Invoke(this, null)!;
                    list[0].Write(writer);
                } else if (isObjectType || isUserData) { // Pointer to object.
                    var data = fieldGetMethod.Invoke(this, null)!;
                    if (((IEnumerable) data).Cast<object?>().Any()) {
                        var obj = (RszObject) ((IList) data)[0]!;
                        writer.Write(obj.objectInstanceIndex);
                    } else {
                        writer.Write(0);
                    }
                } else if (isStringType) { // Array of strings.
                    var str = (string?) fieldGetMethod.Invoke(this, null)!;
                    writer.WriteWString(str);
                } else if (isNonPrimitive) { // Embedded object. (A built-in type like via.vec2.)
                    if (viaType?.Is(typeof(ISimpleViaType)) == true) {
                        var simpleType = (IViaType) fieldGetMethod.Invoke(this, null)!;
                        simpleType.Write(writer);
                    } else {
                        // So it works in the UI, we always put the object in a list. Thus, even if not an array, we need to extract from a list.
                        var list = (IList) fieldGetMethod.Invoke(this, null)!;
                        ((IViaType) list[0]!).Write(writer);
                    }
                } else { // A primitive.
                    var obj   = fieldGetMethod.Invoke(this, null)!;
                    var bytes = obj.GetBytes();
                    writer.Write(bytes);
                }
            }
        }
    }

    public void WriteObjectList(List<RszObject> objectList) {
        // Add all child objects first.
        if (this is not UserDataShell) {
            for (var i = 0; i < structInfo.fields!.Count; i++) {
                var field          = structInfo.fields[i];
                var fieldName      = field.name?.ToConvertedFieldName()!;
                var fieldInfo      = GetType().GetProperty(fieldName)!;
                var isUserData     = field.type == "UserData";
                var isObjectType   = field.type == nameof(Object);
                var fieldGetMethod = fieldInfo.GetMethod!;

                if (isObjectType || isUserData) {
                    if (field.array) { // Array of pointers.
                        var list = (IList) fieldGetMethod.Invoke(this, null)!;
                        foreach (var obj in list) {
                            ((RszObject) obj).WriteObjectList(objectList);
                        }
                    } else { // Pointer to object.
                        // So it works in the UI, we always put the object in a list. Thus, even if not an array, we need to extract from a list.
                        var list = (IList) fieldGetMethod.Invoke(this, null)!;
                        if (list.Count > 0) {
                            ((RszObject) list[0]!).WriteObjectList(objectList);
                        }
                    }
                }
            }
        }

        if (TryGetMatchingUserDataEntry(objectList, out _)) return;

        objectList.Add(this);
    }

    private bool TryGetMatchingUserDataEntry(IList<RszObject> objectList, out int index) {
        if (this is not UserDataShell) {
            index = -1;
            return false;
        }
        for (var i = 0; i < objectList.Count; i++) {
            if (objectList[i].userDataRef == userDataRef) {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }

    public override string? ToString() {
        return structInfo.name;
    }

    private static object ReadTuple(BinaryReader reader, StructJson.Field field) {
        field.twoGenericsInfo ??= new(field);
        var twoGenericsInfo = field.twoGenericsInfo!.Value;
        var item1           = ReadTupleItem(reader, twoGenericsInfo.type1);
        var item2           = ReadTupleItem(reader, twoGenericsInfo.type2);
        var item1Type       = twoGenericsInfo.type1.AsArrayType;
        var item2Type       = twoGenericsInfo.type2.AsArrayType;
        var tupleType       = typeof(ValueTuple<,>).MakeGenericType(item1Type, item2Type);
        var obj             = Activator.CreateInstance(tupleType, item1, item2)!;
        return obj;
    }

    private static object ReadTupleItem(BinaryReader reader, TwoGenericsInfo.GenericTypeInfo typeInfo) {
        if (typeInfo.isArray) {
            var count = reader.ReadInt32();
            var items = Activator.CreateInstance(typeInfo.AsArrayType)!;
            for (var i = 0; i < count; i++) {
                var item = reader.Read(typeInfo.convertedName);
                ((dynamic) items).Add((dynamic) item); // Ugly, but does the job.
            }
            return items;
            // ReSharper disable once RedundantIfElseBlock
        } else {
            var item = reader.Read(typeInfo.convertedName);
            return item;
        }
    }

    private static void WriteTuple(BinaryWriter writer, StructJson.Field field, ITuple obj) {
        field.twoGenericsInfo ??= new(field);
        var twoGenericsInfo = field.twoGenericsInfo!.Value;
        WriteTupleItem(writer, twoGenericsInfo.type1, obj[0]!);
        WriteTupleItem(writer, twoGenericsInfo.type2, obj[1]!);
    }

    private static void WriteTupleItem(BinaryWriter writer, TwoGenericsInfo.GenericTypeInfo typeInfo, object item) {
        if (typeInfo.isArray) {
            var list = (IList) item;
            writer.Write(list.Count);
            foreach (var obj in list) {
                writer.WriteGeneric(typeInfo.convertedName, obj);
            }
        } else {
            writer.WriteGeneric(typeInfo.convertedName, item);
        }
    }
}

public static class RszObjectExtensions {
    public static int GetAlign(this StructJson.Field field) {
        return field.array ? 4 : field.align;
    }

    public static Type? AsType(this string? typeName) {
        return typeName == null ? null : Type.GetType("RE_Editor.Common.Structs." + typeName, true);
    }

    public static List<T> GetDataAsList<T>(this byte[] bytes, int size, int arrayCount, StructJson.Field field) {
        var genericArgs = typeof(T).GenericTypeArguments;
        var isGeneric   = genericArgs.Length != 0;
        var genericType = isGeneric ? genericArgs[0] : null;
        var list        = new List<T>(arrayCount);

        for (var i = 0; i < arrayCount; i++) {
            var startPos = i * size;
            var sub      = bytes.Subsequence(startPos, size);
            if (isGeneric) {
                //var getDataAs = typeof(Extensions).GetMethod(nameof(Extensions.GetDataAs), BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, new[] {typeof(IEnumerable<byte>)}, null)
                //                                  ?.MakeGenericMethod(genericType!);
                //var data = getDataAs?.Invoke(null, new object[] {sub}) ?? throw new("sub.GetDataAs failure.");
                var data = sub.GetDataAs(genericType!);
                T   wrapper;
                if (typeof(T).GetGenericTypeDefinition().Is(typeof(DataSourceWrapper<>))) {
                    wrapper = (T) Activator.CreateInstance(typeof(T), i, data, field)!;
                } else {
                    wrapper = (T) Activator.CreateInstance(typeof(T), i, data)!;
                }
                list.Add(wrapper);
            } else {
                list.Add(sub.GetDataAs<T>());
            }
        }

        return list;
    }
}