using System.Diagnostics;
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
    public static void Main() {
        ExtractItemInfo();
    }

    private static void ExtractItemInfo() {
        var regex = new Regex(@"ItemName_(\d+)");
        var msg = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\ui\message\asset\ItemName.msg.{Global.MSG_VERSION}")
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
}