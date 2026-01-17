using System;
using System.Collections.Generic;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Data.MHWS;

namespace RE_Editor.Data;

public static partial class DataInit {
    // ReSharper disable once IdentifierTypo
    private static void LoadDicts() {
        DataHelper.ARMOR_INFO_LOOKUP_BY_GUID            = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.ARMOR_INFO_LOOKUP_BY_GUID);
        DataHelper.ARMOR_SERIES_INFO_LOOKUP_BY_GUID     = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.ARMOR_SERIES_INFO_LOOKUP_BY_GUID);
        DataHelper.ARMOR_SERIES_BY_ENUM_VALUE           = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.ARMOR_SERIES_BY_ENUM_VALUE);
        DataHelper.ARMOR_LAYERED_INFO_LOOKUP_BY_GUID    = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.ARMOR_LAYERED_INFO_LOOKUP_BY_GUID);
        DataHelper.ARMOR_LAYERED_NAME_LOOKUP_BY_VALUE   = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.ARMOR_LAYERED_NAME_LOOKUP_BY_VALUE);
        DataHelper.ARTIAN_INFO_LOOKUP_BY_GUID           = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.ARTIAN_INFO_LOOKUP_BY_GUID);
        DataHelper.DECORATION_INFO_LOOKUP_BY_ENUM_VALUE = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.DECORATION_INFO_LOOKUP_BY_ENUM_VALUE);
        DataHelper.DECORATION_INFO_LOOKUP_BY_GUID       = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.DECORATION_INFO_LOOKUP_BY_GUID);
        DataHelper.DLC_INFO_LOOKUP_BY_GUID              = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.DLC_INFO_LOOKUP_BY_GUID);
        DataHelper.DLC_NAME_LOOKUP_BY_ENUM_VALUE        = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.DLC_NAME_LOOKUP_BY_ENUM_VALUE);
        DataHelper.ENEMY_NAME_LOOKUP_BY_ENUM_VALUE      = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.ENEMY_NAME_LOOKUP_BY_ENUM_VALUE);
        DataHelper.GIMMICK_NAME_LOOKUP_BY_ENUM_VALUE    = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.GIMMICK_NAME_LOOKUP_BY_ENUM_VALUE);
        DataHelper.GIMMICK_INFO_LOOKUP_BY_GUID          = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.GIMMICK_INFO_LOOKUP_BY_GUID);
        DataHelper.ITEM_NAME_LOOKUP                     = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.ITEM_NAME_LOOKUP);
        DataHelper.ITEM_DESC_LOOKUP                     = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(Assets.ITEM_DESC_LOOKUP);
        DataHelper.ITEM_INFO_LOOKUP_BY_GUID             = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.ITEM_INFO_LOOKUP_BY_GUID);
        DataHelper.MEDAL_NAME_LOOKUP_BY_ENUM_VALUE      = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.MEDAL_NAME_LOOKUP_BY_ENUM_VALUE);
        DataHelper.MEDAL_INFO_LOOKUP_BY_GUID            = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.MEDAL_INFO_LOOKUP_BY_GUID);
        DataHelper.MENU_INFO_LOOKUP_BY_GUID             = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.MENU_INFO_LOOKUP_BY_GUID);
        DataHelper.NPC_NAME_LOOKUP_BY_ENUM_VALUE        = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.NPC_NAME_LOOKUP_BY_ENUM_VALUE);
        DataHelper.OTOMO_INFO_LOOKUP_BY_GUID            = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.OTOMO_INFO_LOOKUP_BY_GUID);
        DataHelper.OTOMO_NAME_LOOKUP_BY_ENUM_VALUE      = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.OTOMO_NAME_LOOKUP_BY_ENUM_VALUE);
        DataHelper.OTOMO_SERIES_INFO_LOOKUP_BY_GUID     = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.OTOMO_SERIES_INFO_LOOKUP_BY_GUID);
        DataHelper.OTOMO_SERIES_BY_ENUM_VALUE           = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.OTOMO_SERIES_BY_ENUM_VALUE);
        DataHelper.OTOMO_LAYERED_INFO_LOOKUP_BY_GUID    = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.OTOMO_LAYERED_INFO_LOOKUP_BY_GUID);
        DataHelper.OTOMO_LAYERED_NAME_LOOKUP_BY_VALUE   = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.OTOMO_LAYERED_NAME_LOOKUP_BY_VALUE);
        DataHelper.PENDANT_NAME_LOOKUP_BY_ENUM_VALUE    = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.PENDANT_NAME_LOOKUP_BY_ENUM_VALUE);
        DataHelper.PENDANT_INFO_LOOKUP_BY_GUID          = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.PENDANT_INFO_LOOKUP_BY_GUID);
        DataHelper.QUEST_INFO_LOOKUP_BY_ENUM_VALUE      = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.QUEST_INFO_LOOKUP_BY_ENUM_VALUE);
        DataHelper.SKILL_NAME_BY_ENUM_VALUE             = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.SKILL_NAME_BY_ENUM_VALUE);
        DataHelper.TALISMAN_INFO_LOOKUP_BY_GUID         = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.TALISMAN_INFO_LOOKUP_BY_GUID);
        DataHelper.TITLE_INFO_LOOKUP_BY_GUID            = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.TITLE_INFO_LOOKUP_BY_GUID);
        DataHelper.WEAPON_INFO_LOOKUP_BY_GUID           = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.WEAPON_INFO_LOOKUP_BY_GUID);
        DataHelper.WEAPON_SERIES_BY_ENUM_VALUE          = DataHelper.LoadDict<Global.LangIndex, Dictionary<int, string>>(Assets.WEAPON_SERIES_BY_ENUM_VALUE);
        DataHelper.WEAPON_LAYERED_INFO_LOOKUP_BY_GUID   = DataHelper.LoadDict<Global.LangIndex, Dictionary<Guid, string>>(Assets.WEAPON_LAYERED_INFO_LOOKUP_BY_GUID);

        foreach (var lang in Global.LANGUAGES) {
            DataHelper.ITEM_NAME_LOOKUP[lang][0]         = "None";
            DataHelper.ITEM_DESC_LOOKUP[lang][0]         = "None";
            DataHelper.SKILL_NAME_BY_ENUM_VALUE[lang][0] = "None";
        }
    }
}