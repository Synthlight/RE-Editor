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
        const string description = "Changes the limit from 10 to 127.";
        const string version     = "1.13";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Title.png",
        };

        var additionalFiles = new Dictionary<string, object> {{@"reframework\plugins\MapIconSizeFixer.dll", $@"{PathHelper.MODS_PATH}\{name}\MapIconSizeFixer.dll"}};

        var mods = new[] {
            baseMod
                .SetName($"{name}: REF (MUST INSTALL BOTH)")
                .SetFiles([])
                .SetAdditionalFiles(additionalFiles)
                .SetSkipPak(true),
            baseMod
                .SetName($"{name}: PAK (MUST INSTALL BOTH)") // 'Loose' too since, of the two zips made, one will be a `pak`, the other won't.
                .SetFiles([PathHelper.ITEM_PARAMETERS_PATH])
                .SetAction(Mod),
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void Mod(List<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_ItemParameters itemParameters:
                    itemParameters.WarpPointPutEnableNum = 256;
                    break;
            }
        }
    }
}