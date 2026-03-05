using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class NoDynamicDifficulty : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "No Dynamic Difficulty";
        const string description = "Makes all potential actions that affect it have a mult of 1.0f, thus doing nothing.";
        const string version     = "1.0";

        List<string> files = [
            @"\natives\STM\GameAssets\Character\AdditionalData\RankChapterSettings\cp_A000Chp4_30RankChapterSettingsUserData.user.3",
            @"\natives\STM\GameAssets\Character\AdditionalData\RankChapterSettings\cp_A000Chp5_03RankChapterSettingsUserData.user.3",
            @"\natives\STM\GameAssets\Character\AdditionalData\RankChapterSettings\cp_A000DefaultRankChapterSettingsUserData.user.3",
            @"\natives\STM\GameAssets\Character\AdditionalData\RankChapterSettings\cp_A100DefaultRankChapterSettingsUserData.user.3"
        ];

        var mod = new NexusMod {
            Name    = name,
            Version = version,
            Desc    = description,
            Files   = files,
            Action  = ModStuff
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true);
    }

    private static void ModStuff(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_RankChapterSettingsUserData rankData:
                    foreach (var settings in rankData.RankChapterSettingsList) {
                        settings.EmMoveFactor =
                            settings.EmDamageFactor =
                                settings.PlDamageFactor =
                                    settings.EmWinceFactor =
                                        settings.RankPointFactor = 1.0f;
                    }
                    break;
            }
        }
    }
}