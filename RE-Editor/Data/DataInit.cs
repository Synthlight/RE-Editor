using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;

#if DD2
using RE_Editor.Data.DD2;
#elif DRDR
using RE_Editor.Data.DRDR;
#elif MHR
using RE_Editor.Data.MHR;
#elif MHWS
using RE_Editor.Data.MHWS;
#elif RE2
using RE_Editor.Data.RE2;
#elif RE3
using RE_Editor.Data.RE3;
#elif RE4
using RE_Editor.Data.RE4;
#elif RE8
using RE_Editor.Data.RE8;
#endif

namespace RE_Editor.Data;

public static partial class DataInit {
    public static void Init() {
        Assembly.Load(nameof(Common));
        Assembly.Load(nameof(Generated));
        DataHelper.InitStructTypeInfo();

        DataHelper.STRUCT_INFO          = LoadDict<uint, StructJson>(Assets.STRUCT_INFO);
        DataHelper.GP_CRC_OVERRIDE_INFO = LoadDict<uint, uint>(Assets.GP_CRC_OVERRIDE_INFO);

        foreach (var (hash, structInfo) in DataHelper.STRUCT_INFO) {
            DataHelper.STRUCT_HASH_BY_NAME[structInfo.name!] = hash;
        }

        var supportedFilesPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\{PathHelper.SUPPORTED_FILES_NAME}";
        if (File.Exists(supportedFilesPath)) {
            DataHelper.SUPPORTED_FILES = File.ReadAllLines(supportedFilesPath);
        }

        LoadDicts();

        foreach (var lang in Enum.GetValues<Global.LangIndex>()) {
            if (!Global.TRANSLATION_MAP.ContainsKey(lang)) Global.TRANSLATION_MAP[lang] = [];
        }

#if MHR
        CreateTranslationsForSkillEnumNameColumns();
#endif
    }

    private static T Load<T>(byte[] data) {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(json);
    }

    private static List<T> LoadList<T>(byte[] data) {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<List<T>>(json);
    }

    private static Dictionary<K, V> LoadDict<K, V>(byte[] data) {
        var json = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<Dictionary<K, V>>(json);
    }

    private static byte[] GetAsset(string assetName) {
        return (byte[]) Assets.ResourceManager.GetObject(assetName);
    }
}