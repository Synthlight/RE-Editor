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
public class MoreBlood : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Collect More Blood";
        const string description = "Modifies the capacity of the Blood Collector.";
        const string version     = "1.0";

        List<string> files = [
            @"\natives\STM\LevelDesign\Item\UserData\ItemEffectData\it99_06_000.user.3",
            @"\natives\STM\LevelDesign\Item\UserData\ItemEffectData\it99_06_003.user.3",
            @"\natives\STM\LevelDesign\Item\UserData\ItemEffectData\it99_06_010.user.3"
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
                .SetName("Collect More Blood: 00999")
                .SetAction(list => MaxStacks(list, Target._999)),
            baseMod
                .SetName("Collect More Blood: 09999")
                .SetAction(list => MaxStacks(list, Target._9999)),
            baseMod
                .SetName("Collect More Blood: 99999")
                .SetAction(list => MaxStacks(list, Target._99999)),
            baseMod
                .SetName("Collect More Blood: x02")
                .SetAction(list => MaxStacks(list, Target.X2)),
            baseMod
                .SetName("Collect More Blood: x03")
                .SetAction(list => MaxStacks(list, Target.X3)),
            baseMod
                .SetName("Collect More Blood: x04")
                .SetAction(list => MaxStacks(list, Target.X4)),
            baseMod
                .SetName("Collect More Blood: x05")
                .SetAction(list => MaxStacks(list, Target.X5)),
            baseMod
                .SetName("Collect More Blood: x10")
                .SetAction(list => MaxStacks(list, Target.X10))
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void MaxStacks(List<RszObject> rszObjectData, Target target) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_ItemStockData_Native itemData:
                    itemData.Stock = GetNewMax(target, itemData.Stock);

                    break;
            }
        }
    }

    private static int GetNewMax(Target target, int baseCapacity) {
        var newMax = target switch {
            Target._999 => 999,
            Target._9999 => 9999,
            Target._99999 => 99999,
            Target.X2 => baseCapacity * 2,
            Target.X3 => baseCapacity * 3,
            Target.X4 => baseCapacity * 4,
            Target.X5 => baseCapacity * 5,
            Target.X10 => baseCapacity * 10,
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
}