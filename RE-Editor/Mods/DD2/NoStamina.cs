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
public class NoStamina : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "No Stamina Consumed";
        const string description = "Changes stamina use.";
        const string version     = "1.7";

        var descriptionOptions = new Dictionary<StaminaOptions, string> {
            [StaminaOptions.JOB_SKILLS]    = "Changes job skills' stamina use.",
            [StaminaOptions.OUT_OF_COMBAT] = "Changes common action (dash/climb/jump/carry/etc) stamina use.",
        };

        var dataFiles = new Dictionary<StaminaOptions, List<string>> {
            [StaminaOptions.JOB_SKILLS] = [],
            [StaminaOptions.OUT_OF_COMBAT] = [
                PathHelper.STAMINA_COMMON_ACTION_PARAM_PATH,
                PathHelper.STAMINA_PARAM_PATH,
            ],
        };
        for (var i = 1; i <= 10; i++) {
            dataFiles[StaminaOptions.JOB_SKILLS]
                .AddRange([
                    $@"natives\STM\AppSystem\ch\Common\Human\UserData\Parameter\Job{i:00}StaminaParameter.user.2",
                    PathHelper.JOB_04_PARAM_PATH,
                    PathHelper.JOB_07_PARAM_PATH,
                ]);
        }

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
        };

        var mods = (from option in Enum.GetValues<StaminaOptions>()
                    from value in Enum.GetValues<StaminaValueOptions>()
                    let optionName = GetOptionName(option)
                    let valueName = GetValueName(value)
                    select baseMod.SetName($"Stamina Use ({optionName}): {valueName}")
                                  .SetDesc(descriptionOptions[option])
                                  .SetFiles(dataFiles[option])
                                  .SetAction(list => Stamina(list, option, value))).ToList();

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void Stamina(List<RszObject> rszObjectData, StaminaOptions option, StaminaValueOptions value) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_HumanStaminaParameterAdditional data:
                    if (option is StaminaOptions.JOB_SKILLS or StaminaOptions.OUT_OF_COMBAT) {
                        data.Value = GetNewStaminaValue(data.Value, value);
                        data.Rate  = GetNewStaminaValue(data.Rate, value);
                    }
                    break;
                case App_HumanStaminaParameter_ConsumeData data:
                    if (option is StaminaOptions.OUT_OF_COMBAT) {
                        data.VeryLight = GetNewStaminaValue(data.VeryLight, value);
                        data.Light     = GetNewStaminaValue(data.Light, value);
                        data.Middle    = GetNewStaminaValue(data.Middle, value);
                        data.Heavy     = GetNewStaminaValue(data.Heavy, value);
                        data.VeryHeavy = GetNewStaminaValue(data.VeryHeavy, value);
                        data.Over      = GetNewStaminaValue(data.Over, value);
                    }
                    break;
                case IConsumeStamina.ILvl1 data:
                    data.ConsumeStamina = GetNewStaminaValue(data.ConsumeStamina, value);
                    break;
                case IConsumeStamina.ILvl2 data:
                    data.ConsumeStaminaLv2 = GetNewStaminaValue(data.ConsumeStaminaLv2, value);
                    break;
            }
        }
    }

    private static float GetNewStaminaValue(float consumption, StaminaValueOptions value) {
        return value switch {
            StaminaValueOptions._0 => consumption * 0f,
            StaminaValueOptions.HALF => consumption * 0.5f,
            StaminaValueOptions.X2 => consumption * 2f,
            StaminaValueOptions.X5 => consumption * 5f,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    public enum StaminaOptions {
        OUT_OF_COMBAT,
        JOB_SKILLS,
    }

    private static string GetOptionName(StaminaOptions option) {
        return option switch {
            StaminaOptions.OUT_OF_COMBAT => "Common Actions Only",
            StaminaOptions.JOB_SKILLS => "Job Skills Only",
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
        };
    }

    public enum StaminaValueOptions {
        _0,
        HALF,
        X2,
        X5,
    }

    private static string GetValueName(StaminaValueOptions option) {
        return option switch {
            StaminaValueOptions._0 => "None",
            StaminaValueOptions.HALF => "Half",
            StaminaValueOptions.X2 => "x2",
            StaminaValueOptions.X5 => "x5",
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
        };
    }
}