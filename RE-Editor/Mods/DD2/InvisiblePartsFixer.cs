using System.Collections.Generic;
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
public class InvisiblePartsFixer : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Invisible Parts Fixer";
        const string description = "Restores missing skin textures from mesh parts hidden by tweaking the armor swap database.";
        const string version     = "1.1";

        var baseMod = new NexusMod {
            Version      = version,
            Desc         = description,
            NameAsBundle = name,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Title.png",
            Files        = [],
        };

        var nulledMasks = new Dictionary<string, object>();
        var reader      = File.OpenText(@"V:\DD2\DD2_PC_Release.list");
        while (!reader.EndOfStream) {
            var line = reader.ReadLine()!
                             .ToLower()
                             .Replace('\\', '/');
            if (line.Contains("/_furmasks/")) {
                nulledMasks[line] = $@"{PathHelper.MODS_PATH}\{name}\null furmask.tex.760230703";
            }
        }

        var mods = new List<NexusMod> {
            baseMod.SetName($"{name} (REF)")
                   .SetDesc($"{description}\nWorks on-the-fly with player equipment. Doesn't work for invisible parts in the inventory or photo mode.")
                   .SetAdditionalFiles(new() {{@"reframework\autorun\InvisiblePartsFixer.lua", $@"{PathHelper.MODS_PATH}\{name}\InvisiblePartsFixer.lua"}})
                   .SetSkipPak(true),
            baseMod.SetName($"{name} (Blank All Fur Masks)")
                   .SetDesc($"{description}\nBlanks every single fur mask in the game, which works for photo mode & the inventory too.")
                   .SetAdditionalFiles(nulledMasks),
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }
}