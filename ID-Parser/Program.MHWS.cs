using System.Text.RegularExpressions;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Models.Enums;
using MSG = RE_Editor.Common.Models.MSG;

namespace RE_Editor.ID_Parser;

public static partial class Program {
    public static void Go() {
        ExtractArmorInfoByGuid();
        ExtractArmorSeriesInfoByGuid();
        ExtractArmorLayeredInfoByGuid();
        ExtractArtianInfoByGuid();
        ExtractDecorationInfo();
        ExtractDlcInfoByGuid();
        ExtractDlcInfoByName();
        ExtractEnemyInfoByName();
        ExtractGimmickInfoByName();
        ExtractGimmickInfoByGuid();
        ExtractItemInfoByName();
        ExtractItemInfoByGuid();
        ExtractMedalInfoByGuid();
        ExtractMenuInfo();
        ExtractNpcInfoByName();
        ExtractPendantInfoByGuid();
        ExtractOtomoInfoByGuid();
        ExtractOtomoSeriesInfoByGuid();
        ExtractOtomoLayeredInfoByGuid();
        ExtractQuestInfo();
        ExtractSkillInfoByName();
        ExtractTalismanInfoByGuid();
        ExtractTitleInfoByGuid();
        ExtractWeaponInfoByGuid();
        ExtractWeaponSeriesInfoByName();
        ExtractWeaponLayeredInfoByGuid();
    }

    private static void ExtractArmorInfoByGuid() {
        var msg       = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\Armor.msg.{Global.MSG_VERSION}");
        var msgByGuid = msg.GetLangGuidMap();
        DataHelper.ARMOR_INFO_LOOKUP_BY_GUID = msgByGuid;
        CreateAssetFile(msgByGuid, "ARMOR_INFO_LOOKUP_BY_GUID");

        // Get only the names, no descriptions.
        var regex = new Regex(@"Armor_ID(\d+)");
        var msgNamesByGuid = msg.GetLangRawMap<Guid>(name => {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (!regex.Match(name.first).Success) return new(Guid.Empty, true);
            return name.id1;
        });
        CreateConstantsFile(msgNamesByGuid[Global.LangIndex.eng].Flip(), "ArmorConstants");
    }

    private static void ExtractArmorSeriesInfoByGuid() {
        var msg       = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\ArmorSeries.msg.{Global.MSG_VERSION}");
        var msgByGuid = msg.GetLangGuidMap();
        DataHelper.ARMOR_SERIES_INFO_LOOKUP_BY_GUID = msgByGuid;
        CreateAssetFile(msgByGuid, "ARMOR_SERIES_INFO_LOOKUP_BY_GUID");

        var regex = new Regex(@"ArmorSeries_(m?\d+)");
        var msgByEnum = msg.GetLangIdMap<App_ArmorDef_SERIES_Fixed>(name => {
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

    private static void ExtractArmorLayeredInfoByGuid() {
        var msg       = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\OuterArmor.msg.{Global.MSG_VERSION}");
        var msgByGuid = msg.GetLangGuidMap();
        DataHelper.ARMOR_LAYERED_INFO_LOOKUP_BY_GUID = msgByGuid;
        CreateAssetFile(msgByGuid, "ARMOR_LAYERED_INFO_LOOKUP_BY_GUID");

        var regex = new Regex(@"OuterArmor_MALE(m?\d+)");
        var msgByValue = msg.GetLangIdMap<int>(name => {
            var match = regex.Match(name);
            if (!match.Success) return new(0, true);
            var value = match.Groups[1].Value.Replace('m', '-');
            return int.Parse(value);
        });
        DataHelper.ARMOR_LAYERED_NAME_LOOKUP_BY_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "ARMOR_LAYERED_NAME_LOOKUP_BY_VALUE");
        CreateConstantsFile(msgByValue[Global.LangIndex.eng].Flip(), "ArmorLayeredConstants");
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

    private static void ExtractDecorationInfo() {
        var msg       = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\Accessory.msg.{Global.MSG_VERSION}");
        var msgByGuid = msg.GetLangGuidMap();
        DataHelper.ITEM_INFO_LOOKUP_BY_GUID = msgByGuid;
        CreateAssetFile(msgByGuid, "DECORATION_INFO_LOOKUP_BY_GUID");

        var regex = new Regex(@"Accessory_ACC_(m?\d+)");
        var msgByEnum = msg.GetLangIdMap<App_EquipDef_ACCESSORY_ID_Fixed>(name => {
            var match = regex.Match(name);
            if (!match.Success) return new(0, true);
            var value = match.Groups[1].Value.Replace('m', '-');
            return (App_EquipDef_ACCESSORY_ID_Fixed) int.Parse(value);
        });
        var msgByValue = msgByEnum.ConvertTo<App_EquipDef_ACCESSORY_ID_Fixed, int>();
        DataHelper.DECORATION_INFO_LOOKUP_BY_ENUM_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "DECORATION_INFO_LOOKUP_BY_ENUM_VALUE");
        CreateConstantsFile(msgByEnum[Global.LangIndex.eng].Flip(), "DecorationConstants");
    }

    private static void ExtractDlcInfoByGuid() {
        var msg   = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\DlcProduct.msg.{Global.MSG_VERSION}")
                           .GetLangGuidMap();
        DataHelper.DLC_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "DLC_INFO_LOOKUP_BY_GUID");
    }

    private static void ExtractDlcInfoByName() {
        var msgData   = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\DlcProduct.msg.{Global.MSG_VERSION}");
        var nameRegex = new Regex(@"DlcProduct_NAME_(m?\d+)");
        var msgByEnum = msgData.GetLangIdMap<App_dlc_DlcProductId_ID_Fixed>(name => {
            var match = nameRegex.Match(name);
            if (!match.Success) return new(0, true);
            var value = match.Groups[1].Value.Replace('m', '-');
            return (App_dlc_DlcProductId_ID_Fixed) int.Parse(value);
        });
        var msgByValue = msgByEnum.ConvertTo<App_dlc_DlcProductId_ID_Fixed, int>();
        DataHelper.DLC_NAME_LOOKUP_BY_ENUM_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "DLC_NAME_LOOKUP_BY_ENUM_VALUE");
        CreateConstantsFile(msgByEnum[Global.LangIndex.eng].Flip(), "DlcConstants");
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

        var msgBitset = msgByEnum.ConvertToByEnumName<App_EnemyDef_ID_Fixed, App_EnemyDef_ID_BIT>();
        CreateAssetFile(msgBitset, "TRANSLATION_App_EnemyDef_ID_BIT");
    }

