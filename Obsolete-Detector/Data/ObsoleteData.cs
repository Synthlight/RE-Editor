using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.PakModels;
using RE_Editor.Obsolete_Detector.Models;

#if DD2
using RE_Editor.Obsolete_Detector.Data.DD2;
#elif DRDR
using RE_Editor.Obsolete_Detector.Data.DRDR;
#elif MHR
using RE_Editor.Obsolete_Detector.Data.MHR;
#elif MHWS
using RE_Editor.Obsolete_Detector.Data.MHWS;
#elif PRAGMATA
using RE_Editor.Obsolete_Detector.Data.PRAGMATA;
#elif RE2
using RE_Editor.Obsolete_Detector.Data.RE2;
#elif RE3
using RE_Editor.Obsolete_Detector.Data.RE3;
#elif RE4
using RE_Editor.Obsolete_Detector.Data.RE4;
#elif RE8
using RE_Editor.Obsolete_Detector.Data.RE8;
#endif

namespace RE_Editor.Obsolete_Detector.Data;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class ObsoleteData {
    public static Dictionary<string, GoodPakInfo> GOOD_PAK_MAP    = [];
    public static Dictionary<uint, uint>          HASH_TO_CRC_MAP = [];
    public static string[]                        PAK_LIST        = [];
    public const  string                          SETTINGS_FILE   = "settings.json";
    public static Settings                        SETTINGS        = new(OnSettingsChanged);

    public static readonly Dictionary<string, PakDateInfo> PAK_INFO_BY_NAME = [];

    private static string ExeDir       => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    private static string SettingsPath => $@"{ExeDir}\{SETTINGS_FILE}";

    public static void Init() {
        // Do it this way so it doesn't matter if the asset resx has a file or not.

        var resources = Assets.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, false)!;

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (DictionaryEntry resource in resources) {
            var resourceKey = (string) resource.Key;
            switch (resourceKey) {
                case "GOOD_PAK_MAP":
                    GOOD_PAK_MAP = DataHelper.LoadDict<string, GoodPakInfo>((byte[]) resource.Value!);
                    break;
                case "HASH_TO_CRC_MAP":
                    HASH_TO_CRC_MAP = DataHelper.LoadDict<uint, uint>((byte[]) resource.Value!);
                    break;
                case "List":
                    PAK_LIST = ((string) resource.Value!).Split('\n');
                    break;
            }
        }

        foreach (var pakDateInfo in PathHelper.PAK_UPDATE_INFO) {
            foreach (var pakFile in pakDateInfo.pakFiles) {
                PAK_INFO_BY_NAME[pakFile] = pakDateInfo;
            }
        }

        ReadSettings();
    }

    private static void ReadSettings() {
        try {
            var settingsPath = SettingsPath;
            if (File.Exists(settingsPath)) {
                var json = File.ReadAllText(settingsPath);
                SETTINGS           = JsonConvert.DeserializeObject<Settings>(json) ?? SETTINGS;
                SETTINGS.onChanged = OnSettingsChanged;
            }
        } catch {
            // ignored
        }
    }

    private static void OnSettingsChanged() {
        var settingsPath = SettingsPath;
        var json         = JsonConvert.SerializeObject(SETTINGS);
        File.WriteAllText(settingsPath, json);
    }
}