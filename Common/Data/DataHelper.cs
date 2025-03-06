using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Models;

#pragma warning disable CS8618

namespace RE_Editor.Common.Data;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static partial class DataHelper {
    public static readonly Dictionary<uint, Type>       RE_STRUCTS = [];
    public static          Dictionary<uint, StructJson> STRUCT_INFO;
    public static readonly Dictionary<string, uint>     STRUCT_HASH_BY_NAME = [];
    public static          Dictionary<uint, uint>       GP_CRC_OVERRIDE_INFO;
    public static          string[]                     SUPPORTED_FILES = [];

    public static void InitStructTypeInfo() {
        var mhrStructs = AppDomain.CurrentDomain.GetAssemblies()
                                  .SelectMany(t => t.GetTypes())
                                  .Where(type => type.GetCustomAttribute<MhrStructAttribute>() != null);
        foreach (var type in mhrStructs) {
            var hashField = type.GetField("HASH", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
            var value     = (uint) hashField.GetValue(null)!;
            RE_STRUCTS[value] = type;
        }
    }
}