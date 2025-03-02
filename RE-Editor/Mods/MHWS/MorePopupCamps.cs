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
        const string name        = "More Popup Camps";
        const string description = "Raises the limit on Popup Camps.";
        const string version     = "1.0.0";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description
        };

        var camps = PathHelper.GetAllCampSafetyFilePaths();

        var mods = new List<INexusMod> {
            baseMod
                .SetName($"{name}")
                .SetFiles([PathHelper.POPUP_CAMP_PATH])
                .SetAction(HigherCampLimits),
            baseMod
                .SetName("Camps Always Safe")
                .SetFiles(camps)
                .SetAction(HigherCampLimits),
        };

        ModMaker.WriteMods(mods, name, copyLooseToFluffy: true, noPakZip: true);
    }

    public static void HigherCampLimits(IList<RszObject> rszObjectData) {
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
                    settings.RiskDegree = App_user_data_Gm800_AaaUniqueParam_RISK_DEGREE.SAFE;
                    break;
            }
        }
    }
}