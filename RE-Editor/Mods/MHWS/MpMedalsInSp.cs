using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Constants;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class MpMedalsInSp : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "MP Medals in SP";
        const string description = "Changes the last 4 medals to unlock after hunting 1 large monster.";
        const string version     = "1.0";

        var mod = new NexusMod {
            Name    = name,
            Version = version,
            Desc    = description,
            Files   = [PathHelper.MEDAL_DATA_PATH],
            Action  = ModMedals
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true, noPakZip: true);
    }

    private static void ModMedals(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_MedalData_cData medal:
                    if (medal.IsHide) medal.IsHide = false;
                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (medal.MedalId_Unwrapped) {
                        case MedalConstants.HUNTERS_UNITED:
                        case MedalConstants.HUNTERS_UNITED_FOREVER:
                        case MedalConstants.GOSSIP_HUNTER:
                        case MedalConstants.NEWLY_FORGED_BONDS:
                            medal.OpenType_Unwrapped    = App_HunterProfileDef_OPEN_TYPE_Fixed.BOSS_HUNT;
                            medal.CountType_Unwrapped   = App_HunterProfileDef_COUNT_TYPE_Fixed.VETERAN_HUNT;
                            medal.IntParam              = 1;
                            medal.Stage_Unwrapped       = App_FieldDef_STAGE_Fixed.INVALID;
                            medal.MissionType_Unwrapped = App_MissionTypeList_TYPE_Fixed.INVALID;
                            medal.MissionID_Unwrapped   = App_MissionIDList_ID_Fixed.INVALID;
                            medal.LifeArea              = App_FieldDef_LIFE_AREA_Fixed.INVALID;
                            medal.EmID                  = (int) App_EnemyDef_ID_Fixed.INVALID;
                            medal.Environment_Unwrapped = App_EnvironmentType_ENVIRONMENT_Fixed.INVALID;
                            break;
                    }
                    break;
            }
        }
    }
}