using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class AkumaToolUnlocker : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Akuma Tool Unlocker";
        const string description = "Removes the story flags required to access the tool items.";
        const string version     = "1.0";

        List<string> files = [
            @"\natives\STM\GameDesign\Common\Item\FixItems.user.3"
        ];

        var mod = new NexusMod {
            Name    = name,
            Version = version,
            Desc    = description,
            Files   = files,
            Action  = ModStuff
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true, noPakZip: true);
    }

    private static void ModStuff(List<RszObject> rszObjects) {
        foreach (var obj in rszObjects) {
            switch (obj) {
                case App_user_data_FixItems_cData fixItem:
                    if (fixItem.StoryPackage == App_StoryPackageFlag_TYPE_Fixed.STORY_000707) {
                        fixItem.StoryPackage = App_StoryPackageFlag_TYPE_Fixed.INVALID;
                    }
                    break;
            }
        }
    }
}