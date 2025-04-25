using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

/// <summary>
/// Works mostly.
/// Known issues:
///  - Skills are sorted by how many points you have in it, THEN by name.
/// </summary>
[UsedImplicitly]
public class SortedSkills : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name                   = "Sorted Skills";
        const string descriptionSkillSoring = "Resorts skills by the name of the skill.";
        const string version                = "1.0";

        var skillCommonData = ReDataFile.Read(PathHelper.CHUNK_PATH + PathHelper.SKILL_COMMON_DATA_PATH).rsz.objectData.OfType<App_user_data_SkillCommonData_cData>().ToList();

        var baseMod = new NexusMod {
            Version = version,
            //Image = $@"{PathHelper.MODS_PATH}\{name}\Pic.png"
        };

        var baseLuaMod = new VariousDataTweak {
            Version = version,
            //Image = $@"{PathHelper.MODS_PATH}\{name}\Pic.png"
        };

        List<INexusMod> mods = [
            new NexusMod {
                Name    = name,
                Version = version,
                Desc    = "Activating this entry does nothing, it exists solely to create the submenu.",
                Files   = [],
                SkipPak = true
            }
        ];

        foreach (var lang in Global.LANGUAGES) {
            var langGroupName = $"Sorting Language- {lang} (Skill)";

            mods.AddRange(new List<INexusMod> {
                new NexusMod {
                    Name     = langGroupName,
                    AddonFor = name,
                    Version  = version,
                    Desc     = "Activating this entry does nothing, it exists solely to create the second submenu.",
                    Files    = [],
                    SkipPak  = true
                }
            });

            AddSkillsSortedBySkillNameMods(mods, baseMod, baseLuaMod, lang, langGroupName, skillCommonData, descriptionSkillSoring);
        }

        ModMaker.WriteMods(mainWindow, mods.OfType<NexusMod>(), name, copyLooseToFluffy: true);
    }

    private static void AddSkillsSortedBySkillNameMods(List<INexusMod> mods, NexusMod baseMod, VariousDataTweak baseLuaMod, Global.LangIndex lang, string langGroupName, List<App_user_data_SkillCommonData_cData> data, string description) {
        var skillsSortedByName = GetSkillsSortedBySkillName(lang, data);

        mods.AddRange(new List<INexusMod> {
            baseMod
                .SetName($"Sort Skills by Name (PAK, {lang})")
                .SetDesc(description)
                .SetAddonFor(langGroupName)
                .SetFiles([PathHelper.SKILL_COMMON_DATA_PATH])
                .SetAction(list => ApplySkillSort(list, skillsSortedByName)),
            baseLuaMod
                .SetName($"Sort Skills by Name (REF, {lang})")
                .SetDesc(description)
                .SetAddonFor(langGroupName)
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.SKILL_DATA,
                        Action = writer => ApplySkillSortRef(writer, skillsSortedByName)
                    }
                ])
                .SetSkipPak(true)
        });
    }

    private static Dictionary<App_HunterDef_Skill_Fixed, int> GetSkillsSortedBySkillName(Global.LangIndex lang, List<App_user_data_SkillCommonData_cData> skillCommonData) {
        var sortedSkills = (from skill in skillCommonData
                            where skill.SkillId != 0 // Skip 'None'.
                            orderby DataHelper.SKILL_NAME_BY_ENUM_VALUE[lang][skill.SkillId]
                            select (App_HunterDef_Skill_Fixed) skill.SkillId).ToList();
        Dictionary<App_HunterDef_Skill_Fixed, int> dict = new(sortedSkills.Count);
        for (var i = 0; i < sortedSkills.Count; i++) {
            dict[sortedSkills[i]] = i + 100;
        }
        return dict;
    }

    public static void ApplySkillSort(List<RszObject> rszObjectData, Dictionary<App_HunterDef_Skill_Fixed, int> sortedSkills) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_SkillCommonData_cData skill:
                    if (skill.SkillId != (int) App_HunterDef_Skill_Fixed.NONE) {
                        skill.SortId = sortedSkills[(App_HunterDef_Skill_Fixed) skill.SkillId];
                    }
                    break;
            }
        }
    }

    public static void ApplySkillSortRef(StreamWriter writer, Dictionary<App_HunterDef_Skill_Fixed, int> sortedSkills) {
        foreach (var (skillId, sortOrder) in sortedSkills) {
            writer.WriteLine($"    if (entry._skillId == {(int) skillId}) then");
            writer.WriteLine($"        entry._SortId = {sortOrder}");
            writer.WriteLine("    end");
        }
    }
}