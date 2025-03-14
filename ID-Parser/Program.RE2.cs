using RE_Editor.Common;
using System.Text.RegularExpressions;
using RE_Editor.Common.Models;
using RE_Editor.Models.Enums;
using System.Diagnostics;

namespace RE_Editor.ID_Parser;

public static partial class Program {
    public static void Main() {
        ExtractItemInfo();
        ExtractWeaponInfo();
    }

    private static void ExtractItemInfo() {
        var regex = new Regex(@"ITEM_NAME_(\d{2}_\d{3})");
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\SectionRoot\Message\Mes_Item\Mes_Item_Name.msg.{Global.MSG_VERSION}")
                     .GetLangIdMap<uint>(name => {
                         var match = regex.Match(name);
                         if (!match.Success) return new(0, true);
                         var value = match.Groups[1].Value;
                         return (uint) (int) Enum.Parse(typeof(App_ropeway_gamemastering_Item_ID), $"sm{value}");
                     });
        CreateAssetFile(msg, "ITEM_NAME_LOOKUP");
        CreateConstantsFile(msg[Global.LangIndex.eng].Flip(), "ItemConstants");
    }

    private static void ExtractWeaponInfo() {
        var regex = new Regex(@"WEAPON_NAME_WP(\d{4})");
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\SectionRoot\Message\Mes_Item\Mes_Item_Name.msg.{Global.MSG_VERSION}")
                     .GetLangIdMap<uint>(name => {
                         var match = regex.Match(name);
                         if (!match.Success) return new(0, true);
                         var value = match.Groups[1].Value;
                         try {
                             return (uint) (int) Enum.Parse(typeof(App_ropeway_EquipmentDefine_WeaponType), $"WP{value}");
                         } catch (Exception) {
                             Debug.WriteLine($"Error reading ${value}.");
                             return new(0, true);
                         }
                     });
        CreateAssetFile(msg, "WEAPON_NAME_LOOKUP");

        var dict = new Dictionary<App_ropeway_EquipmentDefine_WeaponType, string>();
        foreach (var (id, name) in msg[Global.LangIndex.eng]) {
            dict[(App_ropeway_EquipmentDefine_WeaponType) id] = name;
        }
        CreateConstantsFile(dict.Flip(), "WeaponConstants");
    }
}