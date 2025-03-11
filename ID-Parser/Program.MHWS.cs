using System.Text.RegularExpressions;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Models.Enums;
using MSG = RE_Editor.Common.Models.MSG;

namespace RE_Editor.ID_Parser;

public static partial class Program {
    public static void Main() {
        ExtractTitleInfoByGuid();
        ExtractDecorationInfoByGuid();
        ExtractTalismanInfoByGuid();
        ExtractItemInfoByName();
        ExtractItemInfoByGuid();
        ExtractArmorInfoByGuid();
        ExtractArmorSeriesInfoByGuid();
        ExtractWeaponInfoByGuid();
        ExtractSkillInfoByName();
    }

    private static void ExtractTitleInfoByGuid() {
        var msgWords = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\Title_Word.msg.{Global.MSG_VERSION}")
                          .GetLangRawMap(name => {
                              try {
                                  return name.id1;
                              } catch (Exception) {
                                  throw new MSG.SkipReadException();
                              }
                          });
        var msgConjunctions = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\Title_Conjunction.msg.{Global.MSG_VERSION}")
                                 .GetLangRawMap(name => {
                                     try {
                                         return name.id1;
                                     } catch (Exception) {
                                         throw new MSG.SkipReadException();
                                     }
                                 });
        var msg = Merge(msgWords, msgConjunctions);
        CreateAssetFile(msg, "TITLE_INFO_LOOKUP_BY_GUID");
    }

