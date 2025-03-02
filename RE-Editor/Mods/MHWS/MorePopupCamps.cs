using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class MorePopupCamps : IMod {
    [UsedImplicitly]
    public static void Make() {
        const string name        = "More Safe Spaces";
        const string description = "Raises the limit on Popup Camps and makes them all 'safe'.";
        const string version     = "1.0.0";

        var mod = new NexusMod {
            Name    = name,
            Desc    = description,
            Version = version,
            Image   = $@"{PathHelper.MODS_PATH}\More Safe Spaces\Thumb.png",
            Files = PathHelper.GetAllCampSafetyFilePaths()
                              .Append([PathHelper.POPUP_CAMP_PATH]),
            Action = MoreSafeSpaces
        };

        ModMaker.WriteMods([mod], name, copyLooseToFluffy: true, noPakZip: true);
    }

    public static void MoreSafeSpaces(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_CampManagerSetting settings:
                    var campMaxNum = settings.CampMaxNum[0];
                    campMaxNum.Core =
                        campMaxNum.Desert =
                            campMaxNum.Forest =
                                campMaxNum.Oil =
                                    campMaxNum.ShowOBT =
                                        campMaxNum.Wall = 50;

                    var repoTime                          = settings.RepoTimerLimit[0];
                    repoTime.Danger = repoTime.LittleSafe = repoTime.Safe = 1; // Insta-repair?
                    break;
                case App_user_data_Gm800_AaaUniqueParam settings:
                    settings.RiskDegree     = App_user_data_Gm800_AaaUniqueParam_RISK_DEGREE.SAFE;
                    settings.IsEnableScared = false;
                    var enemySightInfo = settings.EnemySightInfo[0];
                    enemySightInfo.LengthRate =
                        enemySightInfo.RangeRate =
                            enemySightInfo.FeelRate = 1;
                    break;
            }
        }
    }
}