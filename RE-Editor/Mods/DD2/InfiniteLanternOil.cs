using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
[SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
[SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
[SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
public class InfiniteLanternOil : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Infinite Lantern Oil";
        const string description = "Infinite lantern oil.";
        const string version     = "1.0";
        const string luaFile     = "InfiniteLanternOil.lua";
        const string luaPath     = $@"{PathHelper.MODS_PATH}\{name}\{luaFile}";

        Directory.CreateDirectory($@"{PathHelper.MODS_PATH}\{name}");
        WriteLua(name, version, luaPath);

        var mod = new NexusMod {
            Version         = version,
            Name            = name,
            Desc            = description,
            Files           = [],
            AdditionalFiles = new() {{$@"reframework\autorun\{luaFile}", luaPath}},
            SkipPak         = true,
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true, noPakZip: true);
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static void WriteLua(string name, string version, string path) {
        using var writer = new StreamWriter(File.Create(path));
        writer.WriteLine($"-- {name}");
        writer.WriteLine("-- By LordGregory");
        writer.WriteLine("");
        writer.WriteLine($"local version = \"{version}\"");
        writer.WriteLine($"log.info(\"Initializing `{name}` v\"..version)");
        writer.WriteLine("");
        writer.WriteLine("sdk.hook(");
        writer.WriteLine("    sdk.find_type_definition(\"app.HumanLanternController\"):get_method(\"consumeOil\"),");
        writer.WriteLine("    function(args)");
        writer.WriteLine("        return sdk.PreHookResult.SKIP_ORIGINAL");
        writer.WriteLine("    end,");
        writer.WriteLine("    function(retval)");
        writer.WriteLine("        return sdk.to_ptr(0)");
        writer.WriteLine("    end");
        writer.Write(")");
    }
}