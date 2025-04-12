using System.Text.RegularExpressions;

namespace RE_Editor.Generator.Models;

public class EnumType(string name, string originalName, string type) {
    public readonly string name         = name;
    public readonly string originalName = originalName;
    public          string type         = type;
    public          int    useCount;
    public          bool   isFlags;

    public  List<string>? entries;
    public  List<string>? values;
    private string?       contents;
    public string? Contents {
        get => contents;
        set {
            if (value == null) {
                entries  = null;
                values   = null;
                contents = null;
                return;
            }
            entries = [];
            values  = [];
            var regex = new Regex(@"\s*(\S+)\s*=\s*(-?\d+),?");
            foreach (var rawLine in value.Split("\r\n")) {
                var line = rawLine.Trim();
                if (line is "{" or "}") continue;
                var name    = regex.Match(line).Groups[1].Value;
                var enumVal = regex.Match(line).Groups[2].Value;
                entries.Add(name);
                values.Add(enumVal);
            }
            contents = value?.Replace("        ", "    ")
                            .Replace("    }", "}");
        }
    }

    public int? EntryCount => entries?.Count;
}