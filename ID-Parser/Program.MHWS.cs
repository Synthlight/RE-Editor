using System.Text.RegularExpressions;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Models.Enums;
using MSG = RE_Editor.Common.Models.MSG;

namespace RE_Editor.ID_Parser;

public static partial class Program {
    public static void Main() {
        ExtractArmorInfoByGuid();
        ExtractArmorSeriesInfoByGuid();
        ExtractArtianInfoByGuid();
        ExtractDecorationInfoByGuid();
        ExtractEnemyInfoByName();
        ExtractItemInfoByName();
        ExtractItemInfoByGuid();
        ExtractMedalInfoByGuid();
        ExtractPendantInfoByGuid();
        ExtractSkillInfoByName();
        ExtractTalismanInfoByGuid();
        ExtractTitleInfoByGuid();
        ExtractWeaponInfoByGuid();
        ExtractWeaponSeriesInfoByName();
    }

    private static void ExtractArmorInfoByGuid() {
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\Armor.msg.{Global.MSG_VERSION}")
                     .GetLangGuidMap();
        DataHelper.ARMOR_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "ARMOR_INFO_LOOKUP_BY_GUID");

        // Get only the names, no descriptions.
        var regex = new Regex(@"Armor_ID(\d+)");
        msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\Armor.msg.{Global.MSG_VERSION}")
                 .GetLangRawMap<Guid>(name => {
                     // ReSharper disable once ConvertIfStatementToReturnStatement
                     if (!regex.Match(name.first).Success) return new(Guid.Empty, true);
                     return name.id1;
                 });
        CreateConstantsFile(msg[Global.LangIndex.eng].Flip(), "ArmorConstants");
    }

    private static void ExtractArmorSeriesInfoByGuid() {
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\ArmorSeries.msg.{Global.MSG_VERSION}")
                     .GetLangGuidMap();
        DataHelper.ARMOR_SERIES_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "ARMOR_SERIES_INFO_LOOKUP_BY_GUID");

        var regex = new Regex(@"ArmorSeries_(m?\d+)");
        var msgByEnum = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\ArmorSeries.msg.{Global.MSG_VERSION}")
                           .GetLangIdMap<App_ArmorDef_SERIES_Fixed>(name => {
                               var match = regex.Match(name);
                               if (!match.Success) return new(0, true);
                               var value = match.Groups[1].Value.Replace('m', '-');
                               return (App_ArmorDef_SERIES_Fixed) int.Parse(value);
                           });
        var msgByValue = msgByEnum.ConvertTo<App_ArmorDef_SERIES_Fixed, int>();
        DataHelper.ARMOR_SERIES_BY_ENUM_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "ARMOR_SERIES_BY_ENUM_VALUE");
        CreateConstantsFile(msgByEnum[Global.LangIndex.eng].Flip(), "ArmorSeriesConstants");
    }

    private static void ExtractArtianInfoByGuid() {
        var msgBonus = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\ArtianBonus.msg.{Global.MSG_VERSION}")
                          .GetLangGuidMap();
        var msgParts = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\ArtianParts.msg.{Global.MSG_VERSION}")
                          .GetLangGuidMap();
        var msgPerformance = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\ArtianPerformance.msg.{Global.MSG_VERSION}")
                                .GetLangGuidMap();
        var msg = Merge(msgBonus, msgParts, msgPerformance);
        DataHelper.ARTIAN_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "ARTIAN_INFO_LOOKUP_BY_GUID");
    }

    private static void ExtractDecorationInfoByGuid() {
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\Accessory.msg.{Global.MSG_VERSION}")
                     .GetLangGuidMap();
        DataHelper.ITEM_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "DECORATION_INFO_LOOKUP_BY_GUID");
    }

    private static void ExtractEnemyInfoByName() {
        var regex = new Regex("EnemyText_NAME_(EM.+)");
        var msg   = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\EnemyText.msg.{Global.MSG_VERSION}");
        var msgByEnum = msg.GetLangIdMap<App_EnemyDef_ID_Fixed>(name => {
            var match = regex.Match(name);
            if (!match.Success) return new(default, true);
            var value = match.Groups[1].Value;
            return new(Enum.Parse<App_EnemyDef_ID_Fixed>(value));
        });
        var msgByValue = msgByEnum.ConvertTo<App_EnemyDef_ID_Fixed, int>();
        DataHelper.ENEMY_NAME_LOOKUP_BY_ENUM_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "ENEMY_NAME_LOOKUP_BY_ENUM_VALUE");
        CreateConstantsFile(msgByEnum[Global.LangIndex.eng].Flip(), "EnemyConstants");

        Dictionary<App_EnemyDef_ID_Fixed, string> large   = [];
        Dictionary<App_EnemyDef_ID_Fixed, string> small   = [];
        Dictionary<App_EnemyDef_ID_Fixed, string> endemic = [];
        foreach (var (id, name) in msgByEnum[Global.LangIndex.eng]) {
            var idString = id.ToString().ToUpper();

            if (idString.StartsWith("EM0")) large[id]   = name;
            if (idString.StartsWith("EM1")) small[id]   = name;
            if (idString.StartsWith("EM5")) endemic[id] = name;
        }
        CreateConstantsFile(large.Flip(), "EnemyConstantsLarge");
        CreateConstantsFile(small.Flip(), "EnemyConstantsSmall");
        CreateConstantsFile(endemic.Flip(), "EnemyConstantsEndemic");

        Dictionary<App_EnemyDef_ID, string> nonFixedEngMap = [];
        foreach (var (fixedId, name) in large) {
            var enumName = Enum.GetName(fixedId)!;
            var normalId = Enum.Parse<App_EnemyDef_ID>(enumName);
            nonFixedEngMap[normalId] = name;
        }
        CreateLuaConstantsFile(nonFixedEngMap.Flip(), "EnemyConstantsNormalLarge");
    }

    private static void ExtractItemInfoByName() {
        var msgData   = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\Item.msg.{Global.MSG_VERSION}");
        var nameRegex = new Regex(@"Item_IT_(\d+)");
        var msgByEnum = msgData.GetLangIdMap<App_ItemDef_ID_Fixed>(name => {
            var match = nameRegex.Match(name);
            if (!match.Success) return new(0, true);
            var value = match.Groups[1].Value;
            return (App_ItemDef_ID_Fixed) int.Parse(value);
        });
        var msgByValue = msgByEnum.ConvertTo<App_ItemDef_ID_Fixed, uint>();
        DataHelper.ITEM_NAME_LOOKUP = msgByValue;
        CreateAssetFile(msgByValue, "ITEM_NAME_LOOKUP");
        CreateConstantsFile(msgByEnum[Global.LangIndex.eng].Flip(), "ItemConstants");

        Dictionary<App_ItemDef_ID, string> nonFixedEngMap = [];
        foreach (var (fixedId, name) in msgByEnum[Global.LangIndex.eng]) {
            var enumName = Enum.GetName(fixedId)!;
            var normalId = Enum.Parse<App_ItemDef_ID>(enumName);
            nonFixedEngMap[normalId] = name;
        }
        CreateLuaConstantsFile(nonFixedEngMap.Flip(), "ItemConstantsNormal");

        var descRegex = new Regex(@"Item_IT_EXP_(\d+)");
        var msg = msgData.GetLangIdMap<uint>(name => {
            var match = descRegex.Match(name);
            if (!match.Success) return new(0, true);
            var value = match.Groups[1].Value;
            return (uint) int.Parse(value);
        });
        DataHelper.ITEM_DESC_LOOKUP = msg;
        CreateAssetFile(msg, "ITEM_DESC_LOOKUP");
    }

    private static void ExtractItemInfoByGuid() {
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\Item.msg.{Global.MSG_VERSION}")
                     .GetLangGuidMap();
        DataHelper.ITEM_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "ITEM_INFO_LOOKUP_BY_GUID");
    }

    private static void ExtractMedalInfoByGuid() {
        var msg       = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\Medal.msg.{Global.MSG_VERSION}");
        var msgByGuid = msg.GetLangGuidMap();
        DataHelper.MEDAL_INFO_LOOKUP_BY_GUID = msgByGuid;
        CreateAssetFile(msgByGuid, "MEDAL_INFO_LOOKUP_BY_GUID");

        var regex = new Regex(@"Medal_NAME_(\d+)");
        var msgByEnum = msg.GetLangIdMap<App_HunterProfileDef_MEDAL_ID_Fixed>(name => {
            var match = regex.Match(name);
            if (!match.Success) return new(0, true);
            var value = match.Groups[1].Value;
            return (App_HunterProfileDef_MEDAL_ID_Fixed) int.Parse(value);
        });
        var msgByValue = msgByEnum.ConvertTo<App_HunterProfileDef_MEDAL_ID_Fixed, int>();
        DataHelper.MEDAL_NAME_LOOKUP_BY_ENUM_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "MEDAL_NAME_LOOKUP_BY_ENUM_VALUE");
        CreateConstantsFile(msgByEnum[Global.LangIndex.eng].Flip(), "MedalConstants");
    }

    private static void ExtractPendantInfoByGuid() {
        var msg       = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\Charm.msg.{Global.MSG_VERSION}");
        var msgByGuid = msg.GetLangGuidMap();
        DataHelper.PENDANT_INFO_LOOKUP_BY_GUID = msgByGuid;
        CreateAssetFile(msgByGuid, "PENDANT_INFO_LOOKUP_BY_GUID");

        var regex = new Regex(@"Charm_(m?\d+)");
        var msgByEnum = msg.GetLangIdMap<App_WeaponCharmDef_TYPE_Fixed>(name => {
            var match = regex.Match(name);
            if (!match.Success) return new(0, true);
            var value = match.Groups[1].Value.Replace('m', '-');
            return (App_WeaponCharmDef_TYPE_Fixed) int.Parse(value);
        });
        var msgByValue = msgByEnum.ConvertTo<App_WeaponCharmDef_TYPE_Fixed, int>();
        DataHelper.PENDANT_NAME_LOOKUP_BY_ENUM_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "PENDANT_NAME_LOOKUP_BY_ENUM_VALUE");
        CreateConstantsFile(msgByEnum[Global.LangIndex.eng].Flip(), "PendantConstants");
    }

    private static void ExtractSkillInfoByName() {
        var regex = new Regex(@"SkillCommon_(m?\d+)");
        var msgByEnum = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\SkillCommon.msg.{Global.MSG_VERSION}")
                           .GetLangIdMap<App_HunterDef_Skill_Fixed>(name => {
                               var match = regex.Match(name);
                               if (!match.Success) return new(0, true);
                               var value = match.Groups[1].Value.Replace('m', '-');
                               return (App_HunterDef_Skill_Fixed) int.Parse(value);
                           });
        var msgByValue = msgByEnum.ConvertTo<App_HunterDef_Skill_Fixed, int>();
        DataHelper.SKILL_NAME_BY_ENUM_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "SKILL_NAME_BY_ENUM_VALUE");
        CreateConstantsFile(msgByEnum[Global.LangIndex.eng].Flip(), "SkillConstants");
    }

    private static void ExtractTalismanInfoByGuid() {
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\Amulet.msg.{Global.MSG_VERSION}")
                     .GetLangGuidMap();
        DataHelper.ITEM_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "TALISMAN_INFO_LOOKUP_BY_GUID");
    }

    private static void ExtractTitleInfoByGuid() {
        var msgWords = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\Title_Word.msg.{Global.MSG_VERSION}")
                          .GetLangGuidMap();
        var msgConjunctions = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\Title_Conjunction.msg.{Global.MSG_VERSION}")
                                 .GetLangGuidMap();
        var msg = Merge(msgWords, msgConjunctions);
        DataHelper.TITLE_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "TITLE_INFO_LOOKUP_BY_GUID");
    }

    private static void ExtractWeaponInfoByGuid() {
        var allMsgs      = new List<Dictionary<Global.LangIndex, Dictionary<Guid, string>>>();
        var nameOnlyMsgs = new List<Dictionary<Global.LangIndex, Dictionary<Guid, string>>>();
        foreach (var weaponType in Global.WEAPON_TYPES) {
            var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\{weaponType}.msg.{Global.MSG_VERSION}")
                         .GetLangGuidMap();
            allMsgs.Add(msg);

            var regex = new Regex($@"{weaponType}_(\d+)");
            msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\{weaponType}.msg.{Global.MSG_VERSION}")
                     .GetLangRawMap<Guid>(name => {
                         // ReSharper disable once ConvertIfStatementToReturnStatement
                         if (!regex.Match(name.first).Success) return new(Guid.Empty, true);
                         return name.id1;
                     });
            nameOnlyMsgs.Add(msg);
        }
        var mergedMsg = Merge(allMsgs);
        DataHelper.WEAPON_INFO_LOOKUP_BY_GUID = mergedMsg;
        CreateAssetFile(mergedMsg, "WEAPON_INFO_LOOKUP_BY_GUID");
        mergedMsg = Merge(nameOnlyMsgs);
        CreateConstantsFile(mergedMsg[Global.LangIndex.eng].Flip(), "WeaponConstants");
    }

    private static void ExtractWeaponSeriesInfoByName() {
        var regex = new Regex(@"WeaponSeries_(m?\d+)");
        var msgByEnum = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\WeaponSeries.msg.{Global.MSG_VERSION}")
                           .GetLangIdMap<App_WeaponDef_SERIES_Fixed>(name => {
                               var match = regex.Match(name);
                               if (!match.Success) return new(0, true);
                               var value = match.Groups[1].Value.Replace('m', '-');
                               return (App_WeaponDef_SERIES_Fixed) int.Parse(value);
                           });
        var msgByValue = msgByEnum.ConvertTo<App_WeaponDef_SERIES_Fixed, int>();
        DataHelper.WEAPON_SERIES_BY_ENUM_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "WEAPON_SERIES_BY_ENUM_VALUE");
        CreateConstantsFile(msgByEnum[Global.LangIndex.eng].Flip(), "WeaponSeriesConstants");
    }
}