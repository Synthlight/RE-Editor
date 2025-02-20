using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class InfiniteChargeBladeBuffs : IMod {
    [UsedImplicitly]
    public static void Make() {
        const string name        = "Infinite Charge Blade Buffs";
        var          description = $"Makes the change blade shield/sword buffs last {int.MaxValue} seconds.";
        const string version     = "1.0.0";

        var mod = new NexusMod {
            Name    = name,
            Version = version,
            Desc    = description,
            Files   = [PathHelper.CHARGE_BLADE_PARAM_PATH],
            Action  = ModFiles
        };

        ModMaker.WriteMods([mod], name, copyLooseToFluffy: true, noPakZip: true);
    }

    public static void ModFiles(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_Wp09ActionParam param:
                    param.ShieldEnhance_Time
                        = param.ShieldEnhance_MaxTime
                            = param.SwordEnhance_Time
                                = int.MaxValue;
                    break;
            }
        }
    }
}