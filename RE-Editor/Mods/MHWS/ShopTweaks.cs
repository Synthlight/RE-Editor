using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Constants;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class ShopTweaks : IMod {
    private static readonly List<App_ItemDef_ID_Fixed> BLACKLIST = [
        ItemConstants.STONE,
        ItemConstants.NORMAL_AMMO,
        ItemConstants.PIERCE_AMMO,
        ItemConstants.SPREAD_AMMO,
        ItemConstants.THORNGRASS_POD,
        ItemConstants.BURST_POD,
        ItemConstants.SCREAMER_POD_2, // First one (70) is fine, the one that's 278 is not.
        ItemConstants.BLEEDING_POD,
        ItemConstants.GROUNDING_POD,
        ItemConstants.POISON_POD,
        ItemConstants.BRIGHTMOSS,
        ItemConstants.TORCH_POD,
        ItemConstants.PUDDLE_POD,
        ItemConstants.BOMB_POD,
        ItemConstants.PIERCING_POD,
        ItemConstants.THUNDER_POD,
        ItemConstants.FROST_POD,
        ItemConstants.DRAGON_POD,
        ItemConstants.FRESH_HONEY,
        ItemConstants.FROSTBURST,
        ItemConstants.HEAVY_BLUNT_POD,
        ItemConstants.HEAVY_SLICING_POD,
        ItemConstants.HEAVY_EXPLOSION_POD,
        ItemConstants.HEAVY_PIERCING_POD,
        ItemConstants.CLOSE_RANGE_COATING,
        ItemConstants.POWER_COATING,
        ItemConstants.PIERCE_COATING,
        ItemConstants.PARALYSIS_COATING,
        ItemConstants.POISON_COATING,
        ItemConstants.SLEEP_COATING,
        ItemConstants.BLAST_COATING,
        ItemConstants.EXHAUST_COATING
    ];

    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Shop Tweaks";
        const string description = "Various shop lists.";
        const string version     = "1.7";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Thumb.png"
        };

        var itemShopData = ReDataFile.Read(PathHelper.CHUNK_PATH + PathHelper.ITEM_SHOP_DATA_PATH);
        var existingShopItems = (from entry in itemShopData.rsz.objectData.OfType<App_user_data_ItemShopData_cData>()
                                 select entry.ItemId).ToList();
        var itemData = ReDataFile.Read(PathHelper.CHUNK_PATH + PathHelper.ITEM_DATA_PATH);
        var itemModeData = (from entry in itemData.rsz.objectData.OfType<App_user_data_ItemData_cData>()
                            let mode = GetMode(entry)
                            where mode != null
                            where !BLACKLIST.Contains((App_ItemDef_ID_Fixed) entry.ItemId)
                            where (App_ItemDef_ID_Fixed) entry.ItemId != App_ItemDef_ID_Fixed.NONE && (App_ItemDef_ID_Fixed) entry.ItemId != App_ItemDef_ID_Fixed.INVALID
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
                .SetName($"{name} - Consumables + Ingredients")
                .SetFiles([PathHelper.ITEM_SHOP_DATA_PATH])
                .SetAction(list => AddShopItems(list, itemModeData, itemSortData, Mode.CONSUMABLES | Mode.INGREDIENTS)),
            baseMod
                .SetName($"{name} - Materials Only")
                .SetFiles([PathHelper.ITEM_SHOP_DATA_PATH])
                .SetAction(list => AddShopItems(list, itemModeData, itemSortData, Mode.MATERIALS)),
            baseMod
                .SetName($"{name} - Arena Coins Only")
                .SetFiles([PathHelper.ITEM_SHOP_DATA_PATH])
                .SetAction(list => AddShopItems(list, itemModeData, itemSortData, Mode.ARENA_COINS))
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);


        ModMaker.WriteMods(mainWindow, [
            new NexusMod {
                Name    = $"{name} - Normal (38)/Pierce (39)/Spread (40) Ammo",
                Version = "1.0",
                Desc    = "ONLY USE IF YOU HAVE BUGGED AMMO AND THEY ARE MISSING!!!!",
                Image   = $@"{PathHelper.MODS_PATH}\{name}\Thumb.png",
                Files   = [PathHelper.ITEM_SHOP_DATA_PATH],
                Action = data => {
                    var shopData = (App_user_data_ItemShopData) data.First(entry => entry is App_user_data_ItemShopData);
                    shopData.Values.Add(CreateItem(itemShopData.rsz, (int) ItemConstants.NORMAL_AMMO, true));
                    shopData.Values.Add(CreateItem(itemShopData.rsz, (int) ItemConstants.PIERCE_AMMO, true));
                    shopData.Values.Add(CreateItem(itemShopData.rsz, (int) ItemConstants.SPREAD_AMMO, true));
                }
            }
        ], $"{name} - Ammo", copyLooseToFluffy: false);
    }

    private static Mode? GetMode(App_user_data_ItemData_cData item) {
        if (item.ItemId == (int) ItemConstants.PARA_POD) return null;

        switch (item.Type) {
            case App_ItemDef_TYPE_Fixed.EXPENDABLE:
                return Mode.CONSUMABLES;
            case App_ItemDef_TYPE_Fixed.POINT:
            case App_ItemDef_TYPE_Fixed.MATERIAL:
                if (item.Type == App_ItemDef_TYPE_Fixed.MATERIAL && DataHelper.ITEM_NAME_LOOKUP[Global.LangIndex.eng][(uint) item.ItemId].EndsWith(" Coin")) {
                    return Mode.MATERIALS | Mode.ARENA_COINS;
                }
                if (item.Type == App_ItemDef_TYPE_Fixed.MATERIAL && DataHelper.ITEM_NAME_LOOKUP[Global.LangIndex.eng][(uint) item.ItemId].ToLower().EndsWith("meal voucher")) {
                    return Mode.MATERIALS | Mode.INGREDIENTS;
                }
                if (item.AddIconType == App_IconDef_AddIcon_Fixed.INGREDIENTS) {
                    return Mode.MATERIALS | Mode.INGREDIENTS;
                }
                return Mode.MATERIALS;
            case App_ItemDef_TYPE_Fixed.SHELL:
            case App_ItemDef_TYPE_Fixed.BOTTLE:
                return Mode.CONSUMABLES;
            case App_ItemDef_TYPE_Fixed.TOOL:
                return null;
            case App_ItemDef_TYPE_Fixed.GEM:
                return Mode.GEMS;
            default: throw new ArgumentOutOfRangeException(nameof(Type));
        }
    }

    private static void AddShopItems(List<RszObject> rszObjectData, Dictionary<int, Mode> itemModeData, Dictionary<int, int> itemSortData, Mode mode) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_ItemShopData itemShopData:
                    var entries = itemShopData.Values.Cast<App_user_data_ItemShopData_cData>().ToList();
                    foreach (var (itemId, itemMode) in itemModeData) {
                        var shouldAdd = (mode.HasFlag(Mode.CONSUMABLES) && itemMode.HasFlag(Mode.CONSUMABLES))
                                        || (mode.HasFlag(Mode.MATERIALS) && itemMode.HasFlag(Mode.MATERIALS))
                                        || (mode.HasFlag(Mode.GEMS) && itemMode.HasFlag(Mode.GEMS))
                                        || (mode.HasFlag(Mode.INGREDIENTS) && itemMode.HasFlag(Mode.INGREDIENTS))
                                        || (mode.HasFlag(Mode.ARENA_COINS) && itemMode.HasFlag(Mode.ARENA_COINS));
                        if (shouldAdd) {
                            entries.Add(CreateItem(itemShopData.rsz, itemId));
                        }
                    }
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

    public static App_user_data_ItemShopData_cData CreateItem(RSZ rsz, int itemId, bool ignoreBlacklist = false) {
        if (!ignoreBlacklist && BLACKLIST.Contains((App_ItemDef_ID_Fixed) itemId)) {
            throw new($"Error: Blacklisted item `{itemId}` made it through.");
        }
        var shopEntry = App_user_data_ItemShopData_cData.Create(rsz);
        shopEntry.ItemId       = itemId;
        shopEntry.StoryPackage = App_StoryPackageFlag_TYPE_Fixed.INVALID;
        shopEntry.IsSaleTarget = true;
        shopEntry._Index       = 0; // Will be resorted later.
        return shopEntry;
    }

    [Flags]
    private enum Mode {
        CONSUMABLES = 1 << 0,
        GEMS        = 1 << 1,
        MATERIALS   = 1 << 2,
        INGREDIENTS = 1 << 3,
        ARENA_COINS = 1 << 4
    }
}