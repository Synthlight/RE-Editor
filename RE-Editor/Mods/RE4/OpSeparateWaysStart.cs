using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Constants;
using RE_Editor.Generated;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class OpSeparateWaysStart : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "OP Separate Ways Start";
        const string description = "Max <all things stackable>/case size for Separate Ways starter inventories.";
        const string version     = "1.2";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Files        = [PathHelper.NEW_GAME_INVENTORY_AO_DATA_PATH],
        };

        var mods = new[] {
            baseMod
                .SetName($"{name}: With Bonus Weapons")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Bonus Weapons.png")
                .SetAction(list => MakeNewInventory(list, Target.WITH_BONUS_WEAPONS)),
            baseMod
                .SetName($"{name}: Without Bonus Weapons")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\No Bonus Weapons.png")
                .SetAction(list => MakeNewInventory(list, Target.WITHOUT_BONUS_WEAPONS)),
            baseMod
                .SetName($"{name}: Just Max Case Size")
                .SetAction(list => MakeNewInventory(list, Target.JUST_MAX_CASE_SIZE)),
            baseMod
                .SetName($"{name}: Just Small Keys")
                .SetAction(list => MakeNewInventory(list, Target.JUST_SMALL_KEYS)),
            baseMod
                .SetName($"{name}: Just Pesetas & Spinels")
                .SetAction(list => MakeNewInventory(list, Target.JUST_MONEY)),
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void MakeNewInventory(IEnumerable<RszObject> rszObjectData, Target target) {
        var weaponCustomUserData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}\{PathHelper.WEAPON_CUSTOM_USERDATA_AO_PATH}")
                                             .rsz.GetEntryObject<Chainsaw_WeaponCustomUserdata>();

        var inventory = rszObjectData.First().rsz.GetEntryObject<Chainsaw_InventoryCatalogUserData>();

        if (target is Target.JUST_MONEY
            or Target.WITH_BONUS_WEAPONS
            or Target.WITHOUT_BONUS_WEAPONS) {
            inventory.PTAS        = 100000000;
            inventory.SpinelCount = 100000000;
        }

        // [0] Leon.
        // [1] Ada.
        // [2] Just has a Red9.
        ModInventory(inventory.Datas[1], target, weaponCustomUserData);
    }

    private static void ModInventory(Chainsaw_InventoryCatalogUserData_Data inventoryData, Target target, Chainsaw_WeaponCustomUserdata weaponCustomUserData) {
        if (target is Target.JUST_MAX_CASE_SIZE
            or Target.WITH_BONUS_WEAPONS
            or Target.WITHOUT_BONUS_WEAPONS) {
            inventoryData.InventoryData[0].InventorySize[0].CurrInventorySize = Chainsaw_AttacheCaseSize.XXL;
        }
        if (target is Target.JUST_SMALL_KEYS
            or Target.WITH_BONUS_WEAPONS
            or Target.WITHOUT_BONUS_WEAPONS) {
            // Needs to be in the key inventory.
            inventoryData.KeyInventorySaveData[0].Items.Add(Re4WeaponInstancer.NewKeyItem(inventoryData.rsz, ItemConstants_CH.SMALL_KEY, 0, 0, 20));
        }
        if (target is Target.WITH_BONUS_WEAPONS
            or Target.WITHOUT_BONUS_WEAPONS) {
            AddWeapons(inventoryData, target, weaponCustomUserData);
        }
    }

    private static void AddWeapons(Chainsaw_InventoryCatalogUserData_Data inventoryData, Target target, Chainsaw_WeaponCustomUserdata weaponCustomUserData) {
        var items     = inventoryData.InventoryData[0].InventoryItems;
        var blacktail = items.First(data => data.Item[0].ItemId == ItemConstants_MC.BLACKTAIL_AC);
        var knife     = items.First(data => data.Item[0].ItemId == ItemConstants_MC.TACTICAL_KNIFE);
        items.Clear();

        const int count = 8888;

        var tmpStock = Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.TMP_STOCK, 7, 0, 1);

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (target) {
            case Target.WITH_BONUS_WEAPONS:
                var scope1 = Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.HIGH_POWER_SCOPE, 6, 3, 1);

                blacktail.STRUCT_SlotIndex_Row = blacktail.STRUCT_SlotIndex_Column = 0;
                items.Add(blacktail);
                // Row 1-2
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_MC.TMP, WeaponConstants_AO.TMP, (int) ItemConstants_CH.SUBMACHINE_GUN_AMMO, 0, 2, weaponCustomUserData, ammoCount: 8888, tweaks: weapon => {
                    var weaponParts = Chainsaw_WeaponPartsCustomSingleData.Create(weapon.rsz);
                    weaponParts.ID     = tmpStock.Item[0].ID;
                    weaponParts.ItemId = (int) ItemConstants_CH.TMP_STOCK;
                    weapon.CurrentWeaponPartsCustom[0].Datas.Add(weaponParts);
                }));
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_MC.STINGRAY, WeaponConstants_AO.STINGRAY, (int) ItemConstants_CH.RIFLE_AMMO, 0, 5, weaponCustomUserData, ammoCount: 8888, tweaks: weapon => {
                    var weaponParts = Chainsaw_WeaponPartsCustomSingleData.Create(weapon.rsz);
                    weaponParts.ID     = scope1.Item[0].ID;
                    weaponParts.ItemId = (int) ItemConstants_CH.HIGH_POWER_SCOPE;
                    weapon.CurrentWeaponPartsCustom[0].Datas.Add(weaponParts);
                }));
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_AO.ELITE_KNIFE, 0, -1, 0, 12, weaponCustomUserData));
                // Row 3-4
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_MC.BLAST_CROSSBOW, WeaponConstants_MC.BLAST_CROSSBOW, (int) ItemConstants_MC.BLAST_ARROWS, 2, 0, weaponCustomUserData, ammoCount: 8888, rotation: Chainsaw_ItemDirection.Rot_090));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.HANDGUN_AMMO, 2, 3, count));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.SHOTGUN_SHELLS, 3, 3, count));
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_MC.SAWED_OFF_W_870, WeaponConstants_MC.SAWED_OFF_W_870, (int) ItemConstants_CH.SHOTGUN_SHELLS, 2, 5, weaponCustomUserData, ammoCount: 8888));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_MC.BLAST_ARROWS, 2, 11, count));
                // Row 5-6
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.SUBMACHINE_GUN_AMMO, 4, 3, count));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.RIFLE_AMMO, 5, 3, count));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.MIXED_HERB_G_PLUSR_PLUSY, 4, 5, count));
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_AO.CHICAGO_SWEEPER, WeaponConstants_AO.CHICAGO_SWEEPER, (int) ItemConstants_CH.SUBMACHINE_GUN_AMMO, 4, 6, weaponCustomUserData, ammoCount: 8888));
                // Row 7
                items.Add(scope1);
                // Row 8-9
                items.Add(tmpStock);
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_AO.INFINITE_ROCKET_LAUNCHER, WeaponConstants_AO.INFINITE_ROCKET_LAUNCHER, (int) ItemConstants_AO.INFINITE_ROCKET_LAUNCHER, 7, 3, weaponCustomUserData));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_AO.BIOSENSOR_SCOPE, 7, 11, 1));
                break;
            case Target.WITHOUT_BONUS_WEAPONS:
                var scope2 = Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.HIGH_POWER_SCOPE, 4, 7, 1);

                blacktail.STRUCT_SlotIndex_Row = blacktail.STRUCT_SlotIndex_Column = 0;
                items.Add(blacktail);
                // Row 1-2
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_MC.TMP, WeaponConstants_AO.TMP, (int) ItemConstants_CH.SUBMACHINE_GUN_AMMO, 0, 2, weaponCustomUserData, ammoCount: 8888, tweaks: weapon => {
                    var weaponParts = Chainsaw_WeaponPartsCustomSingleData.Create(weapon.rsz);
                    weaponParts.ID     = tmpStock.Item[0].ID;
                    weaponParts.ItemId = (int) ItemConstants_CH.TMP_STOCK;
                    weapon.CurrentWeaponPartsCustom[0].Datas.Add(weaponParts);
                }));
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_MC.STINGRAY, WeaponConstants_AO.STINGRAY, (int) ItemConstants_CH.RIFLE_AMMO, 0, 5, weaponCustomUserData, ammoCount: 8888, tweaks: weapon => {
                    var weaponParts = Chainsaw_WeaponPartsCustomSingleData.Create(weapon.rsz);
                    weaponParts.ID     = scope2.Item[0].ID;
                    weaponParts.ItemId = (int) ItemConstants_CH.HIGH_POWER_SCOPE;
                    weapon.CurrentWeaponPartsCustom[0].Datas.Add(weaponParts);
                }));
                knife.STRUCT_SlotIndex_Row    = 0;
                knife.STRUCT_SlotIndex_Column = 12;
                knife.CurrDirection           = Chainsaw_ItemDirection.Default;
                items.Add(knife);
                // Row 3-4
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_MC.BLAST_CROSSBOW, WeaponConstants_MC.BLAST_CROSSBOW, (int) ItemConstants_MC.BLAST_ARROWS, 2, 0, weaponCustomUserData, ammoCount: 8888, rotation: Chainsaw_ItemDirection.Rot_090));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.HANDGUN_AMMO, 2, 3, count));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.SHOTGUN_SHELLS, 3, 3, count));
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_MC.SAWED_OFF_W_870, WeaponConstants_MC.SAWED_OFF_W_870, (int) ItemConstants_CH.SHOTGUN_SHELLS, 2, 5, weaponCustomUserData, ammoCount: 8888));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_MC.BLAST_ARROWS, 2, 11, count));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.MIXED_HERB_G_PLUSR_PLUSY, 3, 12, count));
                // Row 5
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.SUBMACHINE_GUN_AMMO, 4, 3, count));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.RIFLE_AMMO, 4, 5, count));
                items.Add(scope2);
                //items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.GOLD_CHICKEN_EGG, 4, 11, count));
                // Row 6-7
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_MC.ROCKET_LAUNCHER, ItemConstants_MC.ROCKET_LAUNCHER, (int) ItemConstants_MC.ROCKET_LAUNCHER, 5, 3, weaponCustomUserData));
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.HEAVY_GRENADE, 0, -1, 5, 11, weaponCustomUserData, itemCount: count));
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.FLASH_GRENADE, 0, -1, 5, 12, weaponCustomUserData, itemCount: count));
                // Row 8-9
                items.Add(tmpStock);
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_MC.ROCKET_LAUNCHER, ItemConstants_MC.ROCKET_LAUNCHER, (int) ItemConstants_MC.ROCKET_LAUNCHER, 7, 3, weaponCustomUserData));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_AO.BIOSENSOR_SCOPE, 7, 11, 1));
                break;
            default: throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }
    }

    public enum Target {
        WITH_BONUS_WEAPONS,
        WITHOUT_BONUS_WEAPONS,
        JUST_MAX_CASE_SIZE,
        JUST_SMALL_KEYS,
        JUST_MONEY,
    }
}