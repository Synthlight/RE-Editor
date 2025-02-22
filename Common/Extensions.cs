using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using RE_Editor.Common.Models;
using RE_Editor.Common.Structs;

namespace RE_Editor.Common;

public static class Extensions {
    private static readonly Dictionary<string, string> VIA_TYPE_NAME_LOOKUPS = [];
    private static readonly Dictionary<string, Type>   FOUND_TYPE_CACHE      = [];

    static Extensions() {
        var viaTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                       from type in assembly.GetTypes()
                       where type != typeof(IViaType) && type.IsAssignableTo(typeof(IViaType))
                       select type.Name;
        foreach (var viaType in viaTypes) {
            VIA_TYPE_NAME_LOOKUPS[viaType.ToLower()]          = viaType;
            VIA_TYPE_NAME_LOOKUPS[$"via.{viaType}".ToLower()] = viaType;
        }
        Debug.Assert(VIA_TYPE_NAME_LOOKUPS.Count > 0);
    }

    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T> {
        if (val.CompareTo(min) < 0) {
            return min;
        } else if (val.CompareTo(max) > 0) {
            return max;
        } else {
            return val;
        }
    }

    public static bool Is(this Type source, params Type[] types) {
        return types.Any(type => type.IsAssignableFrom(source));
    }

    public static bool Is(this string source, params Type[] types) {
        var foundType = FOUND_TYPE_CACHE.GetValueOrDefault(source);
        foundType ??= (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                       from type in assembly.GetTypes()
                       where type.Name == source
                       from searchingType in types
                       where searchingType.IsAssignableFrom(type)
                       select type).FirstOrDefault();
        if (foundType != null) {
            FOUND_TYPE_CACHE[source] = foundType;
            return true;
        }
        return false;
    }

    public static T[] Subsequence<T>(this IEnumerable<T> arr, int startIndex, int length) {
        return arr.Skip(startIndex).Take(length).ToArray();
    }

    public static bool ContainsIgnoreCase(this IEnumerable<string> arr, string needle) {
        return arr.Any(s => string.Equals(s, needle, StringComparison.CurrentCultureIgnoreCase));
    }

    public static bool ContainsIgnoreCase(this string arr, string needle) {
        return arr.Contains(needle, StringComparison.CurrentCultureIgnoreCase);
    }

    public static V TryGet<K, V>(this IDictionary<K, V>? dict, K key, V defaultValue) {
        if (dict == null) return defaultValue;
        return dict.ContainsKey(key) ? dict[key] : defaultValue;
    }

    public static string TryGet<K>(this IDictionary<K, string>? dict, K key, string defaultValue = "Unknown") {
        if (dict == null) return defaultValue;
        return dict.ContainsKey(key) ? dict[key] : defaultValue;
    }

    public static T GetData<T>(this IEnumerable<byte> bytes) where T : struct {
        return bytes.GetData<T>(0, Marshal.SizeOf(default(T)));
    }

    public static T GetData<T>(this IEnumerable<byte> bytes, int offset) where T : struct {
        return bytes.GetData<T>(offset, Marshal.SizeOf(default(T)));
    }

    public static T GetData<T>(this IEnumerable<byte> bytes, int offset, int size) where T : struct {
        var subsequence = bytes.Subsequence(offset, size);
        var handle      = GCHandle.Alloc(subsequence, GCHandleType.Pinned);

        try {
            var rawDataPtr = handle.AddrOfPinnedObject();
            return (T) Marshal.PtrToStructure(rawDataPtr, typeof(T))!;
        } finally {
            handle.Free();
        }
    }

    public static string ToStringWithId<T>(this string name, T id, bool asHex = false) where T : struct {
        // ReSharper disable once InterpolatedStringExpressionIsNotIFormattable
        var s = asHex ? $"{id:X}" : id.ToString();
        return Global.showIdBeforeName ? $"{s}: {name}" : $"{name}: {s}";
    }

    public static string Sha512(this string fileName) {
        using var file = File.OpenRead(fileName);
        return file.Sha512();
    }

