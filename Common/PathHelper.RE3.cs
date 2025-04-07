using RE_Editor.Common.PakModels;

namespace RE_Editor.Common;

public static partial class PathHelper {
    public const string CONFIG_NAME      = "RE3";
    public const string CHUNK_PATH       = @"V:\RE3\re_chunk_000";
    public const string GAME_PATH        = @"O:\SteamLibrary\steamapps\common\RE3";
    public const string EXE_PATH         = $@"{GAME_PATH}\re3_dump.exe";
    public const string IL2CPP_DUMP_PATH = $@"{GAME_PATH}\il2cpp_dump.json";
    public const string ENUM_HEADER_PATH = $@"{GAME_PATH}\Enums_Internal.hpp";
    public const string REFRAMEWORK_PATH = @"R:\Games\Monster Hunter Rise\REFramework";
    public const string MODS_PATH        = @"R:\Games\Resident Evil 3 Remake\Mods\RTX";
    public const string FLUFFY_MODS_PATH = @"R:\Games\Resident Evil 3 Remake\Mods\Fluffy Manager 5000\Games\RE3R\Mods";
    public const string PAK_LIST_FILE    = "";

    public static readonly string[] OBSOLETE_TYPES_TO_CHECK = [];

    public static readonly string[] PAK_PATHS = [];

    public static readonly List<PakDateInfo> PAK_UPDATE_INFO = [];

    public const string NEXUS_URL              = "";
    public const string JSON_VERSION_CHECK_URL = "";
    public const string WIKI_URL               = "";

    public static readonly string[] TEST_PATHS = [
        @"\natives\STM\"
    ];

    public const string ITEM_DATA_PATH               = "/natives/STM/Escape/UserData/Item/EsItemUserData.user.2";
    public const string WEAPON_BULLET_USER_DATA_PATH = "/natives/STM/SectionRoot/UserData/System/Inventory/WeaponBulletUserData.user.2";
}