using Newtonsoft.Json;

namespace RE_Editor.Obsolete_Detector.Models;

public class Settings(Action? onChanged) {
    [JsonIgnore]
    public Action? onChanged = onChanged;

    private string? gamePath;
    public string? GamePath {
        get => gamePath;
        set {
            gamePath = value;
            onChanged?.Invoke();
        }
    }

    private string? fmmPath;

    public string? FmmPath {
        get => fmmPath;
        set {
            fmmPath = value;
            onChanged?.Invoke();
        }
    }
}