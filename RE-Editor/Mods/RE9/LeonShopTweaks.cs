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
public class LeonShopTweaks : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name    = "Leon's Shop Tweaks";
        const string version = "1.0";

        List<string> files = [
            @"\natives\STM\LevelDesign\Shop\UserData\ShopItemPriceUserData.user.3",
            @"\natives\STM\LevelDesign\Shop\UserData\ShopItemPriceUserData_Insanity.user.3",
            @"\natives\STM\LevelDesign\Shop\UserData\ShopItemPriceUserData_Second.user.3",
        ];

        var baseMod = new NexusMod {
            NameAsBundle = name,
            Version      = version,
            Files        = files
        };

        List<NexusMod> mods = [
            baseMod
                .SetName($"{name} - Price- 1")
                .SetDesc("Changes the prices to 1.")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Price- 1.png")
                .SetAction(list => ModStuff(list)),
            baseMod
                .SetName($"{name} - Price- x0.5")
                .SetDesc("Changes the prices to 1/2.")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Price x0.5.png")
                .SetAction(list => ModStuff(list, 0.5f))
        ];

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    private static void ModStuff(IList<RszObject> rszObjectData, float mult = 0f) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_ShopItemPriceData priceData:
                    foreach (var wrapper in priceData.BuyPrices) {
                        if (mult == 0f) {
                            wrapper.Value = 1;
                        } else {
                            wrapper.Value = (int) (wrapper.Value * mult);
                        }
                    }
                    break;
            }
        }
    }
}