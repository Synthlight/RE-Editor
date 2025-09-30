using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using app;
using REFrameworkNET;
using REFrameworkNET.Attributes;
using Sorted_Skills.Assets;

// This forces the game to sort skills by ONLY the name. By default, the game sorts by level, THEN the name.

// ReSharper disable once CheckNamespace
namespace LordGregory.Mods.Sort_Skills;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
public class SortSkills {
    private static          bool                             initialized;
    private static readonly Dictionary<HunterDef.Skill, int> SKILL_SORT_ORDER = [];

    [ThreadStatic]
    private static int newRetValue;

    // sortSkill(app.EquipDef.EquipSkillInfo, app.EquipDef.EquipSkillInfo)
    [MethodHook(typeof(EquipUtil), nameof(EquipUtil.sortSkill), MethodHookType.Pre)]
    public static PreHookResult SortSkillPre(Span<ulong> args) {
        try {
            if (!initialized) {
                API.LogInfo("Making fallback sort order.");
                foreach (var skill in Enum.GetValues<HunterDef.Skill>()) {
                    SKILL_SORT_ORDER[skill] = 0; // Fallback.
                }
                API.LogInfo("Deserializing assets.");
                var fixedSkillNameLookup = JsonSerializer.Deserialize<Dictionary<LangIndex, Dictionary<HunterDef.Skill_Fixed, string>>>(Assets.SKILL_NAME_BY_ENUM_VALUE)!;
                API.LogInfo("Getting language.");
                var language = OptionUtil.getTextLanguage();
                API.LogInfo("Getting skill id fixed to name map.");
                var langMap = fixedSkillNameLookup[(LangIndex) language];

                API.LogInfo("Making a sorted list.");
                var normalIdListInOrder = (from entry in langMap
                                           let fixedId = entry.Key
                                           let text = entry.Value
                                           let fixedName = Enum.GetName(fixedId)
                                           let normalId = Enum.Parse<HunterDef.Skill>(fixedName)
                                           orderby text
                                           select normalId).ToList();

                API.LogInfo("Saving sort order for the skills.");
                for (var i = 0; i < normalIdListInOrder.Count; i++) {
                    SKILL_SORT_ORDER[normalIdListInOrder[i]] = i;
                }
                API.LogInfo("Initialization done.");
                initialized = true;
            }
            var skill1       = ManagedObject.FromAddress(args[1]).As<EquipDef.EquipSkillInfo>();
            var skill1SortId = SKILL_SORT_ORDER[skill1.Skill];
            var skill2       = ManagedObject.FromAddress(args[2]).As<EquipDef.EquipSkillInfo>();
            var skill2SortId = SKILL_SORT_ORDER[skill2.Skill];
            var comparison   = skill1SortId.CompareTo(skill2SortId);
            newRetValue = comparison;
        } catch (Exception e) {
            API.LogError($"Error: {e}");
        }
        return PreHookResult.Continue;
    }

    [MethodHook(typeof(EquipUtil), nameof(EquipUtil.sortSkill), MethodHookType.Post)]
    public static void SortSkillPost(ref ulong ret) {
        ret = unchecked((uint) newRetValue);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum LangIndex {
        jpn,
        eng,
        fre,
        ita,
        ger,
        spa,
        rus,
        pol,
        ptB = 10,
        kor,
        chT,
        chS,
        ara = 21,
    }
}