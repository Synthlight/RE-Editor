﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class MinMaxMonsterSize : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Min-Max Legal Monster Sizes";
        const string description = "Guarantees a 50/50 min/max legal size for monsters. Will be a crown if the size probabilities would allow it.";
        const string version     = "1.4";

        var mod = new NexusMod {
            Name    = name,
            Version = version,
            Desc    = description,
            Files   = [PathHelper.MONSTER_RANDOM_SIZES_PATH],
            Action  = ModProbabilities,
            Image   = $@"{PathHelper.MODS_PATH}\{name}\Gold Crown.png"
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true);
    }

    public static void ModProbabilities(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_EmParamRandomSize_cRandomSizeData table:
                    // Ignore single size targets.
                    if (table.ProbDataTbl.Count(data => data.Prob > 0) == 1) break;

                    var lowestProbIndex  = table.ProbDataTbl.FirstIndexOf(data => data.Prob > 0);
                    var highestProbIndex = table.ProbDataTbl.LastIndexOf(data => data.Prob > 0);

                    Debug.Assert(lowestProbIndex != highestProbIndex); // Shouldn't ever get here with the first check.

                    table.ProbDataTbl[lowestProbIndex].Prob  = 50;
                    table.ProbDataTbl[highestProbIndex].Prob = 50;

                    // There should only be values between the lowest/highest that need alteration.
                    for (var i = lowestProbIndex + 1; i <= highestProbIndex - 1; i++) {
                        table.ProbDataTbl[i].Prob = 0;
                    }
                    break;
            }
        }
    }
}