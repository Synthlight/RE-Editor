using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class WeaponsPurpleSharpness : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Weapons - Max Purple Sharpness";
        const string description = "Weapons - Max Purple Sharpness.";
        const string version     = "1.6";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Pic.png"
        };

        var baseLuaMod = new VariousDataTweak {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Pic.png"
        };

        var mods = new List<INexusMod> {
            baseMod
                .SetName($"{name} (PAK)")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Base))
                .SetAction(MaxSharpness),
            baseLuaMod
                .SetName($"{name} (REF)")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.WEAPON_DATA,
                        Action = MaxSharpnessRef
                    }
                ])
                .SetSkipPak(true)
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void MaxSharpness(List<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_WeaponData_cData weapon:
                    if (weapon.SharpnessValList[0].Value > 0) {
                        weapon.SharpnessValList[0].Value = 10;
                        weapon.SharpnessValList[1].Value = 10;
                        weapon.SharpnessValList[2].Value = 10;
                        weapon.SharpnessValList[3].Value = 10;
                        weapon.SharpnessValList[4].Value = 10;
                        weapon.SharpnessValList[5].Value = 10;
                        weapon.SharpnessValList[6].Value = 340;
                        foreach (var handicraft in weapon.TakumiValList) {
                            handicraft.Value = 0;
                        }
                    }
                    break;
            }
        }
    }

    public static void MaxSharpnessRef(StreamWriter writer) {
        writer.WriteLine("    if (entry._SharpnessValList[0].m_value > 0) then");
        writer.WriteLine("        entry._SharpnessValList[0] = 10");
        writer.WriteLine("        entry._SharpnessValList[1] = 10");
        writer.WriteLine("        entry._SharpnessValList[2] = 10");
        writer.WriteLine("        entry._SharpnessValList[3] = 10");
        writer.WriteLine("        entry._SharpnessValList[4] = 10");
        writer.WriteLine("        entry._SharpnessValList[5] = 10");
        writer.WriteLine("        entry._SharpnessValList[6] = 340");
        writer.WriteLine("        for handicraftIndex = 0, entry._TakumiValList:get_size() - 1 do");
        writer.WriteLine("            entry._TakumiValList[handicraftIndex] = 0");
        writer.WriteLine("        end");
        writer.WriteLine("    end");
    }
}