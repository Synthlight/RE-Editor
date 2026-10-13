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
public class MorePortCrystals : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Higher Portcrystal Limit";
        const string description = "Changes the limit from 15 to 127.";
        const string version     = "1.17";

        var mod = new NexusMod {
            Name    = name,
            Version = version,
            Desc    = description,
            Image   = $@"{PathHelper.MODS_PATH}\{name}\Title.png",
            Files = [
                PathHelper.ITEM_PARAMETERS_PATH,
                PathHelper.ITEM_PARAMETERS_DARK_ARISEN_PATH
            ],
            AdditionalFiles = new() {{@"reframework\plugins\MapIconSizeFixer.dll", $@"{PathHelper.MODS_PATH}\{name}\MapIconSizeFixer.dll"}},
            Action          = Mod,
            SkipPak         = true
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true, noPakZip: true);
    }

    public static void Mod(List<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_ItemParameters itemParameters:
                    itemParameters.WarpPointPutEnableNum    = 256;
                    itemParameters.NRDWarpPointPutEnableNum = 256;
                    break;
            }
        }
    }
}