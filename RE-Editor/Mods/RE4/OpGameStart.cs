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
public class OpGameStart : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "OP Game Start";
        const string description = "Gives OP weapon setups on new games.";
        const string version     = "1.2";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Files        = [PathHelper.NEW_GAME_INVENTORY_DATA_PATH]
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
        var weaponCustomUserData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}\{PathHelper.WEAPON_CUSTOM_USERDATA_PATH}")
                                             .rsz.GetEntryObject<Chainsaw_WeaponCustomUserdata>();

        var inventory = rszObjectData.First().rsz.GetEntryObject<Chainsaw_InventoryCatalogUserData>();

        if (target is Target.JUST_MONEY
            or Target.WITH_BONUS_WEAPONS
            or Target.WITHOUT_BONUS_WEAPONS) {
            inventory.PTAS        = 100000000;
            inventory.SpinelCount = 100000000;
        }

        // [0] Leon.
        // [1] Just has a Red9.
        ModInventory(inventory.Datas[0], target, weaponCustomUserData);
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
        var items = inventoryData.InventoryData[0].InventoryItems;

        const int count = 8888;

        var knife = items[2]; // Knife.
        knife.STRUCT_SlotIndex_Row    = 8;
        knife.STRUCT_SlotIndex_Column = 10;

        items.RemoveAt(0); // Remove the first aid spray.

        var scope = Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.HIGH_POWER_SCOPE, 1, 10, count);
        var stock = Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.RED9_STOCK, 8, 6, 1);

        // Row 1-2
        items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.STINGRAY, WeaponConstants_CH.STINGRAY, (int) ItemConstants_CH.RIFLE_AMMO, 0, 3, weaponCustomUserData, ammoCount: 10, tweaks: weapon => {
            var weaponParts = Chainsaw_WeaponPartsCustomSingleData.Create(weapon.rsz);
            weaponParts.ID     = scope.Item[0].ID;
            weaponParts.ItemId = (int) ItemConstants_CH.HIGH_POWER_SCOPE;
            weapon.CurrentWeaponPartsCustom[0].Datas.Add(weaponParts);
        }));
        items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.MIXED_HERB_G_PLUSR_PLUSY, 0, 10, count, Chainsaw_ItemDirection.Rot_090));
        items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.GOLD_CHICKEN_EGG, 0, (int) ItemConstants_CH.GOLD_CHICKEN_EGG, 0, 12, weaponCustomUserData, itemCount: count)); // Row 1
        items.Add(scope); // Row 2
        // Row 3-4
        items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.RED9, WeaponConstants_CH.RED9, (int) ItemConstants_CH.HANDGUN_AMMO, 2, 8, weaponCustomUserData, ammoCount: 8, tweaks: weapon => {
            var weaponParts = Chainsaw_WeaponPartsCustomSingleData.Create(weapon.rsz);
            weaponParts.ID     = stock.Item[0].ID;
            weaponParts.ItemId = (int) ItemConstants_CH.RED9_STOCK;
            weapon.CurrentWeaponPartsCustom[0].Datas.Add(weaponParts);
        }));
        items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.HEAVY_GRENADE, 0, -1, 2, 11, weaponCustomUserData, itemCount: count));
        items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.FLASH_GRENADE, 0, -1, 2, 12, weaponCustomUserData, itemCount: count));
        // Row 5-6
        items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.KILLER7, WeaponConstants_CH.KILLER7, (int) ItemConstants_CH.MAGNUM_AMMO, 4, 8, weaponCustomUserData, ammoCount: 7));
        items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.BIOSENSOR_SCOPE, 4, 11, 1));
        // Row 7-8
        items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.PUNISHER, WeaponConstants_CH.PUNISHER, (int) ItemConstants_CH.HANDGUN_AMMO, 6, 8, weaponCustomUserData, ammoCount: 12));
        items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.HANDGUN_AMMO, 6, 11, count));
        items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.RIFLE_AMMO, 7, 11, count));
        // Row 9
        items.Add(stock);

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (target) {
            case Target.WITH_BONUS_WEAPONS:
                // Row 3-5
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.CHICAGO_SWEEPER, WeaponConstants_CH.CHICAGO_SWEEPER, (int) ItemConstants_CH.SUBMACHINE_GUN_AMMO, 2, 0, weaponCustomUserData, ammoCount: 50));
                //items.Add(NewWeapon(inventoryData.rsz, ItemConstants_CH.PRIMAL_KNIFE, 0xFFFFFFFF, 2, 7, ammoCount: 0, rotation: Chainsaw_ItemDirection.Rot_090));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.LASER_SIGHT, 2, 7, 1));
                // Row 6-7
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.INFINITE_ROCKET_LAUNCHER, WeaponConstants_CH.INFINITE_ROCKET_LAUNCHER, (int) ItemConstants_CH.INFINITE_ROCKET_LAUNCHER, 5, 0, weaponCustomUserData));
                // Row 8-9
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.HANDCANNON, WeaponConstants_CH.HANDCANNON, (int) ItemConstants_CH.MAGNUM_AMMO, 7, 0, weaponCustomUserData, ammoCount: 5));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.SHOTGUN_SHELLS, 7, 4, count));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.SUBMACHINE_GUN_AMMO, 7, 6, count));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.MAGNUM_AMMO, 8, 4, count));
                break;
            case Target.WITHOUT_BONUS_WEAPONS:
                // Row 3-4
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.ROCKET_LAUNCHER, 0, (int) ItemConstants_CH.ROCKET_LAUNCHER, 2, 0, weaponCustomUserData));
                // Row 5-6
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.ROCKET_LAUNCHER, 0, (int) ItemConstants_CH.ROCKET_LAUNCHER, 4, 0, weaponCustomUserData));
                // Row 7-8
                items.Add(Re4WeaponInstancer.NewWeapon(inventoryData.rsz, ItemConstants_CH.ROCKET_LAUNCHER, 0, (int) ItemConstants_CH.ROCKET_LAUNCHER, 6, 0, weaponCustomUserData));
                // Row 9
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.SHOTGUN_SHELLS, 8, 0, count));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.SUBMACHINE_GUN_AMMO, 8, 2, count));
                items.Add(Re4WeaponInstancer.NewItem(inventoryData.rsz, ItemConstants_CH.MAGNUM_AMMO, 8, 4, count));
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