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

    public const string NEXUS_URL              = "https://www.nexusmods.com/dragonsdogma2/mods/522";
    public const string JSON_VERSION_CHECK_URL = $"http://brutsches.com/{CONFIG_NAME}-Editor.version.json";
    public const string WIKI_URL               = NEXUS_URL;

    public static readonly string[] TEST_PATHS = [
        @"\natives\STM\",
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