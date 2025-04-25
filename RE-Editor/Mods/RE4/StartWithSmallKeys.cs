using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Constants;
using RE_Editor.Generated;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class StartWithSmallKeys : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string bundleName  = "Start with Small Keys";
        const string description = "Gives you 20 Small Keys at the start of new games.";
        const string version     = "1.0";
        const string imgPath     = $@"{PathHelper.MODS_PATH}\{bundleName}\Small Key Start.png";

        var mod = new NexusMod {
            Name    = bundleName,
            Version = version,
            Desc    = description,
            Image   = imgPath,
            Files = [
                PathHelper.NEW_GAME_INVENTORY_DATA_PATH,
                PathHelper.NEW_GAME_INVENTORY_AO_DATA_PATH
            ],
            Action = AddSmallKeys
        };

        ModMaker.WriteMods(mainWindow, [mod], bundleName, copyLooseToFluffy: true);
    }

    public static void AddSmallKeys(IEnumerable<RszObject> rszObjectData) {
        foreach (var obj in new List<RszObject>(rszObjectData)) {
            if (obj is not Chainsaw_InventoryCatalogUserData inventoryData) continue;

            // Needs to be in the key inventory.
            foreach (var data in inventoryData.Datas) {
                data.KeyInventorySaveData[0].Items.Add(Re4WeaponInstancer.NewKeyItem(obj.rsz, ItemConstants_CH.SMALL_KEY, 0, 0, 20));
            }
        }
    }
}