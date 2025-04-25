using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
[SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
[SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
[SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
public class MoreBoobJiggle : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "More Boob Jiggle";
        const string description = "Because boobs need to jiggle.";
        const string version     = "1.3";

        var baseMod = new NexusMod {
            Version      = version,
            Name         = name,
            Desc         = description,
            NameAsBundle = name,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Running.gif",
            Action       = Mod,
        };

        var mods = new[] {
            baseMod
                .SetName($"{name} (REF)")
                .SetDesc($"{description} Alternate option done as an LUA script. ONLY ENABLE ONE OPTION.")
                .SetFiles([])
                .SetAdditionalFiles(new() {{@"reframework\autorun\MoreBoobJiggle.lua", $@"{PathHelper.MODS_PATH}\{name}\MoreBoobJiggle.lua"}})
                .SetSkipPak(true),
            baseMod
                .SetName($"{name} (PAK)")
                .SetDesc($"{description} ONLY ENABLE ONE OPTION.")
                .SetFiles([PathHelper.SWAP_DATA_TOPS_PATH])
                .SetAction(Mod),
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void Mod(List<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_TopsSwapItem param:
                    param.BreastSwaying *= 1000000;
                    break;
            }
        }
    }
}