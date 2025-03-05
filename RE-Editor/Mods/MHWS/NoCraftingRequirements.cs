using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
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
        const string version     = "1.1";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description
        };

        var mods = new List<INexusMod> {
            baseMod
                .SetName("Armor (Normal)")
                .SetFiles([PathHelper.ARMOR_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL))
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Armor.png"),
            baseMod
                .SetName("Armor (Normal, Ignore Unlock Flags)")
                .SetFiles([PathHelper.ARMOR_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS))
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Armor.png"),
            baseMod
                .SetName("Weapons")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Recipe))
                .SetAction(list => NoRequirements(list, Mode.NORMAL))
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Weapons.png"),
            baseMod
                .SetName("Weapons (Ignore Unlock Flags)")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Recipe)
                                    .Append(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Tree)))
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS))
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Weapons.png"),
            baseMod
                .SetName("Talismans")
                .SetFiles([PathHelper.TALISMAN_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL))
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Talismans.png"),
            baseMod
                .SetName("Talismans (Ignore Unlock Flags)")
                .SetFiles([PathHelper.TALISMAN_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS))
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Talismans.png"),
            baseMod
                .SetName("Kinsects")
                .SetFiles([PathHelper.KINSECT_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL))
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Kinsects.png"),
            baseMod
                .SetName("Kinsects (Ignore Unlock Flags)")
                .SetFiles([PathHelper.KINSECT_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS))
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Kinsects.png"),
            baseMod
                .SetName("Palico")
                .SetFiles([PathHelper.OTOMO_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL))
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Otomo.png"),
            baseMod
                .SetName("Palico (Ignore Unlock Flags)")
                .SetFiles([PathHelper.OTOMO_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS))
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Otomo.png")
        };

        ModMaker.WriteMods(mods, name, copyLooseToFluffy: true);
    }

    public static void NoRequirements(List<RszObject> rszObjectData, Mode mode) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_AmuletRecipeData_cData talisman:
                    if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                        talisman.KeyEnemyId     = App_EnemyDef_ID_Fixed.INVALID;
                        talisman.KeyItemId      = (int) App_ItemDef_ID_Fixed.NONE;
                        talisman.KeyStoryNo     = App_MissionIDList_ID_Fixed.INVALID;
                        talisman.FlagHunterRank = 0;
                        foreach (var item in talisman.ItemId) {
                            item.Value = (int) App_ItemDef_ID_Fixed.NONE;
                        }
                    }
                    if (mode.HasFlag(Mode.NORMAL)) {
                        foreach (var itemNum in talisman.ItemNum) {
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
                        foreach (var item in armor.Item) {
                            item.Value = (int) App_ItemDef_ID_Fixed.NONE;
                        }
                    }
                    if (mode.HasFlag(Mode.NORMAL)) {
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
                        foreach (var item in armor.Item) {
                            item.Value = (int) App_ItemDef_ID_Fixed.NONE;
                        }
                    }
                    if (mode.HasFlag(Mode.NORMAL)) {
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
                        foreach (var item in kinsect.ItemId) {
                            item.Value = (int) App_ItemDef_ID_Fixed.NONE;
                        }
                    }
                    if (mode.HasFlag(Mode.NORMAL)) {
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
                        foreach (var item in weapon.Item) {
                            item.Value = (int) App_ItemDef_ID_Fixed.NONE;
                        }
                    }
                    if (mode.HasFlag(Mode.NORMAL)) {
                        foreach (var itemNum in weapon.ItemNum) {
                            itemNum.Value = 0;
                        }
                    }
                    break;
                case App_user_data_WeaponTree_cColumnData data:
                    if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                        data.StoryFlag = App_MissionIDList_ID.INVALID;
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