#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using RE_Editor.Common;

namespace RE_Editor.Models;

public static class VariousDataWriter {
    public static readonly List<string> WEAPON_TYPES_GUNS_ONLY = ["LightBowgun", "HeavyBowgun"];

    public static void WriteTweak(VariousDataTweak tweak, string modFolderName) {
        var luaName = tweak.LuaName;
        var luaPath = $@"{PathHelper.MODS_PATH}\{modFolderName}\{luaName}";
        tweak.AdditionalFiles!.Add($@"reframework\autorun\{luaName}", luaPath);

        Directory.CreateDirectory(Path.GetDirectoryName(luaPath)!);
        using var writer = new StreamWriter(File.Create(luaPath));
        writer.WriteLine($"""
                          -- {tweak.Name}
                          -- By LordGregory

                          local version = "{tweak.Version}"
                          log.info("Initializing `{tweak.Name}` v"..version)

                          local variousDataManager = sdk.get_managed_singleton("app.VariousDataManager")
                          """);

        var groupedChanges = new Dictionary<VariousDataTweak.Target, List<VariousDataTweak.Change>>();
        foreach (var change in tweak.Changes) {
            if (!groupedChanges.ContainsKey(change.Target)) groupedChanges[change.Target] = [];
            groupedChanges[change.Target].Add(change);
        }

        foreach (var (target, _) in groupedChanges) {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (target) {
                case VariousDataTweak.Target.WEAPON_DATA: {
                    foreach (var type in Global.WEAPON_TYPES) {
                        writer.WriteLine($"local {type}Data = variousDataManager._Setting._EquipDatas._Weapon{type}._Values");
                    }
                    break;
                }
                case VariousDataTweak.Target.WEAPON_DATA_GUNS_ONLY: {
                    foreach (var type in WEAPON_TYPES_GUNS_ONLY) {
                        writer.WriteLine($"local {type}Data = variousDataManager._Setting._EquipDatas._Weapon{type}._Values");
                    }
                    break;
                }
                case VariousDataTweak.Target.WEAPON_RECIPE_DATA: {
                    foreach (var type in Global.WEAPON_TYPES) {
                        writer.WriteLine($"local {type}RecipeData = variousDataManager._Setting._EquipDatas._Weapon{type}Recipe._Values");
                    }
                    break;
                }
                case VariousDataTweak.Target.WEAPON_TREE_DATA: {
                    foreach (var type in Global.WEAPON_TYPES) {
                        writer.WriteLine($"local {type}TreeData = variousDataManager._Setting._EquipDatas._Weapon{type}Tree");
                    }
                    break;
                }
                default:
                    writer.WriteLine($"local {GetTargetName(target)} = {GetTargetType(target)}");
                    break;
            }
        }

        foreach (var (target, changes) in groupedChanges) {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (target) {
                case VariousDataTweak.Target.WEAPON_DATA: {
                    foreach (var type in Global.WEAPON_TYPES) {
                        writer.WriteLine("");
                        writer.WriteLine($"for _, entry in pairs({type}Data) do");

                        foreach (var change in changes) {
                            change.Action(writer);
                        }

                        writer.WriteLine("end");
                    }
                    break;
                }
                case VariousDataTweak.Target.WEAPON_DATA_GUNS_ONLY: {
                    foreach (var type in WEAPON_TYPES_GUNS_ONLY) {
                        writer.WriteLine("");
                        writer.WriteLine($"for _, entry in pairs({type}Data) do");

                        foreach (var change in changes) {
                            change.Action(writer);
                        }

                        writer.WriteLine("end");
                    }
                    break;
                }
                case VariousDataTweak.Target.WEAPON_RECIPE_DATA: {
                    foreach (var type in Global.WEAPON_TYPES) {
                        writer.WriteLine("");
                        writer.WriteLine($"for _, entry in pairs({type}RecipeData) do");

                        foreach (var change in changes) {
                            change.Action(writer);
                        }

                        writer.WriteLine("end");
                    }
                    break;
                }
                case VariousDataTweak.Target.WEAPON_TREE_DATA: {
                    foreach (var change in changes) {
                        change.Action(writer);
                    }
                    break;
                }
                default: {
                    writer.WriteLine("");
                    writer.WriteLine($"for _, entry in pairs({GetTargetName(target)}) do");

                    foreach (var change in changes) {
                        change.Action(writer);
                    }

                    writer.WriteLine("end");
                    break;
                }
            }
        }
    }

    private static string GetTargetName(VariousDataTweak.Target target) {
        return target switch {
            VariousDataTweak.Target.ARMOR_DATA => "armorData",
            VariousDataTweak.Target.ARMOR_RECIPE_DATA => "armorRecipeData",
            VariousDataTweak.Target.DECORATION_DATA => "decorationData",
            VariousDataTweak.Target.ITEM_DATA => "itemData",
            VariousDataTweak.Target.INSECT_DATA => "insectData",
            VariousDataTweak.Target.INSECT_RECIPE_DATA => "insectRecipeData",
            VariousDataTweak.Target.PALICO_ARMOR_DATA => "palicoArmorData",
            VariousDataTweak.Target.PALICO_WEAPON_DATA => "palicoWeaponData",
            VariousDataTweak.Target.PALICO_RECIPE_DATA => "palicoRecipeData",
            VariousDataTweak.Target.PENDANT_DATA => "pendantData",
            VariousDataTweak.Target.SKILL_DATA => "skillData",
            VariousDataTweak.Target.TALISMAN_DATA => "talismanData",
            VariousDataTweak.Target.TALISMAN_GENERATION_SKILL_DATA => "talismanGenerationSkillData",
            VariousDataTweak.Target.TALISMAN_GENERATION_SLOT_DATA => "talismanGenerationSlotData",
            VariousDataTweak.Target.TALISMAN_RECIPE_DATA => "talismanRecipeData",
            VariousDataTweak.Target.WEAPON_DATA => throw new("Can't get a single name here, it's split into separate fields; one for each weapon."),
            VariousDataTweak.Target.WEAPON_DATA_LBG => "LightBowgunData",
            VariousDataTweak.Target.WEAPON_DATA_HBG => "HeavyBowgunData",
            VariousDataTweak.Target.WEAPON_DATA_GUNS_ONLY => throw new("Can't get a single name here, it's split into separate fields; one for each weapon."),
            VariousDataTweak.Target.WEAPON_RECIPE_DATA => throw new("Can't get a single name here, it's split into separate fields; one for each weapon."),
            VariousDataTweak.Target.WEAPON_TREE_DATA => throw new("Can't get a single name here, it's split into separate fields; one for each weapon."),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static string GetTargetType(VariousDataTweak.Target target) {
        return target switch {
            VariousDataTweak.Target.ARMOR_DATA => "variousDataManager._Setting._EquipDatas._ArmorData._Values",
            VariousDataTweak.Target.ARMOR_RECIPE_DATA => "variousDataManager._Setting._EquipDatas._ArmorRecipe._Values",
            VariousDataTweak.Target.DECORATION_DATA => "variousDataManager._Setting._EquipDatas._AccessoryData._Values",
            VariousDataTweak.Target.ITEM_DATA => "variousDataManager._Setting._ItemSetting._ItemData._Values",
            VariousDataTweak.Target.INSECT_DATA => "variousDataManager._Setting._EquipDatas._RodInsectData._Values",
            VariousDataTweak.Target.INSECT_RECIPE_DATA => "variousDataManager._Setting._EquipDatas._RodInsectRecipeData._Values",
            VariousDataTweak.Target.PALICO_ARMOR_DATA => "variousDataManager._Setting._EquipDatas._OtomoArmorData._Values",
            VariousDataTweak.Target.PALICO_WEAPON_DATA => "variousDataManager._Setting._EquipDatas._OtomoWeaponData._Values",
            VariousDataTweak.Target.PALICO_RECIPE_DATA => "variousDataManager._Setting._EquipDatas._OtomoEquipRecipe._Values",
            VariousDataTweak.Target.PENDANT_DATA => "variousDataManager._Setting._EquipDatas._CharmData._Values",
            VariousDataTweak.Target.SKILL_DATA => "variousDataManager._Setting._SkillCommonData._Values",
            VariousDataTweak.Target.TALISMAN_DATA => "variousDataManager._Setting._EquipDatas._AmuletData._Values",
            VariousDataTweak.Target.TALISMAN_GENERATION_SKILL_DATA => "variousDataManager._Setting._RandomAmuletLotSkillTable._Values",
            VariousDataTweak.Target.TALISMAN_GENERATION_SLOT_DATA => "variousDataManager._Setting._RandomAmuletAccSlot._Values",
            VariousDataTweak.Target.TALISMAN_RECIPE_DATA => "variousDataManager._Setting._EquipDatas._AmuletRecipe._Values",
            VariousDataTweak.Target.WEAPON_DATA => throw new("Can't get a single path here, it's split into separate fields; one for each weapon."),
            VariousDataTweak.Target.WEAPON_DATA_LBG => "variousDataManager._Setting._EquipDatas._WeaponLightBowgun._Values",
            VariousDataTweak.Target.WEAPON_DATA_HBG => "variousDataManager._Setting._EquipDatas._WeaponHeavyBowgun._Values",
            VariousDataTweak.Target.WEAPON_DATA_GUNS_ONLY => throw new("Can't get a single path here, it's split into separate fields; one for each weapon."),
            VariousDataTweak.Target.WEAPON_RECIPE_DATA => throw new("Can't get a single path here, it's split into separate fields; one for each weapon."),
            VariousDataTweak.Target.WEAPON_TREE_DATA => throw new("Can't get a single path here, it's split into separate fields; one for each weapon."),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }
}