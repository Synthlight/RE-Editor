using System;
using System.Collections.Generic;
using System.IO;
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
        const string version     = "1.3";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description
        };

        var baseLuaMod = new VariousDataTweak {
            Version      = version,
            NameAsBundle = name,
            Desc         = description
        };

        var mods = new List<INexusMod> {
            baseMod
                .SetName("Armor (PAK)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Armor.png")
                .SetFiles([PathHelper.ARMOR_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL)),
            baseLuaMod
                .SetName("Armor (REF)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Armor.png")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.ARMOR_RECIPE_DATA,
                        Action = writer => NoRequirementsRef(writer, RefTarget.ARMOR, Mode.NORMAL)
                    }
                ])
                .SetSkipPak(true),
            baseMod
                .SetName("Armor (Ignore Unlock Flags) (PAK)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Armor.png")
                .SetFiles([PathHelper.ARMOR_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)),
            baseLuaMod
                .SetName("Armor (Ignore Unlock Flags) (REF)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Armor.png")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.ARMOR_RECIPE_DATA,
                        Action = writer => NoRequirementsRef(writer, RefTarget.ARMOR, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)
                    }
                ])
                .SetSkipPak(true),
            baseMod
                .SetName("Kinsects (PAK)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Kinsects.png")
                .SetFiles([PathHelper.KINSECT_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL)),
            baseLuaMod
                .SetName("Kinsects (REF)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Kinsects.png")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.INSECT_RECIPE_DATA,
                        Action = writer => NoRequirementsRef(writer, RefTarget.INSECTS, Mode.NORMAL)
                    }
                ])
                .SetSkipPak(true),
            baseMod
                .SetName("Kinsects (Ignore Unlock Flags) (PAK)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Kinsects.png")
                .SetFiles([PathHelper.KINSECT_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)),
            baseLuaMod
                .SetName("Kinsects (Ignore Unlock Flags) (REF)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Kinsects.png")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.INSECT_RECIPE_DATA,
                        Action = writer => NoRequirementsRef(writer, RefTarget.INSECTS, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)
                    }
                ])
                .SetSkipPak(true),
            baseMod
                .SetName("Palico (PAK)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Otomo.png")
                .SetFiles([PathHelper.OTOMO_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL)),
            baseLuaMod
                .SetName("Palico (REF)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Otomo.png")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.PALICO_RECIPE_DATA,
                        Action = writer => NoRequirementsRef(writer, RefTarget.PALICO, Mode.NORMAL)
                    }
                ])
                .SetSkipPak(true),
            baseMod
                .SetName("Palico (Ignore Unlock Flags) (PAK)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Otomo.png")
                .SetFiles([PathHelper.OTOMO_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)),
            baseLuaMod
                .SetName("Palico (Ignore Unlock Flags) (REF)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Otomo.png")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.PALICO_RECIPE_DATA,
                        Action = writer => NoRequirementsRef(writer, RefTarget.PALICO, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)
                    }
                ])
                .SetSkipPak(true),
            baseMod
                .SetName("Talismans (PAK)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Talismans.png")
                .SetFiles([PathHelper.TALISMAN_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL)),
            baseLuaMod
                .SetName("Talismans (REF)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Talismans.png")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.TALISMAN_RECIPE_DATA,
                        Action = writer => NoRequirementsRef(writer, RefTarget.TALISMANS, Mode.NORMAL)
                    }
                ])
                .SetSkipPak(true),
            baseMod
                .SetName("Talismans (Ignore Unlock Flags) (PAK)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Talismans.png")
                .SetFiles([PathHelper.TALISMAN_RECIPE_DATA_PATH])
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)),
            baseLuaMod
                .SetName("Talismans (Ignore Unlock Flags) (REF)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Talismans.png")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.TALISMAN_RECIPE_DATA,
                        Action = writer => NoRequirementsRef(writer, RefTarget.TALISMANS, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)
                    }
                ])
                .SetSkipPak(true),
            baseMod
                .SetName("Weapons (PAK)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Weapons.png")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Recipe))
                .SetAction(list => NoRequirements(list, Mode.NORMAL)),
            baseLuaMod
                .SetName("Weapons (REF)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Weapons.png")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.WEAPON_RECIPE_DATA,
                        Action = writer => NoRequirementsRef(writer, RefTarget.WEAPONS, Mode.NORMAL)
                    }
                ])
                .SetSkipPak(true),
            baseMod
                .SetName("Weapons (Ignore Unlock Flags) (PAK)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Weapons.png")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Recipe)
                                    .Append(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Tree)))
                .SetAction(list => NoRequirements(list, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)),
            baseLuaMod
                .SetName("Weapons (Ignore Unlock Flags) (REF)")
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Weapons.png")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.WEAPON_RECIPE_DATA,
                        Action = writer => NoRequirementsRef(writer, RefTarget.WEAPONS, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)
                    },
                    new() {
                        Target = VariousDataTweak.Target.WEAPON_TREE_DATA,
                        Action = writer => NoRequirementsRef(writer, RefTarget.WEAPON_TREES, Mode.NORMAL | Mode.IGNORE_UNLOCK_FLAGS)
                    }
                ])
                .SetSkipPak(true)
        };

        ModMaker.WriteMods(mods, name, copyLooseToFluffy: true);
    }

    public static void NoRequirements(List<RszObject> rszObjectData, Mode mode) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_ArmorRecipeData_cData armor: // Armor
                    if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                        armor.KeyEnemyId     = (int) App_EnemyDef_ID_Fixed.INVALID;
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
                case App_user_data_RodInsectRecipeData_cData kinsect: // Insect
                    if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                        kinsect.KeyEnemyId     = (int) App_EnemyDef_ID_Fixed.INVALID;
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
                case App_user_data_OtomoEquipRecipe_cData armor: // Palico Equip
                    if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                        armor.KeyEnemyId     = (int) App_EnemyDef_ID_Fixed.INVALID;
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
                case App_user_data_AmuletRecipeData_cData talisman: // Talisman
                    if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                        talisman.KeyEnemyId     = (int) App_EnemyDef_ID_Fixed.INVALID;
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
                case App_user_data_WeaponRecipeData_cData weapon: // Weapon
                    if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                        weapon.KeyEnemyId     = (int) App_EnemyDef_ID_Fixed.INVALID;
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
                case App_user_data_WeaponTree_cColumnData data: // Weapon Tree Columns
                    if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                        data.StoryFlag = App_MissionIDList_ID.INVALID;
                    }
                    break;
            }
        }
    }

    public static void NoRequirementsRef(StreamWriter writer, RefTarget target, Mode mode) {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (target) {
            case RefTarget.ARMOR:
            case RefTarget.INSECTS:
            case RefTarget.PALICO:
            case RefTarget.TALISMANS:
            case RefTarget.WEAPONS:
                if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                    writer.WriteLine($"    entry._KeyEnemyId = {(int) App_EnemyDef_ID_Fixed.INVALID}");
                    writer.WriteLine($"    entry._KeyItemId = {(int) App_ItemDef_ID_Fixed.NONE}");
                    writer.WriteLine($"    entry._KeyStoryNo = {(int) App_MissionIDList_ID_Fixed.INVALID}");
                    writer.WriteLine("    entry._FlagHunterRank = 0");
                }
                break;
        }
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (target) {
            case RefTarget.ARMOR:
            case RefTarget.PALICO:
            case RefTarget.WEAPONS:
                if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                    writer.WriteLine("    for i = 0, entry._Item:get_size() - 1 do");
                    writer.WriteLine($"        entry._Item[i] = {(int) App_ItemDef_ID_Fixed.NONE}");
                    writer.WriteLine("    end");
                }
                if (mode.HasFlag(Mode.NORMAL)) {
                    writer.WriteLine("    for i = 0, entry._ItemNum:get_size() - 1 do");
                    //writer.WriteLine("    for i = 1, #entry._ItemNum do");
                    writer.WriteLine("        entry._ItemNum[i] = 0");
                    writer.WriteLine("    end");
                }
                writer.WriteLine("    entry:call(\"onLoad\")");
                break;
            case RefTarget.INSECTS:
            case RefTarget.TALISMANS:
                if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                    writer.WriteLine("    for i = 0, entry._ItemId:get_size() - 1 do");
                    writer.WriteLine($"        entry._ItemId[i] = {(int) App_ItemDef_ID_Fixed.NONE}");
                    writer.WriteLine("    end");
                }
                if (mode.HasFlag(Mode.NORMAL)) {
                    writer.WriteLine("    for i = 0, entry._ItemNum:get_size() - 1 do");
                    writer.WriteLine("        entry._ItemNum[i] = 0");
                    writer.WriteLine("    end");
                }
                writer.WriteLine("    entry:call(\"onLoad\")");
                break;
            case RefTarget.WEAPON_TREES:
                if (mode.HasFlag(Mode.IGNORE_UNLOCK_FLAGS)) {
                    writer.WriteLine("");
                    foreach (var type in Global.WEAPON_TYPES) {
                        writer.WriteLine($"for _, columnData in pairs({type}TreeData._ColumnDataList) do");
                        writer.WriteLine($"    columnData._StoryFlag = {(int) App_MissionIDList_ID.INVALID}");
                        writer.WriteLine("end");
                    }
                }
                break;
        }
    }

    [Flags]
    public enum Mode {
        NORMAL              = 1 << 0,
        IGNORE_UNLOCK_FLAGS = 1 << 1
    }

    public enum RefTarget {
        ARMOR,
        WEAPONS,
        WEAPON_TREES,
        TALISMANS,
        INSECTS,
        PALICO
    }
}