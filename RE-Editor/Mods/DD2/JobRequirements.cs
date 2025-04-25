using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
[SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
[SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
[SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
public class JobRequirements : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "No Job Requirements";
        const string description = "Tweaks weapon & armor data to let you use any armor/weapon with any job.";
        const string version     = "1.4";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Jobs.png",
        };

        var mods = new List<INexusMod> {
            baseMod
                .SetName($"{name}: All (+ Item Tweaks All-in-One)")
                .SetFiles([
                    PathHelper.ARMOR_DATA_PATH,
                    PathHelper.ITEM_DATA_PATH,
                    PathHelper.WEAPON_DATA_PATH,
                ])
                .SetAction(list => {
                    ItemTweaks.GoldCost(list, ItemTweaks.GoldOptions._1);
                    ItemTweaks.SellPrice(list, ItemTweaks.SellOptions.X10);
                    ItemTweaks.Weight(list, ItemTweaks.WeightOptions._0);
                    Jobs(list, JobOptions.ALL);
                }),
            baseMod
                .SetName($"{name}: Armors Only")
                .SetFiles([PathHelper.ARMOR_DATA_PATH])
                .SetAction(list => Jobs(list, JobOptions.ARMOR_ONLY)),
            new ItemDbTweak {
                Name            = $"{name}: Armors Only (REF)",
                NameAsBundle    = name,
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Jobs.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "NoJobRequirementsArmor.lua",
                SkipPak         = true,
                Changes = [
                    new() {
                        Target = ItemDbTweak.Target.ARMOR,
                        Action = writer => { writer.WriteLine($"        entry._{nameof(App_ItemEquipParam.Job)} = {ushort.MaxValue}"); }
                    }
                ],
            },
            baseMod
                .SetName($"{name}: Armors Only (No Weight)")
                .SetFiles([PathHelper.ARMOR_DATA_PATH])
                .SetAction(list => {
                    Jobs(list, JobOptions.ARMOR_ONLY);
                    ItemTweaks.Weight(list, ItemTweaks.WeightOptions._0);
                }),
            new ItemDbTweak {
                Name            = $"{name}: Armors Only (REF, No Weight)",
                NameAsBundle    = name,
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Jobs.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "NoJobRequirementsArmorNoWeight.lua",
                SkipPak         = true,
                Changes = [
                    new() {
                        Target = ItemDbTweak.Target.ARMOR,
                        Action = writer => {
                            writer.WriteLine($"        entry._{nameof(App_ItemEquipParam.Job)} = {ushort.MaxValue}");
                            writer.WriteLine($"        entry._{nameof(App_ItemCommonParam.Weight)} = 0");
                        }
                    }
                ],
            },
            baseMod
                .SetName($"{name}: Weapons Only")
                .SetFiles([PathHelper.WEAPON_DATA_PATH])
                .SetAction(list => Jobs(list, JobOptions.WEAPONS_ONLY)),
            new ItemDbTweak {
                Name            = $"{name}: Weapons Only (REF)",
                NameAsBundle    = name,
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Jobs.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "NoJobRequirementsWeapon.lua",
                SkipPak         = true,
                Changes = [
                    new() {
                        Target = ItemDbTweak.Target.WEAPON,
                        Action = writer => { writer.WriteLine($"        entry._{nameof(App_ItemEquipParam.Job)} = {ushort.MaxValue}"); }
                    }
                ],
            },
            baseMod
                .SetName($"{name}: Weapons Only (No Weight)")
                .SetFiles([PathHelper.WEAPON_DATA_PATH])
                .SetAction(list => {
                    Jobs(list, JobOptions.WEAPONS_ONLY);
                    ItemTweaks.Weight(list, ItemTweaks.WeightOptions._0);
                }),
            new ItemDbTweak {
                Name            = $"{name}: Weapons Only (REF, No Weight)",
                NameAsBundle    = name,
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Jobs.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "NoJobRequirementsWeaponNoWeight.lua",
                SkipPak         = true,
                Changes = [
                    new() {
                        Target = ItemDbTweak.Target.WEAPON,
                        Action = writer => {
                            writer.WriteLine($"        entry._{nameof(App_ItemEquipParam.Job)} = {ushort.MaxValue}");
                            writer.WriteLine($"        entry._{nameof(App_ItemCommonParam.Weight)} = 0");
                        }
                    }
                ],
            },
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void Jobs(List<RszObject> rszObjectData, JobOptions option) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_ItemWeaponParam itemData:
                    switch (option) {
                        case JobOptions.ALL:
                        case JobOptions.WEAPONS_ONLY:
                            itemData.Job = ushort.MaxValue;
                            break;
                    }
                    break;
                case App_ItemArmorParam itemData:
                    switch (option) {
                        case JobOptions.ALL:
                        case JobOptions.ARMOR_ONLY:
                            itemData.Job = ushort.MaxValue;
                            break;
                    }
                    break;
            }
        }
    }

    public enum JobOptions {
        ALL,
        ARMOR_ONLY,
        WEAPONS_ONLY,
    }
}