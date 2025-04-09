using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;

namespace RE_Editor.ID_Parser;

public static partial class Program {
    public static void Go() {
        ExtractItemInfo();
        ExtractShopNames();

        Assembly.Load(nameof(Common));
        Assembly.Load(nameof(Generated));
        DataHelper.InitStructTypeInfo();
        var json = File.ReadAllText($@"{ASSETS_DIR}\STRUCT_INFO.json");
        DataHelper.STRUCT_INFO = JsonConvert.DeserializeObject<Dictionary<string, StructJson>>(json)!.KeyFromHexString();

        // Do after data is loaded/parsed.
        ExtractStyleConstants();

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    private static void ExtractItemInfo() {
        var regex = new Regex(@"item_name_(\d+)");
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\message\ui\ItemName.msg.{Global.MSG_VERSION}")
                     .GetLangIdMap<uint>(name => {
                         var match = regex.Match(name);
                         if (!match.Success) return new(0, true);
                         var value = match.Groups[1].Value;
                         return (uint) int.Parse(value);
                     });
        DataHelper.ITEM_NAME_LOOKUP = msg;
        CreateAssetFile(msg, "ITEM_NAME_LOOKUP");
        CreateConstantsFile(msg[Global.LangIndex.eng].Flip(), "ItemConstants");
    }

    private static void ExtractShopNames() {
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\message\ui\AILocalAreaName.msg.{Global.MSG_VERSION}").GetLangGuidMap();
        DataHelper.SHOP_NAME_LOOKUP = msg;
        CreateAssetFile(msg, "SHOP_NAME_LOOKUP");
        CreateConstantsFile(msg[Global.LangIndex.eng].Flip(), "ShopConstants");
    }

    private static void ExtractStyleConstants() {
        var dataFile = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}\{PathHelper.ARMOR_DATA_PATH}");
        var data     = dataFile.rsz.GetEntryObject<App_ItemArmorData>();
        var styles   = new Dictionary<string, App_TopsStyle>();
        foreach (var armor in data.Params) {
            var value = $"Tops_{armor.StyleNo:D3}";
            try {
                var style = Enum.Parse<App_TopsStyle>(value);
                styles.TryAdd(armor.Name, style);
            } catch (Exception) {
                Console.WriteLine($"Unable to find style {value}.");
            }
        }
        CreateConstantsFile(styles, "StyleConstants");
    }
}