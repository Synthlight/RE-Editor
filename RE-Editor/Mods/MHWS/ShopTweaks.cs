﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class ShopTweaks : IMod {
    [UsedImplicitly]
    public static void Make() {
        const string name        = "Shop Tweaks";
        const string description = "Various shop lists.";
        const string version     = "1.0.0";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description
        };

        var itemShopData = ReDataFile.Read(PathHelper.CHUNK_PATH + PathHelper.ITEM_SHOP_DATA_PATH);
        var existingShopItems = (from entry in itemShopData.rsz.objectData.OfType<App_user_data_ItemShopData_cData>()
                                 select entry.ItemId).ToList();
        var itemData = ReDataFile.Read(PathHelper.CHUNK_PATH + PathHelper.ITEM_DATA_PATH);
        var itemModeData = (from entry in itemData.rsz.objectData.OfType<App_user_data_ItemData_cData>()
                            let mode = GetMode(entry.Type)
                            where mode != null
                            where (App_ItemDef_ID_Fixed) entry.ItemId is > App_ItemDef_ID_Fixed.NONE and < App_ItemDef_ID_Fixed.MAX
                            where !existingShopItems.Contains(entry.ItemId)
                            where DataHelper.ITEM_NAME_LOOKUP[Global.LangIndex.eng].ContainsKey((uint) entry.ItemId)
                                  && DataHelper.ITEM_NAME_LOOKUP[Global.LangIndex.eng][(uint) entry.ItemId] != "#Rejected#"
                            orderby entry.SortId
                            select new KeyValuePair<int, Mode>(entry.ItemId, (Mode) mode)).ToDictionary(pair => pair.Key, pair => pair.Value);
        var itemSortData = (from entry in itemData.rsz.objectData.OfType<App_user_data_ItemData_cData>()
                            select new KeyValuePair<int, int>(entry.ItemId, entry.SortId)).ToDictionary(pair => pair.Key, pair => pair.Value);

        var mods = new List<INexusMod> {
            baseMod
                .SetName($"{name} - Everything")
                .SetFiles([PathHelper.ITEM_SHOP_DATA_PATH])
                .SetAction(list => AddShopItems(list, itemModeData, itemSortData, Mode.CONSUMABLES | Mode.MATERIALS)),
            baseMod
                .SetName($"{name} - Consumables Only")
                .SetFiles([PathHelper.ITEM_SHOP_DATA_PATH])
                .SetAction(list => AddShopItems(list, itemModeData, itemSortData, Mode.CONSUMABLES)),
            baseMod
                .SetName($"{name} - Gems Only")
                .SetFiles([PathHelper.ITEM_SHOP_DATA_PATH])
                .SetAction(list => AddShopItems(list, itemModeData, itemSortData, Mode.GEMS)),
            baseMod
                .SetName($"{name} - Materials Only")
                .SetFiles([PathHelper.ITEM_SHOP_DATA_PATH])
                .SetAction(list => AddShopItems(list, itemModeData, itemSortData, Mode.MATERIALS)),
        };

        ModMaker.WriteMods(mods, name, copyLooseToFluffy: true, noPakZip: true);
    }

    private static Mode? GetMode(App_ItemDef_TYPE_Fixed type) {
        return type switch {
            App_ItemDef_TYPE_Fixed.EXPENDABLE => Mode.CONSUMABLES,
            App_ItemDef_TYPE_Fixed.TOOL => null,
            App_ItemDef_TYPE_Fixed.MATERIAL => Mode.MATERIALS,
            App_ItemDef_TYPE_Fixed.SHELL => Mode.CONSUMABLES,
            App_ItemDef_TYPE_Fixed.BOTTLE => Mode.CONSUMABLES,
            App_ItemDef_TYPE_Fixed.POINT => null,
            App_ItemDef_TYPE_Fixed.GEM => Mode.GEMS,
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private static void AddShopItems(List<RszObject> rszObjectData, Dictionary<int, Mode> itemModeData, Dictionary<int, int> itemSortData, Mode mode) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_ItemShopData itemShopData:
                    var entries = itemShopData.Values.Cast<App_user_data_ItemShopData_cData>().ToList();
                    foreach (var (itemId, itemMode) in itemModeData) {
                        var shouldAdd = (mode.HasFlag(Mode.CONSUMABLES) && itemMode == Mode.CONSUMABLES)
                                        || (mode.HasFlag(Mode.MATERIALS) && itemMode == Mode.MATERIALS)
                                        || (mode.HasFlag(Mode.GEMS) && itemMode == Mode.GEMS);
                        if (shouldAdd) {
                            entries.Add(CreateItem(itemShopData.rsz, itemId));
                        }
                    }
                    // TODO: Change back to `itemSortData[entry1.ItemId].CompareTo(itemSortData[entry2.ItemId])` once released and there won't be unknowns.
                    entries.Sort((entry1, entry2) => itemSortData.TryGet(entry1.ItemId, 0).CompareTo(itemSortData.TryGet(entry2.ItemId, 0)));
                    for (var i = 0; i < entries.Count; i++) {
                        var entry = entries[i];
                        entry._Index       = i;
                        entry.StoryPackage = App_StoryPackageFlag_TYPE_Fixed.INVALID;
                    }
                    itemShopData.Values = new(entries);
                    break;
            }
        }
    }

    private static App_user_data_ItemShopData_cData CreateItem(RSZ rsz, int itemId) {
        var shopEntry = App_user_data_ItemShopData_cData.Create(rsz);
        shopEntry.ItemId       = itemId;
        shopEntry.StoryPackage = App_StoryPackageFlag_TYPE_Fixed.INVALID;
        shopEntry.IsSaleTarget = true;
        shopEntry._Index       = 0; // Will be resorted later.
        return shopEntry;
    }

    [Flags]
    private enum Mode {
        CONSUMABLES = 1,
        GEMS,
        MATERIALS
    }
}