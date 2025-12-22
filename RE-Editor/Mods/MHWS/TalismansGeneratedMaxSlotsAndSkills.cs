using System.Collections.Generic;
using System.IO;
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
public class TalismansGeneratedMaxSlotsAndSkills : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Talismans (Generated) - Max Slots & Skills";
        const string description = "Talismans (Generate/Appraised only) - Max Slots & Skills.";
        const string version     = "1.8";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description
        };

        var baseLuaMod = new VariousDataTweak {
            Version      = version,
            NameAsBundle = name,
            Desc         = description
        };

        var mods = new List<INexusMod> {
            baseMod
                .SetName("Talismans (Generated) - Max Slots & Skills (loose)")
                .SetFiles([
                    PathHelper.TALISMAN_GENERATED_SKILLS_DATA_PATH,
                    PathHelper.TALISMAN_GENERATED_SLOTS_DATA_PATH
                ])
                .SetAction(MaxSkillsAndSlots),
            baseLuaMod
                .SetName("Talismans (Generated) - Max Skills (REF)")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.TALISMAN_GENERATION_SKILL_DATA,
                        Action = MaxGeneratedSkillsRef
                    }
                ])
                .SetSkipPak(true),
            baseLuaMod
                .SetName("Talismans (Generated) - Max Slots (REF)")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.TALISMAN_GENERATION_SLOT_DATA,
                        Action = MaxGeneratedSlotsRef
                    }
                ])
                .SetSkipPak(true)
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void MaxSkillsAndSlots(List<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_RandomAmuletAccSlot_cData slotLot:
                    slotLot.SlotLevel01 = App_EquipDef_SlotLevel_Fixed.Lv3;
                    slotLot.SlotLevel02 = App_EquipDef_SlotLevel_Fixed.Lv3;
                    slotLot.SlotLevel03 = App_EquipDef_SlotLevel_Fixed.Lv3;
                    break;
                case App_user_data_RandomAmuletLotSkillTable_cData skillLot:
                    skillLot.SkillLv = 10;
                    break;
            }
        }
    }

    public static void MaxGeneratedSkillsRef(StreamWriter writer) {
        writer.WriteLine("    entry._SkillLv = 10");
    }

    public static void MaxGeneratedSlotsRef(StreamWriter writer) {
        writer.WriteLine($"    entry._SlotLevel01 = {(int) App_EquipDef_SlotLevel_Fixed.Lv3}");
        writer.WriteLine($"    entry._SlotLevel02 = {(int) App_EquipDef_SlotLevel_Fixed.Lv3}");
        writer.WriteLine($"    entry._SlotLevel03 = {(int) App_EquipDef_SlotLevel_Fixed.Lv3}");
    }
}