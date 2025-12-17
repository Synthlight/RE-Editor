using System.IO;
using Newtonsoft.Json;

namespace RE_Editor.Common.Models;

public class Settings {
    public Global.LangIndex locale                = Global.LangIndex.eng;
    public bool             showIdBeforeName      = true;
    public bool             singleClickToEditMode = true;
    public ThemeType        theme                 = ThemeType.NONE;
}

public static class SettingsController {
    private static readonly string SETTINGS_DIR  = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\RE-Editor";
    private const           string SETTINGS_FILE = "Settings.json";
    private static readonly string JSON_PATH     = $@"{SETTINGS_DIR}\{SETTINGS_FILE}";

    static SettingsController() {
        Directory.CreateDirectory(SETTINGS_DIR);
    }

    public static void Load() {
        if (File.Exists(JSON_PATH)) {
            var json = File.ReadAllText(JSON_PATH);
            Global.settings = JsonConvert.DeserializeObject<Settings>(json)!;
        }
    }

    public static void Save() {
        var json = JsonConvert.SerializeObject(Global.settings);
        File.WriteAllText(JSON_PATH, json);
    }
}