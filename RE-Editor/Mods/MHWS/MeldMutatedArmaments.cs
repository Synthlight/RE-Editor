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
public class MeldMutatedArmaments : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Meld Mutated Armaments";
        const string description = "Adds the 'Mutated Armaments' to the 'Meld Relics' menu.";
        const string version     = "1.0";

        var mod = new NexusMod {
            Name    = name,
            Version = version,
            Desc    = description,
            Files   = [PathHelper.FACILITY_MELD_RELIC_DATA_PATH],
            Action  = ModFiles,
            Image   = $@"{PathHelper.MODS_PATH}\{name}\Pic.png"
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true);
    }

    public static void ModFiles(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_MakaData meldingData:
                    var data = App_user_data_MakaData_cData.Create(meldingData.rsz);
                    data._Index       = meldingData.Values.Count;
                    data.MakaType     = 1; // 0: Wyverian Melding, 1: Relic Melding
                    data.ItemId       = (int) ItemConstants.MUTATED_ARMAMENT;
                    data.NeedPoint    = 200;
                    data.StoryPackage = App_StoryPackageFlag_TYPE_Fixed.STORY_000489; // Match the 'Ancient Weapon Fragment'.
                    meldingData.Values.Add(data);
                    break;
            }
        }
    }
}