using System;
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
public class HideSheathedWeapons : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Hide Sheathed Weapons";
        const string description = "Hides weapons when sheathed.";
        const string version     = "1.0";

        var mod = new NexusMod {
            Version = version,
            Name    = name,
            Desc    = description,
            Files   = [PathHelper.WEAPON_SETTINGS_PATH],
            Action  = ShopData,
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true);
    }

    public static void ShopData(List<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_WeaponSetting_OffsetSetting param:
                    // From a 3rd person camera viewpoint.
                    // X: Negative back, positive forward.
                    // Y: Negative down, positive up.
                    // Z: Negative left, positive right.
                    try {
                        param.SheatheSetting[0].LocalPosition[0].Y = -5000f;
                    } catch (Exception) {
                        // ignored
                    }
                    break;
            }
        }
    }
}