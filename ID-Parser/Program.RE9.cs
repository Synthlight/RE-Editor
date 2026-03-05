using System.Text.RegularExpressions;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;

namespace RE_Editor.ID_Parser;

public static partial class Program {
    public static void Go() {
        //ExtractChallengeInfo(); // Errors reading the file.
        ExtractItemInfo();
    }

    private static void ExtractChallengeInfo() {
        var msg   = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\Message\GameSystem\Bonus.msg.{Global.MSG_VERSION}");
        var regex = new Regex(@"(bo[\d_]+)_Title");
        var msgByValue = msg.GetLangIdMap<string>(name => {
            var match = regex.Match(name);
            if (!match.Success) return new(null!, true);
            var value = match.Groups[1].ToString();
            return value;
        });
        DataHelper.CHALLENGE_NAME_LOOKUP_BY_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "CHALLENGE_NAME_LOOKUP_BY_VALUE");
        CreateConstantsFile(msgByValue[Global.LangIndex.eng].Flip(), "ChallengeConstants");
    }

    private static void ExtractItemInfo() {
        var msg   = MSG.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\Message\GameSystem\Item.msg.{Global.MSG_VERSION}");
        var regex = new Regex(@"Item_(it[\d_]+)");
        var msgByValue = msg.GetLangIdMap<string>(name => {
            var match = regex.Match(name);
            if (!match.Success) return new(null!, true);
            var value = match.Groups[1].ToString();
            return value;
        });
        DataHelper.ITEM_NAME_LOOKUP_BY_VALUE = msgByValue;
        CreateAssetFile(msgByValue, "ITEM_NAME_LOOKUP_BY_VALUE");
        CreateConstantsFile(msgByValue[Global.LangIndex.eng].Flip(), "ItemConstants");
    }
}