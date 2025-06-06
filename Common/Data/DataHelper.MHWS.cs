using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS8618

namespace RE_Editor.Common.Data;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
public static partial class DataHelper {
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> ARMOR_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> ARMOR_SERIES_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<int, string>>  ARMOR_SERIES_BY_ENUM_VALUE;
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> ARMOR_LAYERED_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<int, string>>  ARMOR_LAYERED_NAME_LOOKUP_BY_VALUE;
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> ARTIAN_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> DECORATION_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<int, string>>  DECORATION_INFO_LOOKUP_BY_ENUM_VALUE;
    public static Dictionary<Global.LangIndex, Dictionary<int, string>>  ENEMY_NAME_LOOKUP_BY_ENUM_VALUE;
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> ITEM_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<uint, string>> ITEM_NAME_LOOKUP;
    public static Dictionary<Global.LangIndex, Dictionary<uint, string>> ITEM_DESC_LOOKUP;
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> MEDAL_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<int, string>>  MEDAL_NAME_LOOKUP_BY_ENUM_VALUE;
    public static Dictionary<Global.LangIndex, Dictionary<int, string>>  NPC_NAME_LOOKUP_BY_ENUM_VALUE;
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> OTOMO_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<int, string>>  OTOMO_NAME_LOOKUP_BY_ENUM_VALUE;
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> OTOMO_SERIES_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<int, string>>  OTOMO_SERIES_BY_ENUM_VALUE;
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> OTOMO_LAYERED_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<int, string>>  OTOMO_LAYERED_NAME_LOOKUP_BY_VALUE;
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> PENDANT_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<int, string>>  PENDANT_NAME_LOOKUP_BY_ENUM_VALUE;
    public static Dictionary<Global.LangIndex, Dictionary<int, string>>  QUEST_INFO_LOOKUP_BY_ENUM_VALUE;
    public static Dictionary<Global.LangIndex, Dictionary<int, string>>  SKILL_NAME_BY_ENUM_VALUE;
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> TALISMAN_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> TITLE_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<Guid, string>> WEAPON_INFO_LOOKUP_BY_GUID;
    public static Dictionary<Global.LangIndex, Dictionary<int, string>>  WEAPON_SERIES_BY_ENUM_VALUE;
}