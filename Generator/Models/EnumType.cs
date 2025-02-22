using System.Text.RegularExpressions;

namespace RE_Editor.Generator.Models;

public class EnumType(string name, string originalName, string type) {
    public readonly string name         = name;
    public readonly string originalName = originalName;
    public          string type         = type;
    public          int    useCount;
    public          bool   isFlags;

    public  List<string>? entries;
    private string?       contents;
    public string? Contents {
        get => contents;
        set {
            if (value == null) {
                entries  = null;
                contents = null;
                return;
            }
            entries = [];
            var regex = new Regex(@"\s*(\S+)\s*=");
            foreach (var rawLine in value.Split("\r\n")) {
                var line = rawLine.Trim();
                if (line is "{" or "}") continue;
                var name = regex.Match(line).Groups[1].Value;
                entries.Add(name);
            }
            contents = value?.Replace("        ", "    ")
                            .Replace("    }", "}");
        }
    }

    public int? EntryCount => entries?.Count;
}