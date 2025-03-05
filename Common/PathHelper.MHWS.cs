using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace RE_Editor.Common;

public static partial class PathHelper {
    public const string CONFIG_NAME      = "MHWS";
    public const string CHUNK_PATH       = @"V:\MHWilds\re_chunk_000";
    public const string GAME_PATH        = @"O:\SteamLibrary\steamapps\common\MonsterHunterWilds";
    public const string EXE_PATH         = $@"{GAME_PATH}\MonsterHunterWilds_dump.exe";
    public const string IL2CPP_DUMP_PATH = $@"{GAME_PATH}\il2cpp_dump.json";
    public const string ENUM_HEADER_PATH = $@"{GAME_PATH}\Enums_Internal.hpp";
    public const string REFRAMEWORK_PATH = @"R:\Games\Monster Hunter Rise\REFramework";
    public const string MODS_PATH        = @"R:\Games\Monster Hunter Wilds\Mods";
    public const string FLUFFY_MODS_PATH = @"R:\Games\Monster Hunter Wilds\FMM\Games\MonsterHunterWilds\Mods";

    public const string NEXUS_URL              = "";
    public const string JSON_VERSION_CHECK_URL = $"http://brutsches.com/{CONFIG_NAME}-Editor.version.json";
    public const string WIKI_URL               = NEXUS_URL;

    public static readonly string[] TEST_PATHS = [
        @"\natives\STM"
    ];

    public const string ARMOR_DATA_PATH           = @"\natives\STM\GameDesign\Common\Equip\ArmorData.user.3";
    public const string ARMOR_RECIPE_DATA_PATH    = @"\natives\STM\GameDesign\Common\Equip\ArmorRecipeData.user.3";
    public const string ARMOR_SERIES_DATA_PATH    = @"\natives\STM\GameDesign\Common\Equip\ArmorSeriesData.user.3";
    public const string CHARGE_BLADE_PARAM_PATH   = @"\natives\STM\GameDesign\Player\ActionData\Wp09\GlobalParam\Wp09GlobalActionParam.user.3";
    public const string DECORATION_DATA_PATH      = @"\natives\STM\GameDesign\Common\Equip\AccessoryData.user.3";
    public const string FISH_RANDOM_SIZES_PATH    = @"\natives\STM\GameDesign\Enemy\CommonData\Data\EmCommonRandomSizeFish.user.3";
    public const string ITEM_DATA_PATH            = @"\natives\STM\GameDesign\Common\Item\ItemData.user.3";
    public const string ITEM_RECIPE_DATA_PATH     = @"\natives\STM\GameDesign\Common\Item\ItemRecipe.user.3";
    public const string ITEM_SHOP_DATA_PATH       = @"\natives\STM\GameDesign\Facility\ItemShopData.user.3";
    public const string KINSECT_RECIPE_DATA_PATH  = @"\natives\STM\GameDesign\Common\Equip\RodInsectRecipeData.user.3";
    public const string MONSTER_RANDOM_SIZES_PATH = @"\natives\STM\GameDesign\Enemy\CommonData\Data\EmCommonRandomSize.user.3";
    public const string OTOMO_ARMOR_DATA_PATH     = @"\natives\STM\GameDesign\Otomo\DataParam\OtomoArmorData.user.3";
    public const string OTOMO_RECIPE_DATA_PATH    = @"\natives\STM\GameDesign\Facility\Data\OtomoEquipRecipe.user.3";
    public const string POPUP_CAMP_PATH           = @"\natives\STM\System\SystemSetting\CampManagerSetting.user.3";
    public const string TALISMAN_DATA_PATH        = @"\natives\STM\GameDesign\Common\Equip\AmuletData.user.3";
    public const string TALISMAN_RECIPE_DATA_PATH = @"\natives\STM\GameDesign\Common\Equip\AmuletRecipeData.user.3";

    public static IEnumerable<string> GetAllWeaponFilePaths(WeaponDataType type, string platform = "STM") {
        var postfix = type switch {
            WeaponDataType.Recipe => "Recipe",
            WeaponDataType.Tree => "Tree",
            _ => ""
        };

        return Global.WEAPON_TYPES.Select(s => @$"\natives\{platform}\GameDesign\Common\Weapon\{s}{postfix}.user.3");
    }

    public static IEnumerable<string> GetAllCampSafetyFilePaths(string platform = "STM") {
        return from file in Directory.EnumerateFiles($@"{CHUNK_PATH}\natives\STM\GameDesign\Gimmick\Gm800", "*AaaUniqueParam.user.3", SearchOption.AllDirectories)
               where File.Exists(file)
               select file.Replace(CHUNK_PATH, "");
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum WeaponDataType {
        Base,
        Recipe,
        Tree
    }
}