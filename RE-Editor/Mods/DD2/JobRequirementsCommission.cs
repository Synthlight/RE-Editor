using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
[SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
[SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
[SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
public class JobRequirementsCommission : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "No Job Requirements (Commission)";
        const string description = "Tweaks weapon & armor data to let you use any armor/weapon with any job.";
        const string version     = "1.1";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = "No Job Requirements",
            Desc         = description,
            Image        = @"R:\Games\Dragons Dogma 2\Mods\No Job Requirements\Jobs.png",
        };

        var mods = new[] {
            baseMod
                .SetDesc("Commission: Armor with no weight + Seven Rings Enhanced (x10)")
                .SetName("No Job Requirements: Armors Only (No Weight + Seven Rings Enhanced (x10))")
                .SetFiles([PathHelper.ARMOR_DATA_PATH])
                .SetAction(list => {
                    JobRequirements.Jobs(list, JobRequirements.JobOptions.ARMOR_ONLY);
                    ItemTweaks.Weight(list, ItemTweaks.WeightOptions._0);
                    Mod(list);
                }),
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void Mod(List<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_ItemArmorParam itemData:
                    switch (itemData.Id) {
                        case (int) ItemConstants.RING_OF_ARTICULACY:
                            itemData.SpecialValue = 99;
                            break;
                        case (int) ItemConstants.RING_OF_ACCRUAL:
                        case (int) ItemConstants.RING_OF_AMBITION:
                        case (int) ItemConstants.RING_OF_ENDEAVOR:
                        case (int) ItemConstants.RING_OF_AGGRESSION:
                        case (int) ItemConstants.RING_OF_PERCIPIENCE:
                            itemData.SpecialValue *= 10;
                            break;
                        case (int) ItemConstants.RING_OF_TRIUMPH:
                            itemData.SpecialValue = itemData.SpecialValue2 = 1000;
                            break;
                    }
                    break;
            }
        }
    }
}