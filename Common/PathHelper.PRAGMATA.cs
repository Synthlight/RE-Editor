using RE_Editor.Common.PakModels;

namespace RE_Editor.Common;

public static partial class PathHelper {
    public const string CONFIG_NAME      = "PRAGMATA";
    public const string CHUNK_PATH       = @"V:\Pragmata\re_chunk_000";
    public const string GAME_PATH        = @"O:\SteamLibrary\steamapps\common\PRAGMATA SKETCHBOOK";
    public const string GAME_PATH_MSG    = "";
    public const string EXE_PATH         = $@"{GAME_PATH}\PRAGMATA_SKETCHBOOK_dump.exe";
    public const string IL2CPP_DUMP_PATH = $@"{GAME_PATH}\il2cpp_dump.json";
    public const string ENUM_HEADER_PATH = $@"{GAME_PATH}\Enums_Internal.hpp";
    public const string REFRAMEWORK_PATH = @"R:\Games\Monster Hunter Rise\REFramework";
    public const string MODS_PATH        = @"R:\Games\Pragmata\Mods";
    public const string FLUFFY_MODS_PATH = @"R:\Games\Pragmata\FMM\Games\MonsterHunterWilds\Mods";
    public const string PAK_LIST         = @"R:\Games\Pragmata\PS_STM_Demo.list";
    public const string PAK_LIST_MSG     = "";
    public const string R_EASY_RSZ       = "";

    public static readonly string[] OBSOLETE_TYPES_TO_CHECK = [
        "user",
        "pfb",
        "msg"
    ];

    public static readonly string[] PAK_PATHS = [
        "re_chunk_000.pak",
        "re_chunk_000.pak.sub_000.pak"
    ];

    public static readonly PakDateInfo[] PAK_UPDATE_INFO = [
        new(new(2025, 12, 11), "1.000.00.00", [
            "re_chunk_000.pak",
            "re_chunk_000.pak.sub_000.pak"
        ])
    ];

    public const string NEXUS_URL              = "";
    public const string JSON_VERSION_CHECK_URL = $"http://brutsches.com/{CONFIG_NAME}-Editor.version.json";
    public const string WIKI_URL               = NEXUS_URL;
}