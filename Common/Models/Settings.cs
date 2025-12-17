using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace RE_Editor.Common.Models;

public class Settings {
    public Global.LangIndex locale                = Global.LangIndex.eng;
    public bool             showIdBeforeName      = true;
    public bool             singleClickToEditMode = true;
    public ThemeType        theme                 = ThemeType.NONE;
}

public static class SettingsController {
    private const           string SETTINGS_FILE = "Settings.json";
    private static readonly string JSON_PATH     = "";

    static SettingsController() {
        try {
            var exePath = Assembly.GetEntryAssembly()!.Location;
            JSON_PATH = $@"{Path.GetDirectoryName(exePath)}\{SETTINGS_FILE}";
        } catch (Exception e) {
            Global.Log($"Error getting settings json path: {e.Message}");
        }
    }

    public static void Load() {
        if (JSON_PATH == "") return;
        if (File.Exists(JSON_PATH)) {
            var json = File.ReadAllText(JSON_PATH);
            Global.settings = JsonConvert.DeserializeObject<Settings>(json)!;
        }
    }

    public static void Save() {
        if (JSON_PATH == "") return;
        var json = JsonConvert.SerializeObject(Global.settings);
        File.WriteAllText(JSON_PATH, json);
    }
}