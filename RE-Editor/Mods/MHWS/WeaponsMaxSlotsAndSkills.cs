﻿using System.Collections.Generic;
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
public class WeaponsMaxSlotsAndSkills : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Weapons - Max Slots & Skills";
        const string description = "Weapons - Max Slots & Skills.";
        const string version     = "1.5";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Equipment.png"
        };

        var baseLuaMod = new VariousDataTweak {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Equipment.png"
        };

        var mods = new List<INexusMod> {
            baseMod
                .SetName("Weapons - Max Slots Only (PAK)")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Base))
                .SetAction(MaxSlots),
            baseLuaMod
                .SetName("Weapons - Max Slots Only (REF)")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.WEAPON_DATA,
                        Action = MaxSlotsRef
                    }
                ])
                .SetSkipPak(true),
            baseMod
                .SetName("Weapons - Max Skills Only (PAK)")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Base))
                .SetAction(MaxSkills),
            baseLuaMod
                .SetName("Weapons - Max Skills Only (REF)")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.WEAPON_DATA,
                        Action = MaxSkillsRef
                    }
                ])
                .SetSkipPak(true),
            baseMod
                .SetName("Weapons - Max Slots & Skills (PAK)")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Base))
                .SetAction(data => {
                    MaxSlots(data);
                    MaxSkills(data);
                })
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void MaxSlots(List<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_WeaponData_cData weapon:
                    foreach (var slotLevel in weapon.SlotLevel) {
                        slotLevel.Value = 3;
                    }
                    break;
            }
        }
    }

    public static void MaxSlotsRef(StreamWriter writer) {
        writer.WriteLine("    for _, slotLevel in pairs(entry._SlotLevel) do");
        writer.WriteLine("        slotLevel._Value = 3");
        writer.WriteLine("    end");
    }

    public static void MaxSkills(List<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_WeaponData_cData weapon:
                    foreach (var skillLevel in weapon.SkillLevel) {
                        if (skillLevel.Value > 0) {
                            skillLevel.Value = 10;
                        }
                    }
                    break;
            }
        }
    }

    public static void MaxSkillsRef(StreamWriter writer) {
        writer.WriteLine("    for skillIndex = 0, entry._SkillLevel:get_size() - 1 do");
        writer.WriteLine("        if (entry._SkillLevel[skillIndex].m_value >= 1) then");
        writer.WriteLine("            entry._SkillLevel[skillIndex] = 10");
        writer.WriteLine("        end");
        writer.WriteLine("    end");
    }
}