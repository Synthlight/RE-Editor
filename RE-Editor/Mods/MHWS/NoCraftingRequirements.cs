using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Constants;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class NoCraftingRequirements : IMod {
    [UsedImplicitly]
    public static void Make() {
        const string name        = "No Crafting Requirements";
        const string description = "No Crafting Requirements.";
        const string version     = "1.0.0";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description
        };

        var mods = new List<INexusMod> {
            baseMod
                .SetName("Armor (Normal)")
                .SetFiles([PathHelper.ARMOR_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL)),
            baseMod
                .SetName("Armor (Normal, Ignore Unlock Flags)")
                .SetFiles([PathHelper.ARMOR_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)),
            baseMod
                .SetName("Weapons")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Recipe))
                .SetAction(list => NoRequirements(list, Mode.NORMAL)),
            baseMod
                .SetName("Weapons (Ignore Unlock Flags)")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Recipe))
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)),
            baseMod
                .SetName("Amulets")
                .SetFiles([PathHelper.AMULETS_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL)),
            baseMod
                .SetName("Amulets (Ignore Unlock Flags)")
                .SetFiles([PathHelper.AMULETS_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)),
            baseMod
                .SetName("Kinsects")
                .SetFiles([PathHelper.KINSECT_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL)),
            baseMod
                .SetName("Kinsects (Ignore Unlock Flags)")
                .SetFiles([PathHelper.KINSECT_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)),
            baseMod
                .SetName("Palico")
                .SetFiles([PathHelper.OTOMO_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL)),
            baseMod
                .SetName("Palico (Ignore Unlock Flags)")
                .SetFiles([PathHelper.OTOMO_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS))
        };

        ModMaker.WriteMods(mods, name, copyLooseToFluffy: true, noPakZip: true);
    }

    public static void NoRequirements(List<RszObject> rszObjectData, Mode mode) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_AmuletRecipeData_cData amulet:
                    if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                        amulet.KeyEnemyId     = App_EnemyDef_ID_Fixed.INVALID;
                        amulet.KeyItemId      = (int) App_ItemDef_ID_Fixed.NONE;
                        amulet.KeyStoryNo     = App_MissionIDList_ID_Fixed.INVALID;
                        amulet.FlagHunterRank = 0;
                    }
                    if (mode.HasFlag(Mode.NORMAL)) {
                        foreach (var item in amulet.ItemId) {
                            item.Value = (int) ItemConstants.POTION;
                        }
                        foreach (var itemNum in amulet.ItemNum) {
                            itemNum.Value = 0;
                        }
                    }
                    break;
                case App_user_data_ArmorRecipeData_cData armor:
                    if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                        armor.KeyEnemyId     = App_EnemyDef_ID_Fixed.INVALID;
                        armor.KeyItemId      = (int) App_ItemDef_ID_Fixed.NONE;
                        armor.KeyStoryNo     = App_MissionIDList_ID_Fixed.INVALID;
                        armor.FlagHunterRank = 0;
                    }
                    if (mode.HasFlag(Mode.NORMAL)) {
                        foreach (var item in armor.Item) {
                            item.Value = (int) ItemConstants.POTION;
                        }
                        foreach (var itemNum in armor.ItemNum) {
                            itemNum.Value = 0;
                        }
                    }
                    break;
                case App_user_data_OtomoEquipRecipe_cData armor:
                    if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                        armor.KeyEnemyId     = App_EnemyDef_ID_Fixed.INVALID;
                        armor.KeyItemId      = (int) App_ItemDef_ID_Fixed.NONE;
                        armor.KeyStoryNo     = App_MissionIDList_ID_Fixed.INVALID;
                        armor.FlagHunterRank = 0;
                    }
                    if (mode.HasFlag(Mode.NORMAL)) {
                        foreach (var item in armor.Item) {
                            item.Value = (int) ItemConstants.POTION;
                        }
                        foreach (var itemNum in armor.ItemNum) {
                            itemNum.Value = 0;
                        }
                    }
                    break;
                case App_user_data_RodInsectRecipeData_cData kinsect:
                    if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                        kinsect.KeyEnemyId     = App_EnemyDef_ID_Fixed.INVALID;
                        kinsect.KeyItemId      = (int) App_ItemDef_ID_Fixed.NONE;
                        kinsect.KeyStoryNo     = App_MissionIDList_ID_Fixed.INVALID;
                        kinsect.FlagHunterRank = 0;
                    }
                    if (mode.HasFlag(Mode.NORMAL)) {
                        foreach (var item in kinsect.ItemId) {
                            item.Value = (int) ItemConstants.POTION;
                        }
                        foreach (var itemNum in kinsect.ItemNum) {
                            itemNum.Value = 0;
                        }
                    }
                    break;
                case App_user_data_WeaponRecipeData_cData weapon:
                    if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                        weapon.KeyEnemyId     = App_EnemyDef_ID_Fixed.INVALID;
                        weapon.KeyItemId      = (int) App_ItemDef_ID_Fixed.NONE;
                        weapon.KeyStoryNo     = App_MissionIDList_ID_Fixed.INVALID;
                        weapon.FlagHunterRank = 0;
                    }
                    if (mode.HasFlag(Mode.NORMAL)) {
                        foreach (var item in weapon.Item) {
                            item.Value = (int) ItemConstants.POTION;
                        }
                        foreach (var itemNum in weapon.ItemNum) {
                            itemNum.Value = 0;
                        }
                    }
                    break;
            }
        }
    }

    [Flags]
    public enum Mode {
        NORMAL = 1,
        IGNORE_UNLOCK_FLAGS
    }
}