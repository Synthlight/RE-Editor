using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Constants;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class MaxStackSize : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Stack Size Changes";
        const string description = "Modifies the stack size of stackable items.";
        const string version     = "1.9";

        var itemDataFiles = new List<string> {
            PathHelper.ITEM_DATA_PATH,
            PathHelper.ITEM_DATA_PATH_AO,
            PathHelper.ITEM_DATA_PATH_AO_OVR,
            PathHelper.ITEM_DATA_PATH_MC,
            PathHelper.ITEM_DATA_PATH_MC_OVR,
        };

        var itemFilesWithDragCraft = new List<string>(itemDataFiles) {
            PathHelper.ITEM_DRAG_CRAFT_DATA_PATH
        };

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Files        = itemDataFiles
        };

        var mods = new[] {
            baseMod
                .SetName("Stack Size (All): 0999")
                .SetAction(list => MaxStacks(list, Target._999, Type.FULL)),
            baseMod
                .SetName("Stack Size (All): 9999")
                .SetAction(list => MaxStacks(list, Target._9999, Type.FULL)),
            baseMod
                .SetName("Stack Size (All): x02")
                .SetAction(list => MaxStacks(list, Target.X2, Type.FULL)),
            baseMod
                .SetName("Stack Size (All): x03")
                .SetAction(list => MaxStacks(list, Target.X3, Type.FULL)),
            baseMod
                .SetName("Stack Size (All): x04")
                .SetAction(list => MaxStacks(list, Target.X4, Type.FULL)),
            baseMod
                .SetName("Stack Size (All): x05")
                .SetAction(list => MaxStacks(list, Target.X5, Type.FULL)),
            baseMod
                .SetName("Stack Size (All): x10")
                .SetAction(list => MaxStacks(list, Target.X10, Type.FULL)),
            baseMod
                .SetName("Stack Size (All + Herbs): 0999")
                .SetAction(list => MaxStacks(list, Target._999, Type.FULL_WITH_HERBS))
                .SetFiles(itemFilesWithDragCraft),
            baseMod
                .SetName("Stack Size (All + Herbs): 9999")
                .SetAction(list => MaxStacks(list, Target._9999, Type.FULL_WITH_HERBS))
                .SetFiles(itemFilesWithDragCraft),
            baseMod
                .SetName("Stack Size (All + Herbs): x02")
                .SetAction(list => MaxStacks(list, Target.X2, Type.FULL_WITH_HERBS))
                .SetFiles(itemFilesWithDragCraft),
            baseMod
                .SetName("Stack Size (All + Herbs): x03")
                .SetAction(list => MaxStacks(list, Target.X3, Type.FULL_WITH_HERBS))
                .SetFiles(itemFilesWithDragCraft),
            baseMod
                .SetName("Stack Size (All + Herbs): x04")
                .SetAction(list => MaxStacks(list, Target.X4, Type.FULL_WITH_HERBS))
                .SetFiles(itemFilesWithDragCraft),
            baseMod
                .SetName("Stack Size (All + Herbs): x05")
                .SetAction(list => MaxStacks(list, Target.X5, Type.FULL_WITH_HERBS))
                .SetFiles(itemFilesWithDragCraft),
            baseMod
                .SetName("Stack Size (All + Herbs): x10")
                .SetAction(list => MaxStacks(list, Target.X10, Type.FULL_WITH_HERBS))
                .SetFiles(itemFilesWithDragCraft),
            baseMod
                .SetName("Stack Size (Ammo Only): 0999")
                .SetAction(list => MaxStacks(list, Target._999, Type.AMMO_ONLY)),
            baseMod
                .SetName("Stack Size (Ammo Only): 9999")
                .SetAction(list => MaxStacks(list, Target._9999, Type.AMMO_ONLY)),
            baseMod
                .SetName("Stack Size (Ammo Only): x02")
                .SetAction(list => MaxStacks(list, Target.X2, Type.AMMO_ONLY)),
            baseMod
                .SetName("Stack Size (Ammo Only): x03")
                .SetAction(list => MaxStacks(list, Target.X3, Type.AMMO_ONLY)),
            baseMod
                .SetName("Stack Size (Ammo Only): x04")
                .SetAction(list => MaxStacks(list, Target.X4, Type.AMMO_ONLY)),
            baseMod
                .SetName("Stack Size (Ammo Only): x05")
                .SetAction(list => MaxStacks(list, Target.X5, Type.AMMO_ONLY)),
            baseMod
                .SetName("Stack Size (Ammo Only): x10")
                .SetAction(list => MaxStacks(list, Target.X10, Type.AMMO_ONLY)),
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void MaxStacks(List<RszObject> rszObjectData, Target target, Type type) {
        var sizeChanged = new HashSet<int>();

        foreach (var obj in rszObjectData) {
            switch (obj) {
                case Chainsaw_ItemDefinitionUserData_Data itemData:
                    var item = itemData.ItemDefineData[0];

                    // Because Hand Grenades have an ItemDefinition stack size of 60 for some bizarre reason.
                    if (itemData.ItemId == ItemConstants_CH.HAND_GRENADE
                        || itemData.ItemId == ItemConstants_CH.FLASH_GRENADE
                        || itemData.ItemId == ItemConstants_CH.HEAVY_GRENADE
                        || itemData.ItemId == ItemConstants_CH.GOLD_CHICKEN_EGG
                        || itemData.ItemId == ItemConstants_CH.BROWN_CHICKEN_EGG
                        || itemData.ItemId == ItemConstants_CH.CHICKEN_EGG
                       ) {
                        item.StackMax = itemData.WeaponDefineData[0].StackMax;
                    }

                    switch (type) {
                        case Type.FULL_WITH_HERBS: {
                            if (itemData.ItemId == ItemConstants_CH.MIXED_HERB_G_PLUSG
                                || itemData.ItemId == ItemConstants_CH.MIXED_HERB_G_PLUSR
                                || itemData.ItemId == ItemConstants_CH.MIXED_HERB_G_PLUSY
                                || itemData.ItemId == ItemConstants_CH.GREEN_HERB
                                || itemData.ItemId == ItemConstants_CH.RED_HERB
                                || itemData.ItemId == ItemConstants_CH.YELLOW_HERB) {
                                var newMax = GetNewMax(target, item);

                                if (sizeChanged.Contains(itemData.ItemId)) break;
                                if (item.StackMax < newMax) {
                                    item.StackMax = newMax.Clamp(1, 9999);
                                    sizeChanged.Add(itemData.ItemId);
                                }
                            }
                            goto case Type.FULL;
                        }
                        case Type.AMMO_ONLY: {
                            if (itemData.ItemId == ItemConstants_CH.HANDGUN_AMMO
                                || itemData.ItemId == ItemConstants_CH.SUBMACHINE_GUN_AMMO
                                || itemData.ItemId == ItemConstants_CH.MAGNUM_AMMO
                                || itemData.ItemId == ItemConstants_CH.SHOTGUN_SHELLS
                                || itemData.ItemId == ItemConstants_CH.RIFLE_AMMO
                                || itemData.ItemId == ItemConstants_CH.ATTACHABLE_MINES
                                || itemData.ItemId == ItemConstants_CH.BOLTS
                                || itemData.ItemId == ItemConstants_MC.BLAST_ARROWS
                                || itemData.ItemId == ItemConstants_MC.EXPLOSIVE_ARROWS) {
                                var newMax = GetNewMax(target, item);

                                if (sizeChanged.Contains(itemData.ItemId)) break;
                                if (item.StackMax < newMax) {
                                    item.StackMax = newMax.Clamp(1, 9999);
                                    sizeChanged.Add(itemData.ItemId);
                                }
                            }
                            break;
                        }
                        case Type.FULL: {
                            if ((item.StackMax > 1
                                 || itemData.ItemId == ItemConstants_CH.FIRST_AID_SPRAY
                                 || itemData.ItemId == ItemConstants_CH.GOLD_CHICKEN_EGG
                                 || itemData.ItemId == ItemConstants_CH.BROWN_CHICKEN_EGG
                                 || itemData.ItemId == ItemConstants_CH.CHICKEN_EGG
                                 || itemData.ItemId == ItemConstants_CH.HAND_GRENADE
                                 || itemData.ItemId == ItemConstants_CH.FLASH_GRENADE
                                 || itemData.ItemId == ItemConstants_CH.HEAVY_GRENADE
                                 || itemData.ItemId == ItemConstants_CH.RESOURCES_S
                                 || itemData.ItemId == ItemConstants_CH.RESOURCES_L
                                 || itemData.ItemId == ItemConstants_CH.VIPER
                                 || itemData.ItemId == ItemConstants_CH.BLACK_BASS
                                 || itemData.ItemId == ItemConstants_CH.BLACK_BASS_L
                                 || itemData.ItemId == ItemConstants_CH.LUNKER_BASS
                                 || itemData.ItemId == ItemConstants_AO.LORD_OF_THE_WATERWAY
                                 || itemData.ItemId == ItemConstants_CH.MIXED_HERB_G_PLUSG_PLUSG
                                 || itemData.ItemId == ItemConstants_CH.MIXED_HERB_G_PLUSG_PLUSY
                                 || itemData.ItemId == ItemConstants_CH.MIXED_HERB_G_PLUSR_PLUSY
                                 || itemData.ItemId == ItemConstants_CH.RHINOCEROS_BEETLE
                                )
                                && itemData.ItemId != ItemConstants_CH.PESETAS
                               ) {
                                var newMax = GetNewMax(target, item);

                                if (sizeChanged.Contains(itemData.ItemId)) break;
                                if (item.StackMax < newMax) {
                                    item.StackMax = newMax.Clamp(1, 9999);
                                    sizeChanged.Add(itemData.ItemId);
                                }

                                // The few that have the stack size in two locations since weapon data (the 'clip') has its own max that should match.
                                if (itemData.ItemId == ItemConstants_CH.HAND_GRENADE
                                    || itemData.ItemId == ItemConstants_CH.FLASH_GRENADE
                                    || itemData.ItemId == ItemConstants_CH.HEAVY_GRENADE
                                    || itemData.ItemId == ItemConstants_CH.GOLD_CHICKEN_EGG
                                    || itemData.ItemId == ItemConstants_CH.BROWN_CHICKEN_EGG
                                    || itemData.ItemId == ItemConstants_CH.CHICKEN_EGG
                                   ) {
                                    itemData.WeaponDefineData[0].StackMax = item.StackMax;
                                }
                            }
                            break;
                        }
                        default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                    break;
                case Chainsaw_ItemCombineDefinitionUserData craftData:
                    craftData.Datas.Clear();
                    break;
            }
        }
    }

    private static int GetNewMax(Target target, Chainsaw_ItemDefiniition item) {
        var newMax = target switch {
            Target._999 => 999,
            Target._9999 => 9999,
            Target.X2 => item.StackMax * 2,
            Target.X3 => item.StackMax * 3,
            Target.X4 => item.StackMax * 4,
            Target.X5 => item.StackMax * 5,
            Target.X10 => item.StackMax * 10,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
        return newMax;
    }

    public enum Target {
        _999,
        _9999,
        X2,
        X3,
        X4,
        X5,
        X10,
    }

    public enum Type {
        FULL,
        FULL_WITH_HERBS,
        AMMO_ONLY,
    }
}