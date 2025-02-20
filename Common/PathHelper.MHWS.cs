using System.Diagnostics.CodeAnalysis;

namespace RE_Editor.Common;

public static partial class PathHelper {
    public const string CONFIG_NAME      = "MHWS";
    public const string CHUNK_PATH       = @"V:\MHWilds\re_chunk_000";
    public const string GAME_PATH        = @"O:\SteamLibrary\steamapps\common\MonsterHunterWildsBetatest";
    public const string EXE_PATH         = $@"{GAME_PATH}\MonsterHunterWildsBeta_dump.exe";
    public const string IL2CPP_DUMP_PATH = $@"{GAME_PATH}\il2cpp_dump.json";
    public const string ENUM_HEADER_PATH = $@"{GAME_PATH}\Enums_Internal.hpp";
    public const string REFRAMEWORK_PATH = @"R:\Games\Monster Hunter Rise\REFramework";
    public const string MODS_PATH        = @"R:\Games\Monster Hunter Wilds\Mods";
    public const string FLUFFY_MODS_PATH = @"R:\Games\Monster Hunter Wilds\FMM\Games\MonsterHunterWildsBeta\Mods";
    public const string PYTHON38_PATH    = @"R:\Games\Monster Hunter Rise\REFramework\reversing\rsz\venv\Scripts\python.exe";

    public const string NEXUS_URL              = "";
    public const string JSON_VERSION_CHECK_URL = $"http://brutsches.com/{CONFIG_NAME}-Editor.version.json";
    public const string WIKI_URL               = "https://github.com/Synthlight/MHR-Editor/wiki";

    public static readonly string[] TEST_PATHS = [
        @"\natives\STM"
    ];

    public const string AMULETS_RECIPE_DATA_PATH  = @"\natives\STM\GameDesign\Common\Equip\AmuletRecipeData.user.3";
    public const string ARMOR_DATA_PATH           = @"\natives\STM\GameDesign\Common\Equip\ArmorData.user.3";
    public const string ARMOR_RECIPE_DATA_PATH    = @"\natives\STM\GameDesign\Common\Equip\ArmorRecipeData.user.3";
    public const string ARMOR_SERIES_DATA_PATH    = @"\natives\STM\GameDesign\Common\Equip\ArmorSeriesData.user.3";
    public const string CHARGE_BLADE_PARAM_PATH   = @"\natives\STM\GameDesign\Player\ActionData\Wp09\GlobalParam\Wp09GlobalActionParam.user.3";
    public const string FISH_RANDOM_SIZES_PATH    = @"\natives\STM\GameDesign\Enemy\CommonData\Data\EmCommonRandomSizeFish.user.3";
    public const string ITEM_DATA_PATH            = @"\natives\STM\GameDesign\Common\Item\ItemData.user.3";
    public const string ITEM_RECIPE_DATA_PATH     = @"\natives\STM\GameDesign\Common\Item\ItemRecipe.user.3";
    public const string KINSECT_RECIPE_DATA_PATH  = @"\natives\STM\GameDesign\Common\Item\RodInsectRecipeData.user.3";
    public const string MONSTER_RANDOM_SIZES_PATH = @"\natives\STM\GameDesign\Enemy\CommonData\Data\EmCommonRandomSize.user.3";
    public const string OTOMO_RECIPE_DATA_PATH    = @"\natives\STM\GameDesign\Facility\Data\OtomoEquipRecipe.user.3";
    public const string POPUP_CAMP_PATH           = @"\natives\STM\System\SystemSetting\CampManagerSetting.user.3";

    public static IEnumerable<string> GetAllWeaponFilePaths(WeaponDataType type, string platform = "STM") {
        var postfix                                = "";
        if (type == WeaponDataType.Recipe) postfix = "Recipe";

        return Global.WEAPON_TYPES.Select(s => @$"\natives\{platform}\GameDesign\Common\Weapon\{s}{postfix}.user.3");
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum WeaponDataType {
        Base,
        Recipe
    }
}