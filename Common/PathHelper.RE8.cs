using RE_Editor.Common.PakModels;

namespace RE_Editor.Common;

public static partial class PathHelper {
    public const string CONFIG_NAME      = "RE8";
    public const string CHUNK_PATH       = @"V:\RE8\re_chunk_000";
    public const string GAME_PATH        = @"O:\SteamLibrary\steamapps\common\Resident Evil Village BIOHAZARD VILLAGE";
    public const string GAME_PATH_MSG    = "";
    public const string EXE_PATH         = $@"{GAME_PATH}\re8_dump.exe";
    public const string IL2CPP_DUMP_PATH = $@"{GAME_PATH}\il2cpp_dump.json";
    public const string ENUM_HEADER_PATH = $@"{GAME_PATH}\Enums_Internal.hpp";
    public const string REFRAMEWORK_PATH = @"R:\Games\Monster Hunter Rise\REFramework";
    public const string MODS_PATH        = @"R:\Games\Resident Evil Village\Mods";
    public const string FLUFFY_MODS_PATH = @"R:\Games\Resident Evil Village\Fluffy Mod Manager\Games\RE8\Mods";
    public const string PAK_LIST         = "";
    public const string PAK_LIST_MSG     = "";

    public static readonly string[] OBSOLETE_TYPES_TO_CHECK = [];

    public static readonly string[] PAK_PATHS = [];

    public static readonly PakDateInfo[] PAK_UPDATE_INFO = [];

    public const string NEXUS_URL              = "";
    public const string JSON_VERSION_CHECK_URL = "";
    public const string WIKI_URL               = "";

    public const string ITEM_DATA_PATH = "/natives/STM/SingletonUserDatas/ItemSpecificationData.user.2";
}