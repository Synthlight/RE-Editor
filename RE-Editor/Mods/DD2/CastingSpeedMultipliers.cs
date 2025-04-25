using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Generated.Models;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class CastingSpeedMultipliers : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Casting Speed Multipliers";
        const string description = "Casting speed multipliers.";
        const string version     = "1.3";

        var dataFiles = new Dictionary<TargetOptions, List<string>> {
            [TargetOptions.MAGE]         = [PathHelper.JOB_03_PARAM_PATH],
            [TargetOptions.MAGIC_ARCHER] = [PathHelper.JOB_08_PARAM_PATH],
            [TargetOptions.SORCERER]     = [PathHelper.JOB_06_PARAM_PATH],
        };

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
        };

        var mods = (from option in Enum.GetValues<TargetOptions>()
                    from value in Enum.GetValues<ValueOptions>()
                    let optionName = GetOptionName(option)
                    let valueName = GetValueName(value)
                    select baseMod.SetName($"Casting Speed ({optionName}): {valueName}")
                                  .SetFiles(dataFiles[option])
                                  .SetAction(list => Mod(list, value))).ToList();

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void Mod(List<RszObject> rszObjectData, ValueOptions option) {
        var multiplier = GetMultiplier(option);

        foreach (var obj in rszObjectData) {
            switch (obj) {
                case ICastingSpeed.IAddRate param:
                    param.AddRateForPreparingSpellWhileConcentrate *= multiplier;
                    param.AddRateForPreparingSpellWhileLevitating  *= multiplier;
                    param.AddRateForPreparingSpellWhileRunning     *= multiplier;
                    param.AddRateForPreparingSpellWhileWalking     *= multiplier;
                    break;
                case ICastingSpeed.IPrepTime param:
                    param.PrepareTime *= multiplier;
                    break;
                case ICastingSpeed.ISecPrepare param:
                    param.SecPrepare *= multiplier;
                    break;
                case ICastingSpeed.ISecPrepareAndFocus param:
                    param.SecPrepare            *= multiplier;
                    param.SecPrepareFocus       *= multiplier;
                    param.SecPrepareFocusAndLv2 *= multiplier;
                    param.SecPrepareLv2         *= multiplier;
                    break;
                case App_Job08Parameter_MultiLockOnParameter param:
                    param.AnotherLockOnExternalFrame *= multiplier;
                    break;
                case App_Job08Parameter_MultiLockOnParameter_MultiParam param:
                    param.MultiLockOnFrame *= multiplier;
                    break;
            }
        }
    }

    public enum TargetOptions {
        MAGE,
        MAGIC_ARCHER,
        SORCERER,
    }

    private static object GetOptionName(TargetOptions option) {
        return option switch {
            TargetOptions.MAGE => "Mage",
            TargetOptions.MAGIC_ARCHER => "Magic Archer",
            TargetOptions.SORCERER => "Sorcerer",
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
        };
    }

    public enum ValueOptions {
        _0,
        _1,
        _2,
        _3,
        _4,
        _5,
        _6,
        _7,
        _8,
        _9,
    }

    private static string GetValueName(ValueOptions option) {
        return option switch {
            ValueOptions._0 => "Instant",
            ValueOptions._1 => "x0.1",
            ValueOptions._2 => "x0.2",
            ValueOptions._3 => "x0.3",
            ValueOptions._4 => "x0.4",
            ValueOptions._5 => "x0.5",
            ValueOptions._6 => "x0.6",
            ValueOptions._7 => "x0.7",
            ValueOptions._8 => "x0.8",
            ValueOptions._9 => "x0.9",
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
        };
    }

    private static float GetMultiplier(ValueOptions option) {
        return option switch {
            ValueOptions._0 => 0.001f,
            ValueOptions._1 => 0.1f,
            ValueOptions._2 => 0.2f,
            ValueOptions._3 => 0.3f,
            ValueOptions._4 => 0.4f,
            ValueOptions._5 => 0.5f,
            ValueOptions._6 => 0.6f,
            ValueOptions._7 => 0.7f,
            ValueOptions._8 => 0.8f,
            ValueOptions._9 => 0.9f,
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
        };
    }
}