using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Models;
using RE_Editor.Common.PakModels;

#pragma warning disable CS8618

namespace RE_Editor.Common.Data;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static partial class DataHelper {
    public static readonly Dictionary<uint, Type>         RE_STRUCTS = [];
    public static          Dictionary<uint, StructJson>   STRUCT_INFO;
    public static readonly Dictionary<string, uint>       STRUCT_HASH_BY_NAME = [];
    public static          Dictionary<uint, uint>         GP_CRC_OVERRIDE_INFO;
    public static          string[]                       SUPPORTED_FILES  = [];
    public static          Dictionary<string, InfoByHash> OBSOLETE_BY_HASH = [];

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

    public static T Load<T>(byte[] data) {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(json)!;
    }

    public static List<T> LoadList<T>(byte[] data) {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<List<T>>(json)!;
    }

    public static Dictionary<K, V> LoadDict<K, V>(byte[] data) where K : notnull {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<Dictionary<K, V>>(json)!;
    }
}