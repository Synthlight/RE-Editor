using RE_Editor.Common;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Models;
using RE_Editor.Common.Structs;
using RE_Editor.Generator.Models;
using Color = RE_Editor.Common.Structs.Color;
using Guid = RE_Editor.Common.Structs.Guid;

#if DD2
using RE_Editor.Common.Data;
#elif DRDR
using RE_Editor.Common.Data;
#elif MHR
using RE_Editor.Common.Data;
#elif MHWS
using RE_Editor.Common.Data;
#elif RE2
using RE_Editor.Common.Data;
#elif RE3
using RE_Editor.Common.Data;
#elif RE4
using RE_Editor.Common.Data;
#endif

namespace RE_Editor.Generator;

public class StructTemplate(GenerateFiles generator, StructType structType) {
    public readonly  uint                    hash        = structType.hash;
    public readonly  StructJson              structInfo  = structType.structInfo;
    private readonly string                  className   = structType.name;
    private readonly Dictionary<string, int> usedNames   = [];
    private          int                     sortOrder   = 1000;
    private readonly string?                 parentClass = structType.parent;

    public void Generate(bool dryRun) {
        var       filename = $@"{GenerateFiles.STRUCT_GEN_PATH}\{className}.cs";
        using var file     = new StreamWriter(dryRun ? new MemoryStream() : File.Open(filename, FileMode.Create, FileAccess.Write));

        WriteUsings(file);
        WriteClassHeader(file);
        if (structInfo.fields != null) {
            foreach (var field in structInfo.fields) {
                WriteProperty(file, field);
            }
        }
        WriteClassCreate(file);
        WriteClassCopy(file);
        WriteClassFooter(file);
        file.Flush();
        file.Close();
    }

    private static void WriteUsings(TextWriter file) {
        file.WriteLine("// ReSharper disable All");
        file.WriteLine("using System.Collections;");
        file.WriteLine("using System.Collections.ObjectModel;");
        file.WriteLine("using System.ComponentModel;");
        file.WriteLine("using System.Diagnostics.CodeAnalysis;");
        file.WriteLine("using System.Globalization;");
        file.WriteLine("using RE_Editor.Common;");
        file.WriteLine("using RE_Editor.Common.Attributes;");
        file.WriteLine("using RE_Editor.Common.Controls.Models;");
        file.WriteLine("using RE_Editor.Common.Data;");
        file.WriteLine("using RE_Editor.Common.Models;");
        file.WriteLine("using RE_Editor.Common.Models.List_Wrappers;");
        file.WriteLine("using RE_Editor.Common.Structs;");
        file.WriteLine("using RE_Editor.Models.Enums;");
        file.WriteLine("using DateTime = RE_Editor.Common.Structs.DateTime;");
        file.WriteLine("using Guid = RE_Editor.Common.Structs.Guid;");
        file.WriteLine("using Range = RE_Editor.Common.Structs.Range;");
        file.WriteLine("using Size = RE_Editor.Common.Structs.Size;");
    }

    private void WriteClassHeader(TextWriter file) {
        file.WriteLine("");
        file.WriteLine("#pragma warning disable CS8600");
        file.WriteLine("#pragma warning disable CS8601");
        file.WriteLine("#pragma warning disable CS8602");
        file.WriteLine("#pragma warning disable CS8603");
        file.WriteLine("#pragma warning disable CS8618");
        file.WriteLine("");
        file.WriteLine("namespace RE_Editor.Models.Structs;");
        file.WriteLine("");
        file.WriteLine("[SuppressMessage(\"ReSharper\", \"InconsistentNaming\")]");
        file.WriteLine("[SuppressMessage(\"ReSharper\", \"UnusedMember.Global\")]");
        file.WriteLine("[SuppressMessage(\"ReSharper\", \"ClassNeverInstantiated.Global\")]");
        file.WriteLine("[SuppressMessage(\"ReSharper\", \"IdentifierTypo\")]");
        file.WriteLine("[SuppressMessage(\"CodeQuality\", \"IDE0079:Remove unnecessary suppression\")]");
        file.WriteLine("[MhrStruct]");
        file.WriteLine($"// {structInfo.name}");
        file.WriteLine($"public partial class {className} : {parentClass ?? nameof(RszObject)} {{");
        file.WriteLine($"    public {(parentClass == null ? "const" : "new const")} uint HASH = 0x{hash:X};");
    }

    private void WriteProperty(TextWriter file, StructJson.Field field) {
        if (GenerateFiles.UNSUPPORTED_DATA_TYPES.Contains(field.type!)) return;
        if (GenerateFiles.UNSUPPORTED_OBJECT_TYPES.Any(s => field.originalType!.Contains(s))) return;

        // Happened for some RE4 types.
        if (field.originalType == "") {
            var convertedTypeName = field.GetCSharpType();
            if (convertedTypeName != null) {
                field.originalType = convertedTypeName;
            }

            if (field.originalType == "") {
                Global.Log($"Warning: Unknown originalType, skipping: {structInfo.name}::{field.name}");
                return;
            }
        }

        var newName             = field.name?.ToConvertedFieldName()!;
        var primitiveName       = field.GetCSharpType();
        var typeName            = field.originalType!.ToConvertedTypeName();
        var isPrimitive         = primitiveName != null;
        var isEnumType          = typeName != null && generator.enumTypes.ContainsKey(typeName);
        var buttonType          = field.buttonType;
        var buttonPrimitive     = field.buttonPrimitive;
        var isNonPrimitive      = !isPrimitive && !isEnumType; // via.thing
        var isUserData          = field.type == "UserData";
        var isStruct            = field.type == "Struct";
        var isObjectType        = field.type == nameof(Object);
        var viaType             = GetViaType(field, isNonPrimitive, typeName, ref isObjectType, isUserData);
        var negativeOneForEmpty = GetNegativeForEmptyAllowed(field);
        var modifier            = ""; // `override` or `virtual`

        if (field.overrideCount > 0) modifier     = "override ";
        else if (field.virtualCount > 0) modifier = "virtual ";

        // Dirty hack for MHR. It inherits from the normal base, but there's no defined size to breakout fields from.
        // So just show the raw data list.
        if (className == "Snow_BitSetFlagNoEnum") modifier = "new ";

        if (!usedNames.TryAdd(newName, 1)) {
            usedNames[newName]++;
            newName += usedNames[newName].ToString();
        }

        file.WriteLine("");
        file.WriteLine($"    // {field.name}");
        file.WriteLine($"    // {field.originalType}");
        if (typeName!.StartsWith("System_ValueTuple")) {
            if (field.twoGenericsInfo == null) {
                // TODO: Make a better system to parse whatever generic args it actually has.
                // (Incl. handling that types might be primitives using the `System.X` name.)
                return;
            }
            var twoGenericsInfo = field.twoGenericsInfo!.Value;
            file.WriteLine($"    [SortOrder({sortOrder})]");
            if (field.array) {
                file.WriteLine("    [IsList]");
            }
            file.WriteLine($"    public {modifier}ObservableCollection<ValueTuple<{twoGenericsInfo.type1.AsArrayTypeName}, {twoGenericsInfo.type2.AsArrayTypeName}>> {newName} {{ get; set; }}");
        } else if (newName == Global.BITSET_FIELD_NAME && (className == Global.BITSET_NAME || parentClass == Global.BITSET_NAME) && !className.EndsWith("NoEnum")) {
            file.WriteLine($"    public {modifier}BitArray {newName} {{ get; set; }}");
            file.WriteLine("    [DisplayName(\"\")]");
            file.WriteLine($"    public {modifier}int {newName}_Length {{ get; set; }}");

            if (parentClass == Global.BITSET_NAME) {
                var enumType   = structInfo.GetGenericParam()?.ToConvertedTypeName()!;
                var enumData   = generator.enumTypes[enumType];
                var entries    = enumData.entries!;
                var correction = "";

                // Enums that start with -1 wind up being off-by-one in the UI, but ones that start with 0 are fine.
                // The solution is just +1 where it's all accessed to offset it so the first generated property equates to 0.
                if (enumData.values![0].StartsWith('-')) {
                    var negValue = int.Parse(enumData.values![0]);
                    var offBy    = Math.Abs(negValue);
                    correction = $" + {offBy}";
                }

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < entries.Count; i++) {
                    var entry = entries[i];
                    file.WriteLine("");
                    file.WriteLine($"    [DisplayName(\"{entry}\")]");
                    file.WriteLine($"    [BitIndex((int) {enumType}.{entry}{correction})]");
                    file.WriteLine($"    public bool {enumType}_{entry} {{");
                    file.WriteLine($"        get => {Global.BITSET_FIELD_NAME}[(int) {enumType}.{entry}{correction}];");
                    file.WriteLine($"        set => {Global.BITSET_FIELD_NAME}[(int) {enumType}.{entry}{correction}] = value;");
                    file.WriteLine("    }");
                }
            }
        } else if (field.type == nameof(UIntArray)) {
            file.WriteLine("    [IsList]");
            file.WriteLine($"    public {modifier}ObservableCollection<{nameof(UIntArray)}> {newName} {{ get; set; }}");
        } else if (field.array) {
            file.WriteLine($"    [SortOrder({sortOrder})]");
            if (buttonType != null) {
                if (primitiveName == null) {
                    throw new InvalidDataException("Button type found but primitiveName is null.");
                }
                file.WriteLine($"    [DataSource({nameof(DataSourceType)}.{buttonType})]");
                foreach (var additionalAttributes in GetAdditionalAttributesForDataSourceType(buttonType)) {
                    file.WriteLine($"    {additionalAttributes}");
                }
                file.WriteLine("    [IsList]");
                file.WriteLine($"    public {modifier}ObservableCollection<DataSourceWrapper<{primitiveName}>> {newName} {{ get; set; }}");
            } else if (isUserData) {
                file.WriteLine("    [IsList]");
                file.WriteLine($"    public {modifier}ObservableCollection<{nameof(UserDataShell)}> {newName} {{ get; set; }}");
            } else if (isNonPrimitive && viaType != null) {
                file.WriteLine("    [IsList]");
                file.WriteLine($"    public {modifier}ObservableCollection<{viaType}> {newName} {{ get; set; }}");
            } else if (isObjectType || isStruct) {
                file.WriteLine("    [IsList]");
                file.WriteLine($"    public {modifier}ObservableCollection<{typeName}> {newName} {{ get; set; }}");
            } else if (isEnumType) {
                file.WriteLine("    [IsList]");
                file.WriteLine($"    public {modifier}ObservableCollection<GenericWrapper<{typeName}>> {newName} {{ get; set; }}");
            } else if (isPrimitive) {
                file.WriteLine("    [IsList]");
                file.WriteLine($"    public {modifier}ObservableCollection<GenericWrapper<{primitiveName}>> {newName} {{ get; set; }}");
            } else {
                throw new InvalidDataException("Not a primitive, enum, or object array type.");
            }
        } else {
            if (viaType == nameof(Color)) {
                // Unwrap color fields.

                file.WriteLine($"    [SortOrder({sortOrder})]");
                file.WriteLine("    [DisplayName(\"\")]");
                file.WriteLine($"    public {modifier}ObservableCollection<{viaType}> {newName} {{ get; set; }}");

                file.WriteLine("");
                file.WriteLine($"    [SortOrder({sortOrder + 5})]");
                file.WriteLine($"    [DisplayName(\"{newName}\")]");
                file.WriteLine($"    public {modifier}{viaType} {newName}_Unwrapped {{");
                file.WriteLine("        get {");
                file.WriteLine($"            return {newName}[0];");
                file.WriteLine("        }");
                file.WriteLine("        set {");
                file.WriteLine($"             {newName}[0] = value;");
                file.WriteLine("        }");
                file.WriteLine("    }");
            } else if (PathHelper.CONFIG_NAME == "MHWS" && isObjectType && typeName?.EndsWith("_Serializable") == true && generator.structTypes[typeName].structInfo.fields![0].type != nameof(Object)) {
                // Special case for MHWS's fucked enum wrapping nonsense.
                var unwrappedName      = $"{newName}_Unwrapped";
                var unwrappedType      = typeName.ToFixedEnumName();
                var wrappedStructInfo  = generator.structTypes[typeName];
                var valuePrimitiveName = wrappedStructInfo.structInfo.fields![0].GetCSharpType(); // Being a wrapper for enums, it must have a primitive type.

                file.WriteLine($"    [SortOrder({sortOrder})]");
                file.WriteLine("    [DisplayName(\"\")]");
                file.WriteLine($"    public {modifier}ObservableCollection<{typeName}> {newName} {{ get; set; }}");
                file.WriteLine("");
                file.WriteLine($"    [SortOrder({sortOrder + 5})]");
                file.WriteLine($"    [DisplayName(\"{newName}\")]");

                if (buttonType != null) {
                    file.WriteLine($"    [DataSource({nameof(DataSourceType)}.{buttonType})]");
                }

                file.WriteLine($"    public {modifier}{unwrappedType} {unwrappedName} {{");
                file.WriteLine("        get {");
                file.WriteLine($"            return ({unwrappedType}) {newName}[0].Value;");
                file.WriteLine("        }");
                file.WriteLine("        set {");
                file.WriteLine($"             {newName}[0].Value = ({valuePrimitiveName}) value;");
                file.WriteLine("        }");
                file.WriteLine("    }");

                if (buttonType != null) {
                    var lookupName = GetLookupForDataSourceType(buttonType);
                    file.WriteLine("");
                    file.WriteLine($"    [SortOrder({sortOrder})]");
                    file.WriteLine($"    [CustomSorter(typeof(ButtonSorter))]");
                    file.WriteLine($"    [DisplayName(\"{newName}\")]");
                    file.WriteLine($"    public {modifier}string {unwrappedName}_button => {(negativeOneForEmpty ? $"{unwrappedName} == -1 ? \"<None>\".ToStringWithId({unwrappedName}) : " : "")}" +
                                   $"DataHelper.{lookupName}[Global.locale].TryGet(({buttonPrimitive}) {unwrappedName}).ToStringWithId({unwrappedName});");
                }
            } else if (buttonType != null) { //  && field.name != "_Id" -- Not sure which needed this? Breaks for DD2 stuff like item drop params.
                var lookupName = GetLookupForDataSourceType(buttonType);
                file.WriteLine($"    [SortOrder({sortOrder + 10})]");
                file.WriteLine($"    [DataSource({nameof(DataSourceType)}.{buttonType})]");
                if (negativeOneForEmpty) file.WriteLine("    [NegativeOneForEmpty]");
                file.WriteLine($"    public {modifier}{primitiveName} {newName} {{ get; set; }}");
                file.WriteLine("");
                file.WriteLine($"    [SortOrder({sortOrder})]");
                file.WriteLine($"    [CustomSorter(typeof(ButtonSorter))]");
                file.WriteLine($"    [DisplayName(\"{newName}\")]");
#if MHR
                file.WriteLine($"    public {modifier}string {newName}_button => DataHelper.{lookupName}[Global.locale].TryGet(({buttonPrimitive}) {newName}).ToStringWithId({newName}{(buttonType == DataSourceType.ITEMS ? ", true" : "")});");
#elif RE4
                file.WriteLine($"    public {modifier}string {newName}_button => {(negativeOneForEmpty ? $"{newName} == -1 ? \"<None>\".ToStringWithId({newName}) : " : "")}" +
                               $"DataHelper.{lookupName}[Global.variant][Global.locale].TryGet(({buttonPrimitive}) {newName}).ToStringWithId({newName}{(buttonType == DataSourceType.ITEMS ? ", true" : "")});");
#else
                file.WriteLine($"    public {modifier}string {newName}_button => {(negativeOneForEmpty ? $"{newName} == -1 ? \"<None>\".ToStringWithId({newName}) : " : "")}" +
                               $"DataHelper.{lookupName}[Global.locale].TryGet(({buttonPrimitive}) {newName}).ToStringWithId({newName});");
#endif
            } else if (viaType?.Is(typeof(ISimpleViaType)) == true) {
                file.WriteLine($"    [SortOrder({sortOrder})]");
                file.WriteLine($"    public {modifier}{viaType} {newName} {{ get; set; }}");
            } else if (isUserData) {
                file.WriteLine($"    [SortOrder({sortOrder})]");
                file.WriteLine($"    public {modifier}ObservableCollection<{nameof(UserDataShell)}> {newName} {{ get; set; }}");
            } else if (isNonPrimitive && viaType != null) {
                file.WriteLine($"    [SortOrder({sortOrder})]");
                file.WriteLine($"    public {modifier}ObservableCollection<{viaType}> {newName} {{ get; set; }}");
            } else if (isObjectType) {
                file.WriteLine($"    [SortOrder({sortOrder})]");
                file.WriteLine($"    public {modifier}ObservableCollection<{typeName}> {newName} {{ get; set; }}");
            } else if (isEnumType) {
                file.WriteLine($"    [SortOrder({sortOrder})]");
                file.WriteLine($"    public {modifier}{typeName} {newName} {{ get; set; }}");
            } else if (isPrimitive) {
                file.WriteLine($"    [SortOrder({sortOrder})]");
                file.WriteLine($"    public {modifier}{primitiveName} {newName} {{ get; set; }}");
            } else {
                throw new InvalidDataException("Not a primitive, enum, or object type.");
            }
        }

        sortOrder += 100;
    }

    private static string? GetViaType(StructJson.Field field, bool isNonPrimitive, string? typeName, ref bool isObjectType, bool isUserData) {
        // We do it here and later since we sometimes overwrite them.
        var viaType = field.originalType?.GetViaType();

        if (field.type == "Struct") return null;

        switch (typeName) {
            case "Via_Prefab":
                viaType      = nameof(Prefab);
                isObjectType = false;
                break;
            case "System_Type":
                viaType      = nameof(Type);
                isObjectType = false;
                break;
            case "Via_OBB":
                viaType      = nameof(UIntArray);
                isObjectType = false;
                break;
            default: {
                if (isNonPrimitive && !isObjectType && !isUserData) {
                    // This makes sure we've implemented the via type during generation.
                    viaType = field.type!.GetViaType() ?? throw new NotImplementedException($"Hard-coded type '{field.type}' not implemented.");
                }
                break;
            }
        }
        return viaType;
    }

    private void WriteClassCreate(TextWriter file) {
        if (className.StartsWith("Via_")) return;

        var isParentViaType = parentClass?.ToLower().StartsWith("via") ?? false;
        var modifier        = parentClass == null || isParentViaType ? "" : "new ";
        var usedNamesLocal  = new Dictionary<string, int>();

        file.WriteLine("");
        file.WriteLine($"    public {modifier}static {className} Create(RSZ rsz) {{");
        file.WriteLine($"        var obj = Create<{className}>(rsz, HASH);");

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var field in structInfo.fields!) {
            if (string.IsNullOrEmpty(field.name) || string.IsNullOrEmpty(field.originalType)) continue;
            if (GenerateFiles.UNSUPPORTED_DATA_TYPES.Contains(field.type!)) continue;
            if (GenerateFiles.UNSUPPORTED_OBJECT_TYPES.Any(s => field.originalType!.Contains(s))) continue;

            var newName        = field.name?.ToConvertedFieldName()!;
            var primitiveName  = field.GetCSharpType();
            var typeName       = field.originalType!.ToConvertedTypeName();
            var isPrimitive    = primitiveName != null;
            var isEnumType     = typeName != null && generator.enumTypes.ContainsKey(typeName);
            var buttonType     = field.buttonType;
            var isNonPrimitive = !isPrimitive && !isEnumType; // via.thing
            var isUserData     = field.type == "UserData";
            var isStruct       = field.type == "Struct";
            var isObjectType   = field.type == nameof(Object);
            var viaType        = GetViaType(field, isNonPrimitive, typeName, ref isObjectType, isUserData);

            if (!usedNamesLocal.TryAdd(newName, 1)) {
                usedNamesLocal[newName]++;
                newName += usedNamesLocal[newName].ToString();
            }

            if (typeName!.StartsWith("System_ValueTuple") && field.twoGenericsInfo == null) {
                // TODO: Make a better system to parse whatever generic args it actually has.
                // (Incl. handling that types might be primitives using the `System.X` name.)
                continue;
            }

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (newName == Global.BITSET_FIELD_NAME && className == Global.BITSET_NAME && !className.EndsWith("NoEnum")) {
                file.WriteLine($"        obj.{newName} = new(1234);"); // There's no enum data to work with. I'm just using a random value here.
            } else if (newName == Global.BITSET_FIELD_NAME && parentClass == Global.BITSET_NAME && !className.EndsWith("NoEnum")) {
                var enumType = structInfo.GetGenericParam()?.ToConvertedTypeName()!;
                var enumData = generator.enumTypes[enumType];
                file.WriteLine($"        obj.{newName} = new({enumData.EntryCount});"); // Should really be `MaxElement`, but it's static so...
            } else if (!field.array && field.originalType.StartsWith("snow.Bitset`1<System")) {
                file.WriteLine($"        obj.{newName} = new(new());"); // GenericWrapper
            } else if (!field.array && isObjectType && viaType == null && typeName != null && !isEnumType) {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (typeName.StartsWith("Via")) { // For things like `Via_AnimationCurve` which generate from the json but aren't our manually implemented `via` types.
                    file.WriteLine($"        obj.{newName} = [new()];");
                } else {
                    file.WriteLine($"        obj.{newName} = [{typeName}.Create(rsz)];");
                }
            } else if (viaType?.Is(typeof(ISimpleViaType)) == true
                       || isUserData
                       || (isNonPrimitive && viaType != null)
                       || isObjectType) {
                if (!field.array && viaType != null
                                 && !viaType.Is(typeof(ISimpleViaType))
                                 && viaType != nameof(Type)
                                 && viaType != nameof(Prefab)
                                 && viaType != nameof(UIntArray)) {
                    file.WriteLine($"        obj.{newName} = [new()];");
                } else {
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (!field.array && viaType == nameof(Guid)) {
                        file.WriteLine($"        obj.{newName} = Guid.New();");
                    } else {
                        file.WriteLine($"        obj.{newName} = new();");
                    }
                }
            } else if (isEnumType && !field.array && buttonType == null) {
                file.WriteLine($"        obj.{newName} = Enum.GetValues<{typeName}>()[0];");
            } else if ((isEnumType || isPrimitive) && field.array) {
                file.WriteLine($"        obj.{newName} = new(new());"); // GenericWrapper
            } else if (isStruct && field.array) {
                file.WriteLine($"        obj.{newName} = new();");
            }
        }

        file.WriteLine("        return obj;");
        file.WriteLine("    }");
    }

    private void WriteClassCopy(TextWriter file) {
        if (className.StartsWith("Via_")) return;

        var isParentViaType = parentClass?.ToLower().StartsWith("via") ?? false;
        var modifier        = parentClass == null || isParentViaType ? "virtual " : "override ";
        var usedNamesLocal  = new Dictionary<string, int>();

        file.WriteLine("");
        file.WriteLine($"    public {modifier}{className} Copy() {{");

        file.WriteLine($"        var obj = base.Copy<{className}>();");

        foreach (var field in structInfo.fields!) {
            if (string.IsNullOrEmpty(field.name) || string.IsNullOrEmpty(field.originalType)) continue;
            if (GenerateFiles.UNSUPPORTED_DATA_TYPES.Contains(field.type!)) continue;
            if (GenerateFiles.UNSUPPORTED_OBJECT_TYPES.Any(s => field.originalType!.Contains(s))) continue;

            var newName        = field.name?.ToConvertedFieldName()!;
            var primitiveName  = field.GetCSharpType();
            var typeName       = field.originalType!.ToConvertedTypeName();
            var isPrimitive    = primitiveName != null;
            var isEnumType     = typeName != null && generator.enumTypes.ContainsKey(typeName);
            var buttonType     = field.buttonType;
            var isNonPrimitive = !isPrimitive && !isEnumType; // via.thing
            var isUserData     = field.type == "UserData";
            var isStruct       = field.type == "Struct";
            var isObjectType   = field.type == nameof(Object);
            var viaType        = GetViaType(field, isNonPrimitive, typeName, ref isObjectType, isUserData);

            if (!usedNamesLocal.TryAdd(newName, 1)) {
                usedNamesLocal[newName]++;
                newName += usedNamesLocal[newName].ToString();
            }

            // TODO: Fix generic/dataSource wrappers.

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (typeName!.StartsWith("System_ValueTuple")) {
                if (field.twoGenericsInfo == null) {
                    // TODO: Make a better system to parse whatever generic args it actually has.
                    // (Incl. handling that types might be primitives using the `System.X` name.)
                    continue;
                }
                file.WriteLine($"        foreach (var x in {newName}) {{");
                var twoGenericsInfo = field.twoGenericsInfo!.Value;

                var item1 = "x.Item1";
                if (twoGenericsInfo.type1.isArray) {
                    item1 = $"new({item1})";
                }

                var item2 = "x.Item2";
                if (twoGenericsInfo.type2.isArray) {
                    item2 = $"new({item2})";
                }

                file.WriteLine($"            obj.{newName}.Add(new({item1}, {item2}));");
                file.WriteLine("        }");
            } else if (newName == Global.BITSET_FIELD_NAME && (className == Global.BITSET_NAME || parentClass == Global.BITSET_NAME) && !className.EndsWith("NoEnum")) {
                file.WriteLine($"        obj.{newName} = new({newName});");
            } else if (!field.array && viaType?.Is(typeof(ISimpleViaType)) == true) {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (!field.array && viaType == nameof(Guid)) {
                    file.WriteLine($"        obj.{newName} = {newName}.Copy();");
                } else {
                    file.WriteLine($"        obj.{newName} = new();");
                }
            } else if ((field.array || isObjectType || isStruct || isNonPrimitive) && buttonType == null) {
                file.WriteLine($"        obj.{newName} ??= new();");
                file.WriteLine($"        foreach (var x in {newName}) {{");
                if (typeName == "System_Type" || viaType == "Type") { // `Type` is a built-in type, no copy.
                    file.WriteLine($"            obj.{newName}.Add(x);");
                } else {
                    if (viaType?.Is(typeof(ISimpleViaType)) == true) {
                        file.WriteLine($"            obj.{newName}.Add(x.Copy());");
                    } else if (isObjectType && viaType == null && isNonPrimitive && typeName?.Contains("GenericWrapper") == false) {
                        file.WriteLine($"            obj.{newName}.Add({(isEnumType ? "x" : $"x.Copy<{typeName}>()")});");
                    } else {
                        file.WriteLine($"            obj.{newName}.Add({(isEnumType ? "x" : "x.Copy()")});");
                    }
                }
                file.WriteLine("        }");
            } else {
                file.WriteLine($"        obj.{newName} = {newName};");
            }
        }

        file.WriteLine("        return obj;");
        file.WriteLine("    }");
    }

    private static void WriteClassFooter(TextWriter file) {
        file.Write("}");
    }

    private static List<string> GetAdditionalAttributesForDataSourceType(DataSourceType? dataSourceType) {
        return dataSourceType switch {
#if MHR
            DataSourceType.ITEMS => ["[ButtonIdAsHex]"],
#elif RE4
            DataSourceType.ITEMS => ["[ButtonIdAsHex]"],
            DataSourceType.WEAPONS => ["[ButtonIdAsHex]"],
#endif
            _ => []
        };
    }

    public static string GetLookupForDataSourceType(DataSourceType? dataSourceType) {
        return dataSourceType switch {
#if DD2
            DataSourceType.ITEMS => nameof(DataHelper.ITEM_NAME_LOOKUP),
#elif DRDR
            DataSourceType.ITEMS => nameof(DataHelper.ITEM_NAME_LOOKUP),
#elif MHR
            DataSourceType.DANGO_SKILLS => nameof(DataHelper.DANGO_SKILL_NAME_LOOKUP),
            DataSourceType.ITEMS => nameof(DataHelper.ITEM_NAME_LOOKUP),
            DataSourceType.RAMPAGE_SKILLS => nameof(DataHelper.RAMPAGE_SKILL_NAME_LOOKUP),
            DataSourceType.SKILLS => nameof(DataHelper.SKILL_NAME_LOOKUP),
            DataSourceType.SWITCH_SKILLS => nameof(DataHelper.SWITCH_SKILL_NAME_LOOKUP),
#elif MHWS
            DataSourceType.ARMOR_SERIES => nameof(DataHelper.ARMOR_SERIES_BY_ENUM_VALUE),
            DataSourceType.DECORATIONS => nameof(DataHelper.DECORATION_INFO_LOOKUP_BY_ENUM_VALUE),
            DataSourceType.DLC => nameof(DataHelper.DLC_NAME_LOOKUP_BY_ENUM_VALUE),
            DataSourceType.ENEMIES => nameof(DataHelper.ENEMY_NAME_LOOKUP_BY_ENUM_VALUE),
            DataSourceType.GIMMICKS => nameof(DataHelper.GIMMICK_NAME_LOOKUP_BY_ENUM_VALUE),
            DataSourceType.ITEMS => nameof(DataHelper.ITEM_NAME_LOOKUP),
            DataSourceType.MEDALS => nameof(DataHelper.MEDAL_NAME_LOOKUP_BY_ENUM_VALUE),
            DataSourceType.MENU => nameof(DataHelper.MENU_NAME_LOOKUP_BY_ENUM_VALUE),
            DataSourceType.NPCS => nameof(DataHelper.NPC_NAME_LOOKUP_BY_ENUM_VALUE),
            DataSourceType.OTOMO_SERIES => nameof(DataHelper.OTOMO_SERIES_BY_ENUM_VALUE),
            DataSourceType.QUESTS => nameof(DataHelper.QUEST_INFO_LOOKUP_BY_ENUM_VALUE),
            DataSourceType.PENDANTS => nameof(DataHelper.PENDANT_NAME_LOOKUP_BY_ENUM_VALUE),
            DataSourceType.SKILLS => nameof(DataHelper.SKILL_NAME_BY_ENUM_VALUE),
            DataSourceType.WEAPON_SERIES => nameof(DataHelper.WEAPON_SERIES_BY_ENUM_VALUE),
#elif RE2
            DataSourceType.ITEMS => nameof(DataHelper.ITEM_NAME_LOOKUP),
            DataSourceType.WEAPONS => nameof(DataHelper.WEAPON_NAME_LOOKUP),
#elif RE3
            DataSourceType.ITEMS => nameof(DataHelper.ITEM_NAME_LOOKUP),
            DataSourceType.WEAPONS => nameof(DataHelper.WEAPON_NAME_LOOKUP),
#elif RE4
            DataSourceType.ITEMS => nameof(DataHelper.ITEM_NAME_LOOKUP),
            DataSourceType.WEAPONS => nameof(DataHelper.WEAPON_NAME_LOOKUP),
#endif
            _ => throw new ArgumentOutOfRangeException(dataSourceType.ToString())
        };
    }

    private static bool GetNegativeForEmptyAllowed(StructJson.Field field) {
        return field.name?.ToConvertedFieldName() switch {
#if RE4
            "CurrentAmmo" => true,
#endif
            _ => false
        };
    }
}