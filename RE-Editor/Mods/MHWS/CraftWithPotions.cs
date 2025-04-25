using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Constants;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class CraftWithPotions : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Craft With Potions";
        const string description = "Craft any item with only a potion.";
        const string version     = "1.0";

        List<string> files = [PathHelper.ITEM_RECIPE_DATA_PATH];

        var mod = new NexusMod {
            Name    = name,
            Version = version,
            Desc    = description,
            Files   = files,
            Action  = ModStuff
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true);
    }

    private static void ModStuff(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_cItemRecipe_cData item:
                    item.Item[0].Value = (int) ItemConstants.POTION;
                    item.Item[1].Value = (int) ItemConstants.___;
                    break;
            }
        }
    }
}