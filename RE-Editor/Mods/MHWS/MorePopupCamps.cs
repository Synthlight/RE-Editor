using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class MorePopupCamps : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "More Safe Spaces";
        const string description = "Makes all camp areas safe and optionally increases the base limit.";
        const string version     = "1.7";

        var baseMod = new NexusMod {
            NameAsBundle = name,
            Desc         = description,
            Version      = version,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Thumb.png",
            Files = PathHelper.GetAllCampSafetyFilePaths()
                              .Append([PathHelper.POPUP_CAMP_PATH])
        };

        var mods = new List<INexusMod> {
            baseMod
                .SetName($"{name} (No Count Change)")
                .SetAction(list => MoreSafeSpaces(list, 0)),
            baseMod
                .SetName($"{name} (+05)")
                .SetAction(list => MoreSafeSpaces(list, 5)),
            baseMod
                .SetName($"{name} (+10)")
                .SetAction(list => MoreSafeSpaces(list, 10)),
            baseMod
                .SetName($"{name} (+15)")
                .SetAction(list => MoreSafeSpaces(list, 15)),
            baseMod
                .SetName($"{name} (+50)")
                .SetAction(list => MoreSafeSpaces(list, 50))
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void MoreSafeSpaces(IList<RszObject> rszObjectData, int countAdd) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_CampManagerSetting settings:
                    var campMaxNum = settings.CampMaxNum[0];
                    campMaxNum.Core =
                        campMaxNum.Desert =
                            campMaxNum.Forest =
                                campMaxNum.Oil =
                                    campMaxNum.ShowOBT =
                                        campMaxNum.Wall += countAdd;

                    var repoTime                          = settings.RepoTimerLimit[0];
                    repoTime.Danger = repoTime.LittleSafe = repoTime.Safe = 1; // Insta-repair?
                    break;
                case App_user_data_Gm800_AaaUniqueParam settings:
                    settings.RiskDegree     = App_user_data_Gm800_AaaUniqueParam_RISK_DEGREE.SAFE;
                    settings.IsEnableScared = false;
                    var enemySightInfo = settings.EnemySightInfo[0];
                    enemySightInfo.LengthRate =
                        enemySightInfo.RangeRate =
                            enemySightInfo.FeelRate = 0;
                    break;
            }
        }
    }
}