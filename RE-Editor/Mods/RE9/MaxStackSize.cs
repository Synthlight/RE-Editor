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
        const string version     = "1.2";

        List<string> files = [
            PathHelper.ITEM_DATA_PATH
        ];

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Pic.png",
            Files        = files
        };

        var mods = new[] {
            baseMod
                .SetName("Stack Size (All): 00999")
                .SetAction(list => MaxStacks(list, Target._999, Type.FULL)),
            baseMod
                .SetName("Stack Size (All): 09999")
                .SetAction(list => MaxStacks(list, Target._9999, Type.FULL)),
            baseMod
                .SetName("Stack Size (All): 99999")
                .SetAction(list => MaxStacks(list, Target._99999, Type.FULL)),
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
                .SetAction(list => MaxStacks(list, Target.X10, Type.FULL))
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void MaxStacks(List<RszObject> rszObjectData, Target target, Type type) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_ItemCatalogUserData_ItemData itemData:
                    var itemId           = itemData.ItemIDStr;
                    var capacitySettings = itemData.SlotCapacitySetting[0];

                    var shouldAlter = capacitySettings.BaseCapacity > 1
                                      || itemId == ItemConstants.GREEN_HERB
                                      || itemId == ItemConstants.MIXED_HERB_G_PLUSG
                                      || itemId == ItemConstants.MIXED_HERB_G_PLUSG_PLUSG
                                      || itemId == ItemConstants.SCRAP
                                      || itemId == ItemConstants.RARE_METAL
                                      || itemId == ItemConstants.EMPTY_INJECTOR
                                      || itemId == ItemConstants.STEROIDS
                                      || itemId == ItemConstants.STABILIZER
                                      || itemId == ItemConstants.GUNPOWDER_SMALL
                                      || itemId == ItemConstants.GUNPOWDER_LARGE;

                    if (shouldAlter) {
                        var newMax = GetNewMax(target, capacitySettings);
                        capacitySettings.BaseCapacity = newMax;
                        if (newMax > capacitySettings.BaseItemBoxCapacity) {
                            capacitySettings.BaseItemBoxCapacity = newMax;
                        }
                    }

                    break;
            }
        }
    }

    private static int GetNewMax(Target target, App_InventorySlotCapacitySetting item) {
        var newMax = target switch {
            Target._999 => 999,
            Target._9999 => 9999,
            Target._99999 => 99999,
            Target.X2 => item.BaseCapacity * 2,
            Target.X3 => item.BaseCapacity * 3,
            Target.X4 => item.BaseCapacity * 4,
            Target.X5 => item.BaseCapacity * 5,
            Target.X10 => item.BaseCapacity * 10,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };
        return newMax;
    }

    public enum Target {
        _999,
        _9999,
        _99999,
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