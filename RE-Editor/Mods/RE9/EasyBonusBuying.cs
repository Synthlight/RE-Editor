using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class EasyBonusBuying : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Easy Bonus Buying";
        const string description = "Changes the purchase conditions.";
        const string version     = "1.0";

        List<string> files = [
            PathHelper.BONUS_DATA_PATH
        ];

        var mod = new NexusMod {
            Name    = name,
            Version = version,
            Desc    = description,
            Image   = $@"{PathHelper.MODS_PATH}\{name}\Pic.png",
            Files   = files,
            Action  = ModStuff
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true);
    }

    private static void ModStuff(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_EXShopAcquirableCondition_ID002 condition:
                    if (condition.Price > 1) {
                        condition.Price = 1;
                    }
                    break;
                case App_EXShopAcquirableCondition_ID004 condition:
                    if (condition.Price > 1) {
                        condition.Price = 1;
                    }
                    break;
            }
        }
    }
}