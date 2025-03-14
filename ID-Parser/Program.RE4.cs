using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models.Enums;

namespace RE_Editor.ID_Parser;

public static partial class Program {
    public static void Main() {
        ExtractItemInfo();
        ExtractStatusEffectInfo();
        ExtractWeaponInfo();
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static void ExtractItemInfo() {
        for (var i = 0; i < Global.VARIANTS.Count; i++) {
            var variant = Global.VARIANTS[i];
            var folder  = Global.FOLDERS[i];
            var regex   = new Regex("(?:CH|MC|AO)_Mes_Main_(WEAPON_NAME|ITEM_NAME)_(.+?)_000");
            var msgs = new List<string> {
                           $@"{PathHelper.CHUNK_PATH}\natives\STM\{folder}\Message\Mes_Main_Item\{variant}_Mes_Main_Item_Name.msg.{Global.MSG_VERSION}",
                           $@"{PathHelper.CHUNK_PATH}\natives\STM\{folder}\Message\Mes_Main_Item\{variant}_Mes_Main_Item_Name_Misc.msg.{Global.MSG_VERSION}",
                       }
                       .Where(File.Exists)
                       .Select(file => MSG.Read(file)
                                          .GetLangIdMap<uint>(name => {
                                              var match = regex.Match(name);
                                              if (!match.Success) return new(0, true);
                                              var subType = match.Groups[1].Value;
                                              var value   = match.Groups[2].Value.ToLower();
                                              value = subType switch {
                                                  "ITEM_NAME" => $"it_sm{value}",
                                                  "WEAPON_NAME" => $"it_{value}",
                                                  _ => value
                                              };
                                              try {
                                                  return (uint) (int) Enum.Parse(typeof(Chainsaw_ItemID), value);
                                              } catch (Exception) {
                                                  Debug.WriteLine($"Error reading ${value}.");
                                                  return new(0, true);
                                              }
                                          }))
                       .ToList();
            var msg = msgs.MergeDictionaries();
            CreateAssetFile(msg, $"{variant}_ITEM_NAME_LOOKUP");
            CreateConstantsFile(msg[Global.LangIndex.eng].Flip(), $"ItemConstants_{variant}");
        }
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static void ExtractStatusEffectInfo() {
        var regex = new Regex("(?:CH|MC|AO)_Mes_Main_(StatusEffectID_.*)");
        var msg = new List<string> {$@"{PathHelper.CHUNK_PATH}\natives\STM\_Chainsaw\Message\Mes_Main_Charm\CH_Mes_Main_StatusEffect.msg.{Global.MSG_VERSION}",}
                  .Where(File.Exists)
                  .Select(file => MSG.Read(file)
                                     .GetLangIdMap<uint>(name => {
                                         var match = regex.Match(name);
                                         if (!match.Success) return new(0, true);
                                         var value = match.Groups[1].Value;
                                         try {
                                             return (uint) (int) Enum.Parse(typeof(Chainsaw_StatusEffectID), value);
                                         } catch (Exception) {
                                             Debug.WriteLine($"Error reading ${value}.");
                                             return new(0, true);
                                         }
                                     }))
                  .ToList()
                  .MergeDictionaries();

        CreateAssetFile(msg, "STATUS_EFFECT_NAME_LOOKUP");

        var dict = new Dictionary<Chainsaw_StatusEffectID, string>();
        foreach (var (id, name) in msg[Global.LangIndex.eng]) {
            dict[(Chainsaw_StatusEffectID) id] = name;
        }

        CreateConstantsFile(dict.Flip(), "StatusEffectConstants");
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static void ExtractWeaponInfo() {
        for (var i = 0; i < Global.VARIANTS.Count; i++) {
            var variant = Global.VARIANTS[i];
            var folder  = Global.FOLDERS[i];
            var regex   = new Regex("(?:CH|MC|AO)_Mes_Main_WEAPON_NAME_([a-zA-Z0-9]+?)_");
            var msg = new List<string> {
                          $@"{PathHelper.CHUNK_PATH}\natives\STM\{folder}\Message\Mes_Main_Item\{variant}_Mes_Main_Item_Name.msg.{Global.MSG_VERSION}",
                          $@"{PathHelper.CHUNK_PATH}\natives\STM\{folder}\Message\Mes_Main_Item\{variant}_Mes_Main_Item_Name_Misc.msg.{Global.MSG_VERSION}",
                      }
                      .Where(File.Exists)
                      .Select(file => MSG.Read(file)
                                         .GetLangIdMap<uint>(name => {
                                             var match = regex.Match(name);
                                             if (!match.Success) return new(0, true);
                                             var value = match.Groups[1].Value.ToLower();
                                             return (uint) (int) Enum.Parse(typeof(Chainsaw_WeaponID), value);
                                         }))
                      .ToList()
                      .MergeDictionaries();

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var lang in Global.LANGUAGES) {
                // I don't know why this one is nowhere in the name data, but it's the real ID of Punisher MC in AO.
                if (variant == "AO") {
                    msg[lang].TryAdd(6112, "Punisher MC (AO)");
                    msg[lang].TryAdd(6114, "SR M1903 MC (AO)");
                }
            }

            CreateAssetFile(msg, $"{variant}_WEAPON_NAME_LOOKUP");
            CreateConstantsFile(msg[Global.LangIndex.eng].Flip(), $"WeaponConstants_{variant}");
        }
    }
}