    private static void ExtractGimmickInfoByName() {
        var msgData   = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\Gimmick.msg.{Global.MSG_VERSION}");
        var nameRegex = new Regex(@"Gimmick_NAME_(m?\d+)");
        var msgByEnum = msgData.GetLangIdMap<App_GimmickDef_ID_Fixed>(name => {
            var match = nameRegex.Match(name);
            if (!match.Success) return new(0, true);
            var value = match.Groups[1].Value.Replace('m', '-');
            return (App_GimmickDef_ID_Fixed) int.Parse(value);
        });
        var msgByValue = msgByEnum.ConvertTo<App_GimmickDef_ID_Fixed, int>();
        DataHelper.GIMMICK_NAME_LOOKUP_BY_ENUM_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "GIMMICK_NAME_LOOKUP_BY_ENUM_VALUE");
        CreateConstantsFile(msgByEnum[Global.LangIndex.eng].Flip(), "GimmickConstants");
    }

    private static void ExtractGimmickInfoByGuid() {
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\Gimmick.msg.{Global.MSG_VERSION}")
                     .GetLangGuidMap();
        DataHelper.GIMMICK_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "GIMMICK_INFO_LOOKUP_BY_GUID");
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

    private static void ExtractMenuInfo() {
        // Do first so we have GUID data to reference.
        {
            var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Reference\RefMenu.msg.{Global.MSG_VERSION}")
                         .GetLangGuidMap();
            DataHelper.MENU_INFO_LOOKUP_BY_GUID = msg;
            CreateAssetFile(msg, "MENU_INFO_LOOKUP_BY_GUID");
        }

        // TODO: This requires generated data to be loaded so it can't be done here.
        // The numbers don't line up to the enum, so we have to hack it and associate this from the file itself.
        /*
        {
            var menuDataFile = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}\{PathHelper.MENU_SETTING_DATA_PATH}");
            var menuData     = menuDataFile.rsz.GetEntryObject<App_user_data_MenuData>().Values;

            Dictionary<Global.LangIndex, Dictionary<App_Menu_ID_Fixed, string>> msgByEnum = [];
            foreach (var data in menuData) {
                foreach (var lang in Global.LANGUAGES) {
                    var name = DataHelper.MENU_INFO_LOOKUP_BY_GUID[lang][data.Title];
                    msgByEnum[lang]              ??= [];
                    msgByEnum[lang][data.MenuId] =   name;
                }
            }

            var msgByValue = msgByEnum.ConvertTo<App_Menu_ID_Fixed, int>();
            DataHelper.MENU_NAME_LOOKUP_BY_ENUM_VALUE = msgByValue;
            CreateAssetFile(msgByValue, "MENU_NAME_LOOKUP");
            CreateConstantsFile(msgByEnum[Global.LangIndex.eng].Flip(), "MenuConstants");
        }
        */
    }

    private static void ExtractNpcInfoByName() {
        var msgData   = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\NpcName.msg.{Global.MSG_VERSION}");
        var nameRegex = new Regex(@"NpcName_NN_(m?\d+)");
        var msgByEnum = msgData.GetLangIdMap<App_NpcDef_ID_Fixed>(name => {
            var match = nameRegex.Match(name);
            if (!match.Success) return new(0, true);
            var value = match.Groups[1].Value.Replace('m', '-');
            return (App_NpcDef_ID_Fixed) int.Parse(value);
        });
        var msgByValue = msgByEnum.ConvertTo<App_NpcDef_ID_Fixed, int>();
        DataHelper.NPC_NAME_LOOKUP_BY_ENUM_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "NPC_NAME_LOOKUP_BY_ENUM_VALUE");
        CreateConstantsFile(msgByEnum[Global.LangIndex.eng].Flip(), "NpcConstants");
    }

    private static void ExtractOtomoInfoByGuid() {
        var msg       = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\OtomoArmor.msg.{Global.MSG_VERSION}");
        var msgByGuid = msg.GetLangGuidMap();
        DataHelper.OTOMO_INFO_LOOKUP_BY_GUID = msgByGuid;
        CreateAssetFile(msgByGuid, "OTOMO_INFO_LOOKUP_BY_GUID");

        // The armor file uses an int field for data ID. The enum does not go high enough.
        var regex = new Regex(@"OtomoArmor_(m?\d+)");
        var msgByValue = msg.GetLangIdMap<int>(name => {
            var match = regex.Match(name);
            if (!match.Success) return new(0, true);
            var value = match.Groups[1].Value.Replace('m', '-');
            return int.Parse(value);
        });
        DataHelper.OTOMO_NAME_LOOKUP_BY_ENUM_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "OTOMO_NAME_LOOKUP_BY_ENUM_VALUE");
        CreateConstantsFile(msgByValue[Global.LangIndex.eng].Flip(), "OtomoArmorConstants");
    }

    private static void ExtractOtomoSeriesInfoByGuid() {
        var msg       = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\OtomoEquipSeries.msg.{Global.MSG_VERSION}");
        var msgByGuid = msg.GetLangGuidMap();
        DataHelper.OTOMO_SERIES_INFO_LOOKUP_BY_GUID = msgByGuid;
        CreateAssetFile(msgByGuid, "OTOMO_SERIES_INFO_LOOKUP_BY_GUID");

        // The armor file uses an int field for data ID. The enum does not go high enough.
        var regex = new Regex(@"OtomoEquipSeries_(m?\d+)");
        var msgByEnum = msg.GetLangIdMap<App_OtEquipDef_EQUIP_DATA_ID_Fixed>(name => {
            var match = regex.Match(name);
            if (!match.Success) return new(0, true);
            var value = match.Groups[1].Value.Replace('m', '-');
            return (App_OtEquipDef_EQUIP_DATA_ID_Fixed) int.Parse(value);
        });
        var msgByValue = msgByEnum.ConvertTo<App_OtEquipDef_EQUIP_DATA_ID_Fixed, int>();
        DataHelper.OTOMO_SERIES_BY_ENUM_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "OTOMO_SERIES_BY_ENUM_VALUE");
        CreateConstantsFile(msgByValue[Global.LangIndex.eng].Flip(), "OtomoArmorSeriesConstants");
    }

    private static void ExtractOtomoLayeredInfoByGuid() {
        var msg       = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\OtomoOuterArmor.msg.{Global.MSG_VERSION}");
        var msgByGuid = msg.GetLangGuidMap();
        DataHelper.OTOMO_LAYERED_INFO_LOOKUP_BY_GUID = msgByGuid;
        CreateAssetFile(msgByGuid, "OTOMO_LAYERED_INFO_LOOKUP_BY_GUID");

        // The armor file uses an int field for data ID. The enum does not go high enough.
        var regex = new Regex(@"OtomoOuterArmor_(m?\d+)");
        var msgByValue = msg.GetLangIdMap<int>(name => {
            var match = regex.Match(name);
            if (!match.Success) return new(0, true);
            var value = match.Groups[1].Value.Replace('m', '-');
            return int.Parse(value);
        });
        DataHelper.OTOMO_LAYERED_NAME_LOOKUP_BY_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "OTOMO_LAYERED_NAME_LOOKUP_BY_VALUE");
        CreateConstantsFile(msgByValue[Global.LangIndex.eng].Flip(), "OtomoArmorLayeredConstants");
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

    private static void ExtractQuestInfo() {
        var langMaps  = new List<Dictionary<Global.LangIndex, Dictionary<App_MissionIDList_ID_Fixed, string>>>();
        var fileRegex = new Regex(@"Mission\d+\.msg");
        var nameRegex = new Regex(@"Mission(\d+)_000");
        foreach (var file in Directory.EnumerateFiles($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Mission", $"*.msg.{Global.MSG_VERSION}")) {
            if (!fileRegex.IsMatch(file)) continue;

            var msg = MSG.Read(file);
            var values = msg.GetLangIdMap<App_MissionIDList_ID_Fixed>(name => {
                var match = nameRegex.Match(name);
                if (!match.Success) return new(0, true);
                var value = $"MISSION_{match.Groups[1].Value}";
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (Enum.TryParse<App_MissionIDList_ID_Fixed>(value, out var result)) {
                    return result;
                }
                return new(0, true);
            });
            langMaps.Add(values);
        }
        var msgByEnum  = langMaps.MergeDictionaries();
        var msgByValue = msgByEnum.ConvertTo<App_MissionIDList_ID_Fixed, int>();
        DataHelper.QUEST_INFO_LOOKUP_BY_ENUM_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "QUEST_INFO_LOOKUP_BY_ENUM_VALUE");
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
        var msg       = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\Amulet.msg.{Global.MSG_VERSION}");
        var msgByGuid = msg.GetLangGuidMap();
        DataHelper.ITEM_INFO_LOOKUP_BY_GUID = msgByGuid;
        CreateAssetFile(msgByGuid, "TALISMAN_INFO_LOOKUP_BY_GUID");

        // Get only the names, no descriptions.
        var regex = new Regex(@"Amulet_(\d+)");
        msgByGuid = msg.GetLangRawMap<Guid>(name => {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (!regex.Match(name.first).Success) return new(Guid.Empty, true);
            return name.id1;
        });
        CreateConstantsFile(msgByGuid[Global.LangIndex.eng].Flip(), "TalismanConstants");
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
            var msg       = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\{weaponType}.msg.{Global.MSG_VERSION}");
            var msgByGuid = msg.GetLangGuidMap();
            allMsgs.Add(msgByGuid);

            var regex = new Regex($@"{weaponType}_(\d+)");
            var msgNameByGuid = msg.GetLangRawMap<Guid>(name => {
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (!regex.Match(name.first).Success) return new(Guid.Empty, true);
                return name.id1;
            });
            nameOnlyMsgs.Add(msgNameByGuid);
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

    private static void ExtractWeaponLayeredInfoByGuid() {
        var allMsgs      = new List<Dictionary<Global.LangIndex, Dictionary<Guid, string>>>();
        var nameOnlyMsgs = new List<Dictionary<Global.LangIndex, Dictionary<Guid, string>>>();
        foreach (var weaponType in Global.WEAPON_TYPES) {
            var msg       = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\Outer{weaponType}.msg.{Global.MSG_VERSION}");
            var msgByGuid = msg.GetLangGuidMap();
            allMsgs.Add(msgByGuid);

            var regex = new Regex($@"{weaponType}_(\d+)");
            var msgNameByGuid = msg.GetLangRawMap<Guid>(name => {
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (!regex.Match(name.first).Success) return new(Guid.Empty, true);
                return name.id1;
            });
            nameOnlyMsgs.Add(msgNameByGuid);
        }
        var mergedMsg = Merge(allMsgs);
        DataHelper.WEAPON_LAYERED_INFO_LOOKUP_BY_GUID = mergedMsg;
        CreateAssetFile(mergedMsg, "WEAPON_LAYERED_INFO_LOOKUP_BY_GUID");
        mergedMsg = Merge(nameOnlyMsgs);
        CreateConstantsFile(mergedMsg[Global.LangIndex.eng].Flip(), "WeaponLayeredConstants");
    }
}