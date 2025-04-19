using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Common.PakModels;

#if DD2
using RE_Editor.Data.DD2;
#elif DRDR
using RE_Editor.Data.DRDR;
#elif MHR
using RE_Editor.Data.MHR;
#elif MHWS
using RE_Editor.Common.PakModels;
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

        DataHelper.STRUCT_INFO          = DataHelper.LoadDict<string, StructJson>(Assets.STRUCT_INFO).KeyFromHexString();
        DataHelper.GP_CRC_OVERRIDE_INFO = DataHelper.LoadDict<uint, uint>(Assets.GP_CRC_OVERRIDE_INFO);

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

        // Load translation resources, usually dumped for enums.
        var resources = Assets.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;
        foreach (DictionaryEntry resource in resources) {
            var resourceKey = (string) resource.Key;
            if (resourceKey.StartsWith("TRANSLATION_")) {
                var data = DataHelper.LoadDict<Global.LangIndex, Dictionary<string, string>>((byte[]) resource.Value!);
                foreach (var (lang, dict) in data) {
                    if (!Global.TRANSLATION_MAP.ContainsKey(lang)) Global.TRANSLATION_MAP[lang] = [];
                    foreach (var (key, value) in dict) {
                        Global.TRANSLATION_MAP[lang][key] = value;
                    }
                }
            } else if (resourceKey == "OBSOLETE_BY_HASH") {
                DataHelper.OBSOLETE_BY_HASH = DataHelper.Load<Dictionary<string, InfoByHash>>((byte[]) resource.Value!);
            }
        }

#if MHR
        CreateTranslationsForSkillEnumNameColumns();
#endif
    }
}