    private static void ExtractDecorationInfoByGuid() {
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\Accessory.msg.{Global.MSG_VERSION}")
                     .GetLangRawMap(name => {
                         try {
                             return name.id1;
                         } catch (Exception) {
                             throw new MSG.SkipReadException();
                         }
                     });
        DataHelper.ITEM_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "DECORATION_INFO_LOOKUP_BY_GUID");
    }

    private static void ExtractTalismanInfoByGuid() {
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\Amulet.msg.{Global.MSG_VERSION}")
                     .GetLangRawMap(name => {
                         try {
                             return name.id1;
                         } catch (Exception) {
                             throw new MSG.SkipReadException();
                         }
                     });
        DataHelper.ITEM_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "TALISMAN_INFO_LOOKUP_BY_GUID");
    }

    private static void ExtractItemInfoByName() {
        var regex = new Regex(@"Item_IT_(\d+)");
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\Item.msg.{Global.MSG_VERSION}")
                     .GetLangIdMap(name => {
                         var match = regex.Match(name);
                         var value = match.Groups[1].Value;
                         try {
                             return (uint) int.Parse(value);
                         } catch (Exception) {
                             throw new MSG.SkipReadException();
                         }
                     });
        DataHelper.ITEM_NAME_LOOKUP = msg;
        CreateAssetFile(msg, "ITEM_NAME_LOOKUP");
        CreateConstantsFile(msg[Global.LangIndex.eng].Flip(), "ItemConstants");

        regex = new(@"Item_IT_EXP_(\d+)");
        msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\Item.msg.{Global.MSG_VERSION}")
                 .GetLangIdMap(name => {
                     var match = regex.Match(name);
                     var value = match.Groups[1].Value;
                     try {
                         return (uint) int.Parse(value);
                     } catch (Exception) {
                         throw new MSG.SkipReadException();
                     }
                 });
        DataHelper.ITEM_DESC_LOOKUP = msg;
        CreateAssetFile(msg, "ITEM_DESC_LOOKUP");
    }

    private static void ExtractItemInfoByGuid() {
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Data\Item.msg.{Global.MSG_VERSION}")
                     .GetLangRawMap(name => {
                         try {
                             return name.id1;
                         } catch (Exception) {
                             throw new MSG.SkipReadException();
                         }
                     });
        DataHelper.ITEM_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "ITEM_INFO_LOOKUP_BY_GUID");
    }

    private static void ExtractArmorInfoByGuid() {
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\Armor.msg.{Global.MSG_VERSION}")
                     .GetLangRawMap(name => {
                         try {
                             return name.id1;
                         } catch (Exception) {
                             throw new MSG.SkipReadException();
                         }
                     });
        DataHelper.ARMOR_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "ARMOR_INFO_LOOKUP_BY_GUID");

        // Get only the names, no descriptions.
        var regex = new Regex(@"Armor_ID(\d+)");
        msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\Armor.msg.{Global.MSG_VERSION}")
                 .GetLangRawMap(name => {
                     if (!regex.Match(name.first).Success) {
                         throw new MSG.SkipReadException();
                     }
                     try {
                         return name.id1;
                     } catch (Exception) {
                         throw new MSG.SkipReadException();
                     }
                 });
        CreateConstantsFile(msg[Global.LangIndex.eng].Flip(), "ArmorConstants");
    }

    private static void ExtractArmorSeriesInfoByGuid() {
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\ArmorSeries.msg.{Global.MSG_VERSION}")
                     .GetLangRawMap(name => {
                         try {
                             return name.id1;
                         } catch (Exception) {
                             throw new MSG.SkipReadException();
                         }
                     });
        DataHelper.ARMOR_SERIES_INFO_LOOKUP_BY_GUID = msg;
        CreateAssetFile(msg, "ARMOR_SERIES_INFO_LOOKUP_BY_GUID");

        var regex = new Regex(@"ArmorSeries_(m?\d+)");
        var msgByEnum = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\ArmorSeries.msg.{Global.MSG_VERSION}")
                           .GetLangIdMap(name => {
                               var match = regex.Match(name);
                               var value = match.Groups[1].Value.Replace('m', '-');
                               try {
                                   return int.Parse(value);
                               } catch (Exception) {
                                   throw new MSG.SkipReadException();
                               }
                           });
        DataHelper.ARMOR_SERIES_BY_ENUM_VALUE = msgByEnum;
        CreateAssetFile(msgByEnum, "ARMOR_SERIES_BY_ENUM_VALUE");
    }

    private static void ExtractWeaponInfoByGuid() {
        var allMsgs      = new List<Dictionary<Global.LangIndex, Dictionary<Guid, string>>>();
        var nameOnlyMsgs = new List<Dictionary<Global.LangIndex, Dictionary<Guid, string>>>();
        foreach (var weaponType in Global.WEAPON_TYPES) {
            var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\{weaponType}.msg.{Global.MSG_VERSION}")
                         .GetLangRawMap(name => {
                             try {
                                 return name.id1;
                             } catch (Exception) {
                                 throw new MSG.SkipReadException();
                             }
                         });
            allMsgs.Add(msg);

            var regex = new Regex($@"{weaponType}_(\d+)");
            msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\{weaponType}.msg.{Global.MSG_VERSION}")
                     .GetLangRawMap(name => {
                         if (!regex.Match(name.first).Success) {
                             throw new MSG.SkipReadException();
                         }
                         try {
                             return name.id1;
                         } catch (Exception) {
                             throw new MSG.SkipReadException();
                         }
                     });
            nameOnlyMsgs.Add(msg);
        }
        var mergedMsg = allMsgs.MergeDictionaries();
        CreateAssetFile(mergedMsg, "WEAPON_INFO_LOOKUP_BY_GUID");
        mergedMsg = nameOnlyMsgs.MergeDictionaries();
        CreateConstantsFile(mergedMsg[Global.LangIndex.eng].Flip(), "WeaponConstants");
    }

    private static void ExtractSkillInfoByName() {
        var regex = new Regex(@"SkillCommon_(m?\d+)");
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Text\Excel_Equip\SkillCommon.msg.{Global.MSG_VERSION}")
                     .GetLangIdMap(name => {
                         var match = regex.Match(name);
                         var value = match.Groups[1].Value.Replace('m', '-');
                         try {
                             return (App_HunterDef_Skill_Fixed) int.Parse(value);
                         } catch (Exception) {
                             throw new MSG.SkipReadException();
                         }
                     });
        CreateAssetFile(msg, "SKILL_NAME_BY_ENUM_NAME");
        CreateConstantsFile(msg[Global.LangIndex.eng].Flip(), "SkillConstants");

        var byInt = new Dictionary<Global.LangIndex, Dictionary<int, string>>();
        foreach (var lang in Global.LANGUAGES) {
            foreach (var (id, name) in msg[lang]) {
                if (!byInt.ContainsKey(lang)) byInt[lang] = new();
                byInt[lang][(int) id] = name;
            }
        }
        DataHelper.SKILL_NAME_BY_ENUM_VALUE = byInt;
        CreateAssetFile(byInt, "SKILL_NAME_BY_ENUM_VALUE");
    }
}