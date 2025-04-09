using RE_Editor.Common.PakModels;

namespace RE_Editor.Common;

public static partial class PathHelper {
    public const string CONFIG_NAME      = "DD2";
    public const string CHUNK_PATH       = @"V:\DD2\re_chunk_000";
    public const string GAME_PATH        = @"O:\SteamLibrary\steamapps\common\Dragons Dogma 2";
    public const string EXE_PATH         = $@"{GAME_PATH}\DD2_dump.exe";
    public const string IL2CPP_DUMP_PATH = $@"{GAME_PATH}\il2cpp_dump.json";
    public const string ENUM_HEADER_PATH = $@"{GAME_PATH}\Enums_Internal.hpp";
    public const string REFRAMEWORK_PATH = @"R:\Games\Monster Hunter Rise\REFramework";
    public const string MODS_PATH        = @"R:\Games\Dragons Dogma 2\Mods";
    public const string FLUFFY_MODS_PATH = @"R:\Games\Dragons Dogma 2\Fluffy Mod Manager\Games\DragonsDogma2\Mods";
    public const string PAK_LIST_FILE    = @"R:\Games\Dragons Dogma 2\DD2_PC_Release.list";

    public static readonly string[] OBSOLETE_TYPES_TO_CHECK = [
        "user",
        "pfb"
    ];

    public static readonly string[] PAK_PATHS = [
        "re_chunk_000.pak",
        "re_chunk_000.pak.patch_001.pak",
        "re_chunk_000.pak.patch_002.pak",
        "re_chunk_000.pak.patch_003.pak",
        "re_chunk_000.pak.patch_004.pak",
        "re_chunk_000.pak.patch_005.pak",
        "re_chunk_000.pak.patch_006.pak",
        "re_chunk_000.pak.patch_007.pak",
        "re_chunk_000.pak.patch_008.pak",
        "re_chunk_000.pak.patch_009.pak",
        "re_chunk_000.pak.patch_010.pak",
        "re_chunk_000.pak.patch_011.pak",
        "re_chunk_000.pak.patch_012.pak",
        "re_chunk_000.pak.patch_013.pak"
    ];

    public static readonly List<PakDateInfo> PAK_UPDATE_INFO = [
        new(new(2024, 03, 21), "Release", [
            "re_chunk_000.pak"
        ]) {updateName = "2024-03-21"},
        new(new(2024, 03, 29), "Update 1", [
            "re_chunk_000.pak.patch_001.pak"
        ]) {updateName = "2024-03-29"},
        new(new(2024, 04, 09), "Update 2", [
            "re_chunk_000.pak.patch_002.pak"
        ]) {updateName = "2024-04-09"},
        new(new(2024, 04, 25), "Update 3", [
            "re_chunk_000.pak.patch_003.pak"
        ]) {updateName = "2024-04-25"},
        new(new(2024, 05, 15), "Update 4", [
            "re_chunk_000.pak.patch_004.pak"
        ]) {updateName = "2024-05-15"},
        new(new(2024, 05, 31), "Update 5", [
            "re_chunk_000.pak.patch_005.pak"
        ]) {updateName = "2024-05-31"},
        new(new(2024, 06, 27), "Update 6", [
            "re_chunk_000.pak.patch_006.pak"
        ]) {updateName = "2024-06-27"},
        new(new(2024, 07, 12), "Update 7", [
            "re_chunk_000.pak.patch_007.pak"
        ]) {updateName = "2024-07-12"},
        new(new(2024, 09, 17), "Update 8", [
            "re_chunk_000.pak.patch_008.pak"
        ]) {updateName = "2024-09-17"},
        new(new(2024, 10, 04), "Update 9", [
            "re_chunk_000.pak.patch_009.pak"
        ]) {updateName = "2024-10-04"},
        new(new(2024, 10, 17), "Update 10", [
            "re_chunk_000.pak.patch_010.pak"
        ]) {updateName = "2024-10-17"},
        new(new(2024, 10, 31), "Update 11", [
            "re_chunk_000.pak.patch_011.pak"
        ]) {updateName = "2024-10-31"},
        new(new(2025, 01, 29), "Update 12", [
            "re_chunk_000.pak.patch_012.pak"
        ]) {updateName = "2025-01-29"},
        new(new(2025, 04, 09), "Update 13", [
            "re_chunk_000.pak.patch_013.pak"
        ]) {updateName = "2025-04-09"}
    ];

    public const string NEXUS_URL              = "https://www.nexusmods.com/dragonsdogma2/mods/522";
    public const string JSON_VERSION_CHECK_URL = $"http://brutsches.com/{CONFIG_NAME}-Editor.version.json";
    public const string WIKI_URL               = NEXUS_URL;

    public static readonly string[] TEST_PATHS = [
        @"\natives\STM\"
    ];

    public const string ARMOR_DATA_PATH                  = "natives/STM/AppSystem/Item/ItemData/ItemArmorData.user.2";
    public const string ARMOR_UPGRADE_DATA_PATH          = "natives/STM/AppSystem/Item/ItemData/ArmorEnhanceData.user.2";
    public const string ITEM_DATA_PATH                   = "natives/STM/AppSystem/Item/ItemData/ItemData.user.2";
    public const string ITEM_PARAMETERS_PATH             = "natives/STM/AppSystem/Item/ItemParameters/ItemParameters.user.2";
    public const string ITEM_SHOP_DATA_PATH              = "natives/STM/AppSystem/Item/ItemShopData/ItemShopData.user.2";
    public const string JOB_03_PARAM_PATH                = "natives/STM/AppSystem/ch/Common/Human/UserData/Parameter/Job03Parameter.user.2";
    public const string JOB_04_PARAM_PATH                = "natives/STM/AppSystem/ch/Common/Human/UserData/Parameter/Job04Parameter.user.2";
    public const string JOB_06_PARAM_PATH                = "natives/STM/AppSystem/ch/Common/Human/UserData/Parameter/Job06Parameter.user.2";
    public const string JOB_07_PARAM_PATH                = "natives/STM/AppSystem/ch/Common/Human/UserData/Parameter/Job07Parameter.user.2";
    public const string JOB_08_PARAM_PATH                = "natives/STM/AppSystem/ch/Common/Human/UserData/Parameter/Job08Parameter.user.2";
    public const string STAMINA_COMMON_ACTION_PARAM_PATH = "natives/STM/AppSystem/ch/Common/Human/UserData/Parameter/CommonActionStaminaParameter.user.2";
    public const string STAMINA_PARAM_PATH               = "natives/STM/AppSystem/ch/Common/Human/UserData/Parameter/StaminaParameter.user.2";
    public const string SWAP_DATA_MANTLE_PATH            = "natives/STM/AppSystem/CharaEdit/ch000/SwapData/MantleSwapData.user.2";
    public const string SWAP_DATA_PANTS_PATH             = "natives/STM/AppSystem/CharaEdit/ch000/SwapData/PantsSwapData.user.2";
    public const string SWAP_DATA_TOPS_PATH              = "natives/STM/AppSystem/CharaEdit/ch000/SwapData/TopsSwapData.user.2";
    public const string WEAPON_DATA_PATH                 = "natives/STM/AppSystem/Item/ItemData/ItemWeaponData.user.2";
    public const string WEAPON_SETTINGS_PATH             = "natives/STM/AppSystem/Equipment/wp/WeaponSetting.user.2";
    public const string WEAPON_UPGRADE_DATA_PATH         = "natives/STM/AppSystem/Item/ItemData/WeaponEnhanceData.user.2";
}