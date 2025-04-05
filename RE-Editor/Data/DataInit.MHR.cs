using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Data.MHR;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;

namespace RE_Editor.Data;

public static partial class DataInit {
    // ReSharper disable once IdentifierTypo
    private static void LoadDicts() {
        DataHelper.ARMOR_NAME_LOOKUP              = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.ARMOR_NAME_LOOKUP);
        DataHelper.ARMOR_DESC_LOOKUP              = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.ARMOR_DESC_LOOKUP);
        DataHelper.ARMOR_SERIES_LOOKUP            = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.ARMOR_SERIES_LOOKUP);
        DataHelper.CAT_DOG_ARMOR_NAME_LOOKUP      = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.CAT_DOG_ARMOR_NAME_LOOKUP);
        DataHelper.CAT_DOG_ARMOR_DESC_LOOKUP      = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.CAT_DOG_ARMOR_DESC_LOOKUP);
        DataHelper.CAT_DOG_WEAPON_NAME_LOOKUP     = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.CAT_DOG_WEAPON_NAME_LOOKUP);
        DataHelper.CAT_DOG_WEAPON_DESC_LOOKUP     = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.CAT_DOG_WEAPON_DESC_LOOKUP);
        DataHelper.DANGO_NAME_LOOKUP              = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.DANGO_NAME_LOOKUP);
        DataHelper.DANGO_DESC_LOOKUP              = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.DANGO_DESC_LOOKUP);
        DataHelper.DANGO_SKILL_NAME_LOOKUP        = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.DANGO_SKILL_NAME_LOOKUP);
        DataHelper.DECORATION_NAME_LOOKUP         = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.DECORATION_NAME_LOOKUP);
        DataHelper.DECORATION_DESC_LOOKUP         = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.DECORATION_DESC_LOOKUP);
        DataHelper.GC_TITLE_NAME_LOOKUP           = DataHelper.LoadDict<Global.LangIndex, Dictionary<string, string>>(Assets.GC_TITLE_NAME_LOOKUP);
        DataHelper.GC_TITLE_DESC_LOOKUP           = DataHelper.LoadDict<Global.LangIndex, Dictionary<string, string>>(Assets.GC_TITLE_DESC_LOOKUP);
        DataHelper.ITEM_NAME_LOOKUP               = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.ITEM_NAME_LOOKUP);
        DataHelper.ITEM_DESC_LOOKUP               = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.ITEM_DESC_LOOKUP);
        DataHelper.PETALACE_NAME_LOOKUP           = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.PETALACE_NAME_LOOKUP);
        DataHelper.PETALACE_DESC_LOOKUP           = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.PETALACE_DESC_LOOKUP);
        DataHelper.RAMPAGE_DECORATION_NAME_LOOKUP = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.RAMPAGE_DECORATION_NAME_LOOKUP);
        DataHelper.RAMPAGE_DECORATION_DESC_LOOKUP = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.RAMPAGE_DECORATION_DESC_LOOKUP);
        DataHelper.RAMPAGE_SKILL_NAME_LOOKUP      = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.RAMPAGE_SKILL_NAME_LOOKUP);
        DataHelper.SKILL_NAME_LOOKUP              = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.SKILL_NAME_LOOKUP);
        DataHelper.SWITCH_SKILL_NAME_LOOKUP       = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.SWITCH_SKILL_NAME_LOOKUP);
        DataHelper.SWITCH_SKILL_DESC_LOOKUP       = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.SWITCH_SKILL_DESC_LOOKUP);
        DataHelper.WEAPON_NAME_LOOKUP             = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.WEAPON_NAME_LOOKUP);
        DataHelper.WEAPON_DESC_LOOKUP             = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.WEAPON_DESC_LOOKUP);
    }

    private static void CreateTranslationsForSkillEnumNameColumns() {
        CreateEnumTranslation<Snow_data_DataDef_PlEquipSkillId>(@"^EquipSkill_(\d+)" /* language=regex */,
                                                                Assets.SKILL_ENUM_NAME_LOOKUP,
                                                                typeof(Snow_player_EquipSkillParameter),
                                                                enumName => $"Pl_{enumName}");

        CreateEnumTranslation<Snow_data_DataDef_PlKitchenSkillId>(@"^KitchenSkill_(\d+)" /* language=regex */,
                                                                  Assets.DANGO_SKILL_ENUM_NAME_LOOKUP,
                                                                  typeof(Snow_player_OdangoSkillParameter),
                                                                  enumName => $"Pl_{enumName}");

        CreateEnumTranslation<Snow_data_DataDef_PlHyakuryuSkillId>(@"^HyakuryuSkill_(\d+)" /* language=regex */,
                                                                   Assets.RAMPAGE_SKILL_ENUM_NAME_LOOKUP,
                                                                   typeof(Snow_player_HyakuryuSkillParameter),
                                                                   enumName => enumName,
                                                                   (lang, newName) => lang == Global.LangIndex.eng ? newName.Replace("Hyakuryu", "Rampage") : newName); // Because the translation applies before this does.
    }

    private static void CreateEnumTranslation<T>(string pattern, byte[] asset, Type type, Func<string, string> enumParseName, Func<Global.LangIndex, string, string> translatePropName = null) where T : struct {
        var skillEnumLookup = DataHelper.LoadDict<Global.LangIndex, Dictionary<T, string>>(asset);
        var regex           = new Regex(pattern);
        foreach (var propertyInfo in type.GetProperties()) {
            var propName = propertyInfo.Name;
            var match    = regex.Match(propName);
            if (!match.Success) continue;
            var enumName  = match.Groups[0].Value;
            var enumValue = Enum.Parse<T>(enumParseName(enumName));
            foreach (var lang in Enum.GetValues<Global.LangIndex>()) {
                var skillMap = skillEnumLookup[lang];
                if (!skillMap.ContainsKey(enumValue) || skillMap[enumValue] == "#Rejected#") continue;
                var skillName = skillMap[enumValue];
                var newName   = propName.Replace(enumName, skillName);
                Global.TRANSLATION_MAP[lang][translatePropName == null ? propName : translatePropName(lang, propName)] = newName;
            }
        }
    }
}