    public static string Sha512(this Stream stream) {
        using var sha512 = System.Security.Cryptography.SHA512.Create();
        var       hash   = sha512.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "");
    }

    public static string Sha512(this byte[] bytes) {
        using var sha512 = System.Security.Cryptography.SHA512.Create();
        var       hash   = sha512.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "");
    }

    public static Dictionary<K, V> Sort<K, V, O>(this Dictionary<K, V> dict, Func<KeyValuePair<K, V>, O> keySelector) where K : notnull {
        return dict.OrderBy(keySelector)
                   .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public static Dictionary<K, V> SortDescending<K, V, O>(this Dictionary<K, V> dict, Func<KeyValuePair<K, V>, O> keySelector) where K : notnull {
        return dict.OrderByDescending(keySelector)
                   .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public static Dictionary<K2, V> GetOrCreate<K1, K2, V>(this Dictionary<K1, Dictionary<K2, V>> dict, K1 key) where K1 : notnull
                                                                                                                where K2 : notnull {
        if (dict.ContainsKey(key)) return dict[key];
        dict[key] = new();
        return dict[key];
    }

    public static List<V> GetOrCreate<K, V>(this Dictionary<K, List<V>> dict, K key) where K : notnull {
        if (dict.ContainsKey(key)) return dict[key];
        dict[key] = [];
        return dict[key];
    }

    public static bool IsGeneric(this Type? source, Type genericType) {
        while (source != null && source != typeof(object)) {
            var cur = source.IsGenericType ? source.GetGenericTypeDefinition() : source;
            if (genericType == cur) {
                return true;
            }

            if (source.GetInterfaces().Any(@interface => @interface.IsGeneric(genericType))) return true;

            source = source.BaseType;
        }

        return false;
    }

    public static bool IsSigned(this Type type) {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return Type.GetTypeCode(type) switch {
            TypeCode.Byte => false,
            TypeCode.UInt16 => false,
            TypeCode.UInt32 => false,
            TypeCode.UInt64 => false,
            TypeCode.SByte => true,
            TypeCode.Int16 => true,
            TypeCode.Int32 => true,
            TypeCode.Int64 => true,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static T GetDataAs<T>(this IEnumerable<byte> bytes) {
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        try {
            var type                   = typeof(T);
            if (typeof(T).IsEnum) type = typeof(T).GetEnumUnderlyingType();

            return (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type)!;
        } finally {
            if (handle.IsAllocated) {
                handle.Free();
            }
        }
    }

    public static object GetDataAs(this IEnumerable<byte> bytes, Type type) {
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        try {
            var isEnum                  = type.IsEnum;
            var deserializeType         = type;
            if (isEnum) deserializeType = type.GetEnumUnderlyingType();

            var data = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), deserializeType)!;

            return isEnum ? Enum.ToObject(type, data) : data;
        } finally {
            if (handle.IsAllocated) {
                handle.Free();
            }
        }
    }

    public static byte[] GetBytes<T>(this T @struct) where T : notnull {
        if (@struct is bool b) {
            return [(byte) (b ? 1 : 0)];
        }

        var type = @struct.GetType();
        if (type.IsEnum) {
            var enumType        = type.GetEnumUnderlyingType();
            var valueAsEnumType = Convert.ChangeType(@struct, enumType);
            var getBytes        = typeof(Extensions).GetMethod(nameof(GetBytes), BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)?.MakeGenericMethod(enumType);
            var data            = (byte[]) (getBytes?.Invoke(null, [valueAsEnumType]) ?? throw new("sub.GetDataAs failure."));
            return data;
        }

        var size   = Marshal.SizeOf(@struct);
        var bytes  = new byte[size];
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        try {
            Marshal.StructureToPtr(@struct, handle.AddrOfPinnedObject(), false);
            return bytes;
        } finally {
            if (handle.IsAllocated) {
                handle.Free();
            }
        }
    }

    public static string ReadNullTermString(this BinaryReader reader) {
        var stringBytes = new List<byte>();
        do {
            stringBytes.Add(reader.ReadByte());
        } while (stringBytes[^1] != 0);

        return Encoding.UTF8.GetString(stringBytes.Subsequence(0, stringBytes.Count).ToArray());
    }

    public static string ReadNullTermWString(this BinaryReader reader, long offsetFromBeginning, bool stripNullTerm = true) {
        var pos = reader.BaseStream.Position;
        reader.BaseStream.Position = offsetFromBeginning;
        var @string = reader.ReadNullTermWString(stripNullTerm);
        reader.BaseStream.Position = pos;
        return @string;
    }

    public static string ReadNullTermWString(this BinaryReader reader, bool stripNullTerm = true) {
        var stringBytes = new List<byte>();
        do {
            stringBytes.AddRange(reader.ReadBytes(2));
        } while (stringBytes[^1] != 0 || stringBytes[^2] != 0);

        return Encoding.Unicode.GetString(stringBytes.Subsequence(0, stringBytes.Count - (stripNullTerm ? 2 : 0)).ToArray());
    }

    public static void WriteNullTermWString(this BinaryWriter writer, string str) {
        var bytes = Encoding.Unicode.GetBytes(str);
        writer.Write(bytes);
        writer.Write(new byte[] {0, 0}); // Null termination of Unicode which is 2 zero bytes.
    }

    public static string? ReadWString(this BinaryReader reader, bool stripNullTerm = true) {
        var size = reader.ReadInt32() * 2;
        if (size == 0) return null;
        var bytes = reader.ReadBytes(size);
        return Encoding.Unicode.GetString(bytes.Subsequence(0, bytes.Length - (stripNullTerm ? 2 : 0)).ToArray());
    }

    public static void WriteWString(this BinaryWriter writer, string? str, bool stripNullTerm = true) {
        if (str == null) {
            writer.Write(0);
            return;
        }
        var bytes = Encoding.Unicode.GetBytes(str);
        writer.Write(bytes.Length / 2 + 1); // +1 for the null bytes.
        writer.Write(bytes);
        writer.Write(new byte[] {0, 0}); // Null termination of Unicode which is 2 zero bytes.
    }

    public static char[] ToNullTermCharArray(this string? str) {
        str ??= "\0";
        if (!str.EndsWith("\0")) str += "\0";
        return str.ToCharArray();
    }

    public static List<byte> ReadRemainderAsByteArray(this BinaryReader reader) {
        var list = new List<byte>();
        while (reader.BaseStream.Position < reader.BaseStream.Length) {
            list.Add(reader.ReadByte());
        }
        return list;
    }

    /// <summary>
    /// Made for reading 16 bytes of "data" for unknown underlying type.
    /// </summary>
    public static ObservableCollection<Vec4> ReadVec4Array(this BinaryReader reader) {
        ObservableCollection<Vec4> v = [];
        reader.BaseStream.Align(4);
        var count = reader.ReadInt32();
        if (count > 0) {
            reader.BaseStream.Align(16);
            for (var i = 0; i < count; i++) {
                var vec4 = new Vec4();
                vec4.Read(reader);
                v.Add(vec4);
            }
        }
        return v;
    }

    /// <summary>
    /// Made for writing 16 bytes of "data" for unknown underlying type.
    /// </summary>
    public static void WriteVec4Array(this BinaryWriter writer, ObservableCollection<Vec4> v) {
        writer.BaseStream.Align(4);
        writer.Write(v.Count);
        if (v.Count > 0) {
            writer.BaseStream.Align(16);
            foreach (var vec4 in v) {
                vec4.Write(writer);
            }
        }
    }

    public static void PadTill(this BinaryWriter writer, ulong targetPos) {
        while (writer.BaseStream.Position < (long) targetPos) {
            writer.Write((byte) 0);
        }
    }

    public static void PadTill(this BinaryWriter writer, Func<bool> predicate) {
        while (predicate.Invoke()) {
            writer.Write((byte) 0);
        }
    }

    public static void WriteValueAtOffset(this BinaryWriter writer, ulong value, long whereToWrite) {
        var tempPos = writer.BaseStream.Position;
        writer.BaseStream.Seek(whereToWrite, SeekOrigin.Begin);
        writer.Write(value);
        writer.BaseStream.Seek(tempPos, SeekOrigin.Begin);
    }

    public static void WriteCurrentPositionAtOffset(this BinaryWriter writer, long whereToWrite) {
        var tempPos = writer.BaseStream.Position;
        writer.BaseStream.Seek(whereToWrite, SeekOrigin.Begin);
        writer.Write(tempPos);
        writer.BaseStream.Seek(tempPos, SeekOrigin.Begin);
    }

    public static Dictionary<A, B> MergeDictionaries<A, B>(this IEnumerable<Dictionary<A, B>> dictionaries) where A : notnull {
        var dictionary = new Dictionary<A, B>();
        foreach (var dict in dictionaries) {
            foreach (var pair in dict) {
                dictionary[pair.Key] = pair.Value;
            }
        }
        return dictionary;
    }

    public static Dictionary<A, Dictionary<B, C>> MergeDictionaries<A, B, C>(this IEnumerable<Dictionary<A, Dictionary<B, C>>> dictionaries) where A : notnull
                                                                                                                                             where B : notnull {
        var merged = new Dictionary<A, Dictionary<B, C>>();
        foreach (var dict in dictionaries) {
            foreach (var (key, value) in dict) {
                if (!merged.TryAdd(key, value)) {
                    merged[key] = new List<Dictionary<B, C>> {merged[key], value}.MergeDictionaries();
                }
            }
        }
        return merged;
    }

    public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> elements) {
        return elements.Aggregate(source, (current, element) => current.Append(element));
    }

    public static string? GetCSharpType(this StructJson.Field field) {
        return field.type switch {
            "Bool" => "bool",
            "S8" => "sbyte",
            "U8" => "byte",
            "S16" => "short",
            "U16" => "ushort",
            "S32" => "int",
            "U32" => "uint",
            "S64" => "long",
            "U64" => "ulong",
            "F32" => "float",
            "F64" => "double",
            "String" => "string",
            "Resource" => "string",
            _ => null
        };
    }

    public static string? GetOriginalType(this StructJson.Field field) {
        return field.type switch {
            "Bool" => "System.Bool",
            "S8" => "System.SByte",
            "U8" => "System.Byte",
            "S16" => "System.Int16",
            "U16" => "System.UInt16",
            "S32" => "System.Int32",
            "U32" => "System.UInt32",
            "S64" => "System.Int64",
            "U64" => "System.UInt64",
            "F32" => "System.Single",
            "F64" => "System.Double",
            "String" => "System.String",
            "Resource" => "System.String",
            _ => null
        };
    }

    public static string? GetViaType(this string name) {
        name = name.ToLower();
        return VIA_TYPE_NAME_LOOKUPS.GetValueOrDefault(name);
    }

    public static string ToUpperFirstLetter(this string source) {
        if (string.IsNullOrEmpty(source)) return string.Empty;
        var letters = source.ToCharArray();
        letters[0] = char.ToUpper(letters[0]);
        return new(letters);
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("ReSharper", "GrammarMistakeInComment")]
    public static string? ToConvertedTypeName(this string? source, bool fixTypos = false) {
        if (source == null) return null;
        var name = source.ToUpperFirstLetter()
                         .Replace('.', '_')
                         .Replace("::", "_")
                         .Replace("[]", "");

        if (int.TryParse(name[0].ToString(), out _)) name = "_" + name; // If it starts with a number.
        while (name.EndsWith("k__BackingField")) name     = name[1..name.LastIndexOf('>')]; // Remove the k__BackingField.
        while (name.StartsWith("System_Collections_Generic_List`")
               || name.StartsWith("System_Collections_Generic_IReadOnlyList`")
               || name.StartsWith("System_Collections_Generic_IReadOnlyCollection`")
               || name.StartsWith("System_Collections_Generic_ICollection`")
               || name.StartsWith("System_Collections_Generic_SortedSet`")
               || name.StartsWith("System_Collections_Generic_Queue`")
               || name.StartsWith("Snow_BitSetFlag`")
               || name.StartsWith("Snow_Bitset`")
               || name.StartsWith("Snow_ai_FieldGimmickIntersector_ArrayElement`")
               || name.StartsWith("Snow_enemy_aifsm_EnemyCheckThinkCounter`")
               || name.StartsWith("Snow_enemy_EnemyConvertShellDataList`")
               || name.Contains("[[")) { // The 'array' field already covers this.
            if (name.Contains("[[")) {
                // Transforms things like this:
                // app.JobUniqueParameter.CustomSkillLevelParameter`1.Element[[app.Job04Parameter.CuttingWindParameter.LevelParameter, application, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]][]
                // into this:
                // app.JobUniqueParameter.CustomSkillLevelParameter`1.Element<app.Job04Parameter.CuttingWindParameter.LevelParameter>
                // The `[[` entries are empty structs. We need to transform them into their actual type.
                name = name.Replace("[[", "<")
                           .SubstringToEnd(0, name.IndexOf(',') - 1)
                           .ToUpperFirstLetter();
                name += ">";
            } else if (name.Contains("[[")) {
                name = name.SubstringToEnd(name.LastIndexOf("[[", StringComparison.Ordinal) + 2, name.IndexOf(','))
                           .ToUpperFirstLetter();
            } else {
                name = name.SubstringToEnd(name.LastIndexOf('<') + 1, name.IndexOf('>'))
                           .ToUpperFirstLetter();
            }
        }

        name = name.Replace('<', '_')
                   .Replace(">", "")
                   .Replace('`', '_')
                   .Replace(',', '_');

        // snow.enemy.aifsm.EnemyCheckThinkCounter`1<snow.enemy.em001.Em001Define.ThinkCounterEnum>
        // becomes
        // snow.enemy.em001.Em001Define.ThinkCounterEnum
        // Which shadows the enum by the same name, so do a little postfix magic to fix it.
        if (source.Contains("EnemyCheckThinkCounter`")) {
            name = $"{name}_EnemyCheckThinkCounter";
        }

        // Some of these will never be hit due to replaces above, but left in for safety.
        Debug.Assert(!name.StartsWith("System_Collections"), source);
        Debug.Assert(!name.Contains('`'), source);
        Debug.Assert(!name.Contains('['), source);
        Debug.Assert(!name.Contains(']'), source);
        Debug.Assert(!name.Contains('<'), source);
        Debug.Assert(!name.Contains('>'), source);
        Debug.Assert(!name.Contains(','), source);

        if (name == "System_UInt32") name = "GenericWrapper<uint>";

        if (fixTypos) {
            name = name.Replace("Cariable", "Carryable")
                       .Replace("Evalution", "Evaluation");
        }

        return name;
    }

    public static string SubstringToEnd(this string value, int startIndex, int endIndex) {
        var length = endIndex - startIndex;
        return value.Substring(startIndex, length);
    }

    public static string? ToConvertedFieldName(this string? name) {
        if (name == null) return null;
        while (name.StartsWith('_')) name = name[1..]; // Remove the leading '_'.
        while (name.EndsWith('_')) name   = name[..^1]; // Remove the trailing '_'.

        name = name.ToConvertedTypeName(true)!;
        if (name == "Index") name   = "_Index";
        if (name == "GetType") name = "Get_Type";
        return name;
    }

    [Pure]
    public static string? ToFixedEnumName(this string? name) {
        return name?.Replace("BIT_Serializable", "BIT")
                   .Replace("Bit_Serializable", "Bit")
                   .Replace("_Serializable", "_Fixed");
    }

    /**
     * Returns a list of all the items in the give list matching the given type.
     * Essentially a type-as-a-variable way to call OfType().
     */
    public static object GetGenericItemsOfType<T>(this IReadOnlyList<T> rszObjectData, Type type, bool checkCounts = false) {
        var genericItemsOfType = typeof(Enumerable).GetMethod(nameof(Enumerable.OfType), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                                                   ?.MakeGenericMethod(type)
                                                   .Invoke(null, [rszObjectData]) ?? throw new("rsz.objectData.OfType failure.");
        // We only want to do this when reading an RszObject.
        // Otherwise, it'll mess with counts where it shouldn't, like for mixing in `UserDataShell`.
        if (checkCounts) {
            // Check to make sure the counts match.
            // Without this, it'll fail silently if the generic types don't match.
            // We run into this for the types that inherit from other types with the list being the generic base type. (RE4 does this a lot.)
            var count = ((IEnumerable) genericItemsOfType).Cast<object?>().Count();
            if (count != rszObjectData.Count) {
                Debug.Assert(count == rszObjectData.Count, "Generated generic list size doesn't match given data size.");
                throw new FileNotSupported();
            }
        }

        // So the returned object is IList and not IEnumerable. (And thus we can add/remove from it.)
        var list = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                                     ?.MakeGenericMethod(type)
                                     .Invoke(null, [genericItemsOfType]) ?? throw new("rsz.objectData.ToList failure.");

        return list;
    }

    public static void Align(this Stream stream, int align) {
        while (stream.Position % align != 0) {
            stream.Seek(1, SeekOrigin.Current);
        }
    }

    [Pure]
    public static string GetNameWithoutGenericArity(this Type t) {
        var name  = t.Name;
        var index = name.IndexOf('`');
        return index == -1 ? name : name[..index];
    }

    [Pure]
    public static Dictionary<string, K> Flip<K>(this Dictionary<K, string> dict) where K : notnull {
        var dictionary = new Dictionary<string, K>();
        foreach (var pair in dict) {
            if (dictionary.ContainsKey(pair.Value)) {
                var i = 2;
                while (dictionary.ContainsKey($"{pair.Value}_{i}")) {
                    i++;
                }
                dictionary.Add($"{pair.Value}_{i}", pair.Key);
            } else {
                dictionary.Add(pair.Value, pair.Key);
            }
        }
        return dictionary;
    }

    /// <summary>
    /// Returns the first index of the element matching the given condition.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="condition"></param>
    /// <returns>Either the index or -1 if not found.</returns>
    public static int FirstIndexOf<T>(this IList<T> list, Func<T, bool> condition) {
        for (var i = 0; i < list.Count; i++) {
            if (condition(list[i])) return i;
        }
        return -1;
    }

    /// <summary>
    /// Returns the last index of the element matching the given condition.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="condition"></param>
    /// <returns>Either the index or -1 if not found.</returns>
    public static int LastIndexOf<T>(this IList<T> list, Func<T, bool> condition) {
        for (var i = list.Count - 1; i >= 0; i--) {
            if (condition(list[i])) return i;
        }
        return -1;
    }
}