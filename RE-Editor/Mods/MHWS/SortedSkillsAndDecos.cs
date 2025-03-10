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

namespace RE_Editor.Mods;

[UsedImplicitly]
public class SortedSkillsAndDecos : IMod {
    [UsedImplicitly]
    public static void Make() {
        const string name                   = "Sorted Skills and Decos";
        const string descriptionByDeco      = "Resorts decorations by the name of the decoration.";
        const string descriptionBySkill     = "Resorts decorations by the name of the skill, ordering by either the first or second skill, for decos with two skills.";
        const string descriptionSkillSoring = "Resorts skills by the name of the skill.";
        const string version                = "1.0";

        var decoData        = ReDataFile.Read(PathHelper.CHUNK_PATH + PathHelper.DECORATION_DATA_PATH).rsz.objectData.OfType<App_user_data_AccessoryData_cData>().ToList();
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
            //var langName                     = Global.LANGUAGE_NAME_LOOKUP[lang];
            var langGroupName = $"Sorting Language- {lang}";

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

            AddDecosSortedBySkillNameMods(mods, baseMod, baseLuaMod, lang, langGroupName, decoData, descriptionBySkill);
            AddDecosSortedByDecoNameMods(mods, baseMod, baseLuaMod, lang, langGroupName, decoData, descriptionByDeco);
            AddSkillsSortedBySkillNameMods(mods, baseMod, baseLuaMod, lang, langGroupName, skillCommonData, descriptionSkillSoring);
        }

        ModMaker.WriteMods(mods, name, copyLooseToFluffy: true);
    }

    private static void AddDecosSortedBySkillNameMods(List<INexusMod> mods, NexusMod baseMod, VariousDataTweak baseLuaMod, Global.LangIndex lang, string langGroupName, List<App_user_data_AccessoryData_cData> data, string description) {
        var decosSortedByFirstSkillName  = GetDecoSortedBySkillName(lang, data, false);
        var decosSortedBySecondSkillName = GetDecoSortedBySkillName(lang, data, true);

        mods.AddRange(new List<INexusMod> {
            baseMod
                .SetName($"Sort Decos by Skill Name (Skill1, Skill2) (PAK, {lang})")
                .SetDesc(description)
                .SetAddonFor(langGroupName)
                .SetFiles([PathHelper.DECORATION_DATA_PATH])
                .SetAction(list => ApplyDecoSort(list, decosSortedByFirstSkillName)),
            baseMod
                .SetName($"Sort Decos by Skill Name (Skill2, Skill1) (PAK, {lang})")
                .SetDesc(description)
                .SetAddonFor(langGroupName)
                .SetFiles([PathHelper.DECORATION_DATA_PATH])
                .SetAction(list => ApplyDecoSort(list, decosSortedBySecondSkillName)),
            baseLuaMod
                .SetName($"Sort Decos by Skill Name (Skill1, Skill2) (REF, {lang})")
                .SetDesc(description)
                .SetAddonFor(langGroupName)
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.DECORATION_DATA,
                        Action = writer => ApplyDecoSortRef(writer, decosSortedByFirstSkillName)
                    }
                ])
                .SetSkipPak(true),
            baseLuaMod
                .SetName($"Sort Decos by Skill Name (Skill2, Skill1) (REF, {lang})")
                .SetDesc(description)
                .SetAddonFor(langGroupName)
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.DECORATION_DATA,
                        Action = writer => ApplyDecoSortRef(writer, decosSortedBySecondSkillName)
                    }
                ])
                .SetSkipPak(true)
        });
    }

    private static void AddDecosSortedByDecoNameMods(List<INexusMod> mods, NexusMod baseMod, VariousDataTweak baseLuaMod, Global.LangIndex lang, string langGroupName, List<App_user_data_AccessoryData_cData> data, string description) {
        var decosSortedByDecoName = GetDecoSortedByDecoName(lang, data);

        mods.AddRange(new List<INexusMod> {
            baseMod
                .SetName($"Sort Decos by Deco Name (PAK, {lang})")
                .SetDesc(description)
                .SetAddonFor(langGroupName)
                .SetFiles([PathHelper.DECORATION_DATA_PATH])
                .SetAction(list => ApplyDecoSort(list, decosSortedByDecoName)),
            baseLuaMod
                .SetName($"Sort Decos by Deco Name (REF, {lang})")
                .SetDesc(description)
                .SetAddonFor(langGroupName)
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.DECORATION_DATA,
                        Action = writer => ApplyDecoSortRef(writer, decosSortedByDecoName)
                    }
                ])
                .SetSkipPak(true)
        });
    }

    private static Dictionary<App_EquipDef_ACCESSORY_ID_Fixed, int> GetDecoSortedBySkillName(Global.LangIndex lang, List<App_user_data_AccessoryData_cData> decoData, bool sortBySecondSkillFirst) {
        var sortedDecos = (from deco in decoData
                           let sortSkill = deco.Skill[1].Value == 0 ? deco.Skill[0].Value :
                               sortBySecondSkillFirst               ? deco.Skill[1].Value : deco.Skill[0].Value
                           orderby DataHelper.SKILL_NAME_BY_ENUM_VALUE[lang][sortSkill]
                           select deco.AccessoryId_Unwrapped).ToList();
        Dictionary<App_EquipDef_ACCESSORY_ID_Fixed, int> dict = new(decoData.Count);
        for (var i = 0; i < decoData.Count; i++) {
            dict[sortedDecos[i]] = i + 100;
        }
        return dict;
    }

    private static Dictionary<App_EquipDef_ACCESSORY_ID_Fixed, int> GetDecoSortedByDecoName(Global.LangIndex lang, List<App_user_data_AccessoryData_cData> decoData) {
        var sortedDecos = (from deco in decoData
                           orderby DataHelper.DECORATION_INFO_LOOKUP_BY_GUID[lang][deco.Name.Value]
                           select deco.AccessoryId_Unwrapped).ToList();
        Dictionary<App_EquipDef_ACCESSORY_ID_Fixed, int> dict = new(decoData.Count);
        for (var i = 0; i < decoData.Count; i++) {
            dict[sortedDecos[i]] = i + 100;
        }
        return dict;
    }

    public static void ApplyDecoSort(List<RszObject> rszObjectData, Dictionary<App_EquipDef_ACCESSORY_ID_Fixed, int> sortedDecos) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_AccessoryData_cData deco:
                    deco.SortId = (uint) sortedDecos[deco.AccessoryId_Unwrapped];
                    break;
            }
        }
    }

    public static void ApplyDecoSortRef(StreamWriter writer, Dictionary<App_EquipDef_ACCESSORY_ID_Fixed, int> sortedDecos) {
        foreach (var (decoId, sortOrder) in sortedDecos) {
            writer.WriteLine($"    if (entry._AccessoryId._Value == {(int) decoId}) then");
            writer.WriteLine($"        entry._SortId = {sortOrder}");
            writer.WriteLine("    end");
        }
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