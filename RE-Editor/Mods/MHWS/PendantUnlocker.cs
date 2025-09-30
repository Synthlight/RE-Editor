using System.Collections.Generic;
using System.IO;
using System.Linq;
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
public class PendantUnlocker : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Pendant Unlocker";
        const string description = "Unlock individual pendants.";
        const string version     = "1.1";

        var data     = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{PathHelper.PENDANT_DATA_PATH}");
        var pendants = data.rsz.GetEntryObject<App_user_data_CharmData>().Values.Cast<App_user_data_CharmData_cData>();
        var lockedPendants = (from pendant in pendants
                              where pendant.StoryFlag != App_StoryPackageFlag_TYPE_Fixed.INVALID || pendant.OwnedFlagIndex != -1
                              select pendant).ToList();

        var baseLuaMod = new VariousDataTweak {
            Version      = version,
            NameAsBundle = name,
            Desc         = description
        };

        List<INexusMod> mods = [];

        foreach (var pendant in lockedPendants) {
            mods.Add(baseLuaMod
                     .SetName($"{name} - {pendant.Name_.ToSafeName()}")
                     .SetNameOverride($"{name} - {pendant.Name_}")
                     .SetDefaultLuaName()
                     .SetChanges([
                         new() {
                             Target = VariousDataTweak.Target.PENDANT_DATA,
                             Action = writer => RemoveRequirements(writer, pendant)
                         }
                     ])
                     .SetSkipPak(true));
        }

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true, noPakZip: true);
    }

    public static void RemoveRequirements(StreamWriter writer, App_user_data_CharmData_cData pendant) {
        writer.WriteLine($"    if (entry._Type._Value == {(int) pendant.Type_Unwrapped}) then");
        writer.WriteLine($"        entry._StoryFlag = {(int) App_StoryPackageFlag_TYPE_Fixed.INVALID}");
        writer.WriteLine("        entry._OwnedFlagIndex = -1");
        writer.WriteLine("        entry:call(\"onLoad\")");
        writer.WriteLine("    end");
    }
}