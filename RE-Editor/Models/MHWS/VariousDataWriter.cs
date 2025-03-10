#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using RE_Editor.Common;

namespace RE_Editor.Models;

public static class VariousDataWriter {
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
            if (target == VariousDataTweak.Target.WEAPON_DATA) {
                foreach (var type in Global.WEAPON_TYPES) {
                    writer.WriteLine($"local {type}Data = variousDataManager._Setting._EquipDatas._Weapon{type}._Values");
                }
            } else {
                writer.WriteLine($"local {GetTargetName(target)} = {GetTargetType(target)}");
            }
        }

        foreach (var (target, changes) in groupedChanges) {
            if (target == VariousDataTweak.Target.WEAPON_DATA) {
                foreach (var type in Global.WEAPON_TYPES) {
                    writer.WriteLine("");
                    writer.WriteLine($"for _, entry in pairs({type}Data) do");

                    foreach (var change in changes) {
                        change.Action(writer);
                    }

                    writer.WriteLine("end");
                }
            } else {
                writer.WriteLine("");
                writer.WriteLine($"for _, entry in pairs({GetTargetName(target)}) do");

                foreach (var change in changes) {
                    change.Action(writer);
                }

                writer.WriteLine("end");
            }
        }
    }

    private static string GetTargetName(VariousDataTweak.Target target) {
        return target switch {
            VariousDataTweak.Target.ARMOR_DATA => "armorData",
            VariousDataTweak.Target.DECORATION_DATA => "decorationData",
            VariousDataTweak.Target.ITEM_DATA => "itemData",
            VariousDataTweak.Target.SKILL_DATA => "skillData",
            VariousDataTweak.Target.TALISMAN_DATA => "talismanData",
            VariousDataTweak.Target.WEAPON_DATA => throw new("Can't get a single name here, it's split into separate fields; one for each weapon."),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static string GetTargetType(VariousDataTweak.Target target) {
        return target switch {
            VariousDataTweak.Target.ARMOR_DATA => "variousDataManager._Setting._EquipDatas._ArmorData._Values",
            VariousDataTweak.Target.DECORATION_DATA => "variousDataManager._Setting._EquipDatas._AccessoryData._Values",
            VariousDataTweak.Target.ITEM_DATA => "variousDataManager._Setting._ItemSetting._ItemData._Values",
            VariousDataTweak.Target.SKILL_DATA => "variousDataManager._Setting._SkillCommonData._Values",
            VariousDataTweak.Target.TALISMAN_DATA => "variousDataManager._Setting._EquipDatas._AmuletData._Values",
            VariousDataTweak.Target.WEAPON_DATA => throw new("Can't get a single path here, it's split into separate fields; one for each weapon."),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
    }
}