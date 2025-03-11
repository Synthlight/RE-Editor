using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;

namespace RE_Editor.Mods;

/// <summary>
/// It's not perfect.
/// Doing it by scale works, but they grow/shrink over the span of a second or two, which makes the GS reach on draw attacks minuscule.
/// By position seems to work pretty well.
/// </summary>
[UsedImplicitly]
public class HiddenWeaponsWhenSheathed : IMod {
    [UsedImplicitly]
    public static void Make() {
        const string name        = "Hidden Weapons When Sheathed";
        const string description = "Hides weapons when sheathed.";
        const string version     = "1.0";

        var mod = new NexusMod {
            Name           = name,
            Desc           = description,
            Version        = version,
            Files          = PathHelper.GetAllWeaponVisualParamPaths(),
            FilteredAction = MakeHiddenWhenSheathed
        };

        ModMaker.WriteMods([mod], name, copyLooseToFluffy: true);
    }

    public static bool MakeHiddenWhenSheathed(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_PlayerWeaponVisualParam data:
                    var wasAltered = ModWeaponAttachInfo(data.AttachInfo[0]);
                    wasAltered = ModWeaponAttachInfo(data.SquatttachInfo[0]) || wasAltered;
                    return wasAltered;
            }
        }
        return false;
    }

    // Y Up, X Left, Z Forward
    private static bool ModWeaponAttachInfo(App_cWeaponAttachInfo attackInfo) {
        if (!attackInfo.UseBaseParam) {
            // Works, but they grow to size slowly which means missing stuff due to range on draw attacks.
            /*
            var scale = attackInfo.Scale[0];
            scale.X = 0;
            scale.Y = 0;
            scale.Z = 0;
            */

            // Doesn't have the same issue.
            var pos = attackInfo.Position[0];
            pos.X = 0;
            pos.Y = -1000;
            pos.Z = 0;
            return true;
        }
        return false;
    }
}