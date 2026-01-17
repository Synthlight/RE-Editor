using System.CodeDom;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using Newtonsoft.Json;
using RE_Editor.Common;

namespace RE_Editor.ID_Parser;

public static partial class Program {
    public const  string BASE_PROJ_PATH      = @"..\..\..";
    private const string CONSTANTS_DIR       = $@"{BASE_PROJ_PATH}\RE-Editor\Constants\{PathHelper.CONFIG_NAME}";
    public const  string ASSETS_DIR          = $@"{BASE_PROJ_PATH}\RE-Editor\Data\{PathHelper.CONFIG_NAME}\Assets";
    public const  string DETECTOR_ASSETS_DIR = $@"{BASE_PROJ_PATH}\Obsolete-Detector\Data\{PathHelper.CONFIG_NAME}\Assets";

    public static void Main(string[] args) {
        if (args.Contains("obsoleteMap")) {
            ObsoleteMapMaker.Go();
        } else {
            Go();
        }
    }

    public static uint ParseEnum(Type enumType, string value) {
        return (uint) Convert.ChangeType(Enum.Parse(enumType, value), typeof(uint));
    }

    /**
     * Aids in finding the enum value *in the enum name itself* to get the value of the last entry before the `Max` entry.
     */
    public static int GetOneBelowMax<T>(string toFind) where T : struct, Enum {
        var names = Enum.GetNames<T>();
        for (var i = 0; i < names.Length; i++) {
            if (names[i] == toFind) {
                var target = names[i - 1];
                var regex  = new Regex(@"(\d+)");
                var match  = regex.Match(target);
                return int.Parse(match.Groups[1].Value);
            }
        }
        throw new KeyNotFoundException($"Cannot find `{toFind}` in the enum `{typeof(T)}`.");
    }

    private static void CreateAssetFile(object msg, string filename) {
        Directory.CreateDirectory(ASSETS_DIR);
        File.WriteAllText($@"{ASSETS_DIR}\{filename}.json", JsonConvert.SerializeObject(msg, Formatting.Indented));
    }

    private static void CreateConstantsFile<T>(Dictionary<string, T> engDict, string className, bool asHex = false) where T : notnull {
        Directory.CreateDirectory(CONSTANTS_DIR);
        using var writer = new StreamWriter(File.Create($@"{CONSTANTS_DIR}\{className}.cs"));
        writer.WriteLine("// ReSharper disable All");
        writer.WriteLine("using System;");
        writer.WriteLine("using System.Diagnostics.CodeAnalysis;");
        writer.WriteLine("using RE_Editor.Models.Enums;");
        writer.WriteLine("");
        writer.WriteLine("namespace RE_Editor.Constants;");
        writer.WriteLine("");
        writer.WriteLine("[SuppressMessage(\"ReSharper\", \"InconsistentNaming\")]");
        writer.WriteLine("[SuppressMessage(\"ReSharper\", \"UnusedMember.Global\")]");
        writer.WriteLine("[SuppressMessage(\"ReSharper\", \"IdentifierTypo\")]");
        writer.WriteLine($"public static class {className} {{");
        var compiler  = new CSharpCodeProvider();
        var regex     = new Regex(@"^\d");
        var namesUsed = new List<string?>(engDict.Count);
        engDict = engDict.Sort(pair => pair.Key);
        foreach (var (name, value) in engDict) {
            if (name.ToLower().Contains("#rejected#")
                || name.ToLower().Contains('?')) {
                continue;
            }
            var constName = name.ToUpper()
                                .Replace("'", "")
                                .Replace("\"", "")
                                .Replace(",", "")
                                .Replace(".", "")
                                .Replace("(", "")
                                .Replace(")", "")
                                .Replace("/", "_")
                                .Replace("&", "AND")
                                .Replace("+", "_PLUS")
                                .Replace("%", "_PERCENT")
                                .Replace('-', '_')
                                .Replace('—', '_') // Not a normal hyphen. Found in MHWS DLC names.
                                .Replace(' ', '_')
                                .Replace(':', '_')
                                .Replace('{', '_')
                                .Replace('}', '_')
                                .Replace('[', '_')
                                .Replace(']', '_')
                                .Replace('!', '_')
                                .Replace('<', '_')
                                .Replace('>', '_');
            if (regex.Match(constName).Success) constName = $"_{constName}";
            if (namesUsed.Contains(constName)) continue;
            namesUsed.Add(constName);
            if (typeof(T) == typeof(Guid)) {
                writer.WriteLine($"    public static readonly Guid {constName} = Guid.Parse(\"{value}\");");
            } else if (typeof(T) == typeof(string)) {
                writer.WriteLine($"    public const string {constName} = \"{value}\";");
            } else if (typeof(T).IsEnum) {
                writer.WriteLine($"    public const {typeof(T).Name} {constName} = {typeof(T).Name}.{value};");
            } else {
                var type     = new CodeTypeReference(typeof(T));
                var typeName = compiler.GetTypeOutput(type);
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (asHex) {
                    writer.WriteLine($"    public const {typeName} {constName} = 0x{value:X8};");
                } else {
                    writer.WriteLine($"    public const {typeName} {constName} = {value};");
                }
            }
        }
        writer.WriteLine("}");
    }

    private static void CreateLuaConstantsFile<T>(Dictionary<string, T> engDict, string className, bool asHex = false) where T : notnull {
        Directory.CreateDirectory(CONSTANTS_DIR);
        using var writer = new StreamWriter(File.Create($@"{CONSTANTS_DIR}\{className}.lua"));
        writer.WriteLine($"{className} = {{");
        var regex     = new Regex(@"^\d");
        var namesUsed = new List<string?>(engDict.Count);
        engDict = engDict.Sort(pair => pair.Key);
        foreach (var (name, value) in engDict) {
            if (name.ToLower().Contains("#rejected#")
                || name.ToLower().Contains('?')) {
                continue;
            }
            var constName = name.ToUpper()
                                .Replace("'", "")
                                .Replace("\"", "")
                                .Replace(",", "")
                                .Replace(".", "")
                                .Replace("(", "")
                                .Replace(")", "")
                                .Replace("/", "_")
                                .Replace("&", "AND")
                                .Replace("+", "_PLUS")
                                .Replace("%", "_PERCENT")
                                .Replace('-', '_')
                                .Replace(' ', '_')
                                .Replace(':', '_')
                                .Replace('{', '_')
                                .Replace('}', '_')
                                .Replace('[', '_')
                                .Replace(']', '_')
                                .Replace('!', '_')
                                .Replace('<', '_')
                                .Replace('>', '_')
                                .Replace("Α", "ALPHA")
                                .Replace("Β", "BETA")
                                .Replace("Γ", "GAMMA");
            if (regex.Match(constName).Success) constName = $"_{constName}";
            if (namesUsed.Contains(constName)) continue;
            namesUsed.Add(constName);
            if (typeof(T) == typeof(string)) {
                writer.WriteLine($"    {constName} = \"{value}\";");
            } else if (typeof(T).IsEnum) {
                writer.WriteLine($"    {constName} = {((int) (object) value)},");
            } else {
                throw new NotImplementedException($"{typeof(T)} is not implemented for LUA constants file.");
            }
        }
        writer.WriteLine("}");
    }

    // ReSharper disable once IdentifierTypo
    public static Dictionary<Global.LangIndex, Dictionary<T, string>> Merge<T>(params IList<Dictionary<Global.LangIndex, Dictionary<T, string>>> dicts) where T : notnull {
        var dict = new Dictionary<Global.LangIndex, Dictionary<T, string>>(Global.LANGUAGES.Count);
        foreach (var lang in Global.LANGUAGES) {
            if (!dict.ContainsKey(lang)) dict[lang] = [];
            foreach (var source in dicts) {
                foreach (var (key, value) in source[lang]) {
                    if (dict[lang].ContainsKey(key)) throw new($"Duplicate key found: {key}");
                    dict[lang][key] = value;
                }
            }
        }
        return dict;
    }
}