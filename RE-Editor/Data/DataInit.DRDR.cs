using System;
using System.Collections.Generic;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Data.DRDR;

namespace RE_Editor.Data;

public static partial class DataInit {
    // ReSharper disable once IdentifierTypo
    private static void LoadDicts() {
        DataHelper.ITEM_NAME_LOOKUP = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.ITEM_NAME_LOOKUP);

        foreach (var lang in Global.LANGUAGES) {
            DataHelper.ITEM_NAME_LOOKUP[lang][0] = "None";
        }
    }
}