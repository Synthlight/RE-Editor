using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class OpMercenariesStart : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "OP Mercenaries Start";
        const string description = "Max <all things stackable>/case size for all mercenaries starter inventories.";
        const string version     = "1.0";

        var mod = new NexusMod {
            Name    = name,
            Version = version,
            Desc    = description,
            Files   = PathHelper.NEW_GAME_INVENTORY_MC_DATA_PATHS,
            Action  = MakeNewInventory
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true);
    }

    public static void MakeNewInventory(IEnumerable<RszObject> rszObjectData) {
        foreach (var obj in new List<RszObject>(rszObjectData)) {
            if (obj is not Chainsaw_InventoryCatalogUserData inventoryData) continue;

            foreach (var data in inventoryData.Datas) {
                data.InventoryData[0].InventorySize[0].CurrInventorySize = Chainsaw_AttacheCaseSize.XXL;

                foreach (var item in data.InventoryData[0].InventoryItems) {
                    item.Item[0].CurrentItemCount = 8999;
                    if (item.Item[0] is Chainsaw_WeaponItem weapon) {
                        weapon.CurrentItemCount = 8999;
                    }
                }
            }
        }
    }
}