using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
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
        const string version     = "1.0";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Files        = [PathHelper.ITEM_DATA_PATH]
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
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void MaxStacks(List<RszObject> rszObjectData, Target target, Type type) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_ItemSpecificationData_SpecUnit_BasicSpec itemData:
                    if (itemData.MaxStackSize > 1) {
                        var newMax = GetNewMax(target, itemData);

                        itemData.MaxStackSize = newMax;
                    }
                    break;
            }
        }
    }

    private static int GetNewMax(Target target, App_ItemSpecificationData_SpecUnit_BasicSpec item) {
        var newMax = target switch {
            Target._999 => 999,
            Target._9999 => 9999,
            Target.X2 => item.MaxStackSize * 2,
            Target.X3 => item.MaxStackSize * 3,
            Target.X4 => item.MaxStackSize * 4,
            Target.X5 => item.MaxStackSize * 5,
            Target.X10 => item.MaxStackSize * 10,
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