using RE_Editor.Common.Data;
using RE_Editor.Common;
using RE_Editor.Data.RE2;
using System.Collections.Generic;

namespace RE_Editor.Data;

public static partial class DataInit {
    // ReSharper disable once IdentifierTypo
    private static void LoadDicts() {
        DataHelper.ITEM_NAME_LOOKUP   = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.ITEM_NAME_LOOKUP);
        DataHelper.WEAPON_NAME_LOOKUP = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.WEAPON_NAME_LOOKUP);
    }
}