using System.Diagnostics.CodeAnalysis;
using System.IO;
using RE_Editor.Common.PakModels;

namespace RE_Editor.Common;

public static partial class PathHelper {
    public const string CONFIG_NAME      = "MHWS";
    public const string CHUNK_PATH       = @"V:\MHWilds\re_chunk_000";
    public const string GAME_PATH        = @"O:\SteamLibrary\steamapps\common\MonsterHunterWilds";
    public const string GAME_PATH_MSG    = "";
    public const string EXE_PATH         = $@"{GAME_PATH}\MonsterHunterWilds_dump.exe";
    public const string IL2CPP_DUMP_PATH = $@"{GAME_PATH}\il2cpp_dump.json";
    public const string ENUM_HEADER_PATH = $@"{GAME_PATH}\Enums_Internal.hpp";
    public const string REFRAMEWORK_PATH = @"R:\Games\Monster Hunter Rise\REFramework";
    public const string MODS_PATH        = @"R:\Games\Monster Hunter Wilds\Mods";
    public const string FLUFFY_MODS_PATH = @"R:\Games\Monster Hunter Wilds\FMM\Games\MonsterHunterWilds\Mods";
    public const string PAK_LIST         = @"R:\Games\Monster Hunter Wilds\MHWs.list";
    public const string PAK_LIST_MSG     = "";

    public static readonly string[] OBSOLETE_TYPES_TO_CHECK = [
        "user",
        "pfb",
        "msg"
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
        "re_chunk_000.pak.sub_000.pak",
        "re_chunk_000.pak.sub_000.pak.patch_001.pak",
        "re_chunk_000.pak.sub_000.pak.patch_002.pak",
        "re_chunk_000.pak.sub_000.pak.patch_003.pak",
        "re_chunk_000.pak.sub_000.pak.patch_004.pak",
        "re_chunk_000.pak.sub_000.pak.patch_005.pak",
        "re_chunk_000.pak.sub_000.pak.patch_006.pak",
        "re_chunk_000.pak.sub_000.pak.patch_007.pak",
        "re_chunk_000.pak.sub_000.pak.patch_008.pak",
        "re_chunk_000.pak.sub_000.pak.patch_009.pak"
    ];

    public static readonly PakDateInfo[] PAK_UPDATE_INFO = [
        new(new(2025, 02, 28), "1.000.00.00", [
            "re_chunk_000.pak",
            "re_chunk_000.pak.sub_000.pak"
        ]),
        new(new(2025, 03, 10), "1.000.05.00", [
            "re_chunk_000.pak.patch_001.pak",
            "re_chunk_000.pak.sub_000.pak.patch_001.pak"
        ]),
        new(new(2025, 04, 03), "1.010.00.00", [
            "re_chunk_000.pak.patch_002.pak",
            "re_chunk_000.pak.sub_000.pak.patch_002.pak"
        ]) {updateName = "Title Update 1"},
        new(new(2025, 04, 16), "1.010.01.00", [
            "re_chunk_000.pak.patch_003.pak",
            "re_chunk_000.pak.sub_000.pak.patch_003.pak"
        ]),
        new(new(2025, 05, 27), "1.011.00.00", [
            "re_chunk_000.pak.patch_004.pak",
            "re_chunk_000.pak.patch_005.pak",
            "re_chunk_000.pak.sub_000.pak.patch_004.pak",
            "re_chunk_000.pak.sub_000.pak.patch_005.pak"
        ]) {updateName = "Street Fighter 6 Special Collaboration"},
        new(new(2025, 06, 30), "1.020.00.00", [
            "re_chunk_000.pak.patch_006.pak",
            "re_chunk_000.pak.sub_000.pak.patch_006.pak"
        ]) {updateName = "Title Update 2"},
        new(new(2025, 07, 01), "1.020.01.00", [
            "re_chunk_000.pak.patch_007.pak",
            "re_chunk_000.pak.sub_000.pak.patch_007.pak"
        ]),
        new(new(2025, 08, 12), "1.021.00.00", [
            "re_chunk_000.pak.patch_008.pak",
            "re_chunk_000.pak.sub_000.pak.patch_008.pak"
        ]) {updateName = "Title Update 2.5"},
        new(new(2025, 08, 14), "1.021.01.00", [
            "re_chunk_000.pak.patch_009.pak",
            "re_chunk_000.pak.sub_000.pak.patch_009.pak"
        ])
    ];

    public const string NEXUS_URL              = "";
    public const string JSON_VERSION_CHECK_URL = $"http://brutsches.com/{CONFIG_NAME}-Editor.version.json";
    public const string WIKI_URL               = NEXUS_URL;

    public const string ARMOR_DATA_PATH                     = @"\natives\STM\GameDesign\Common\Equip\ArmorData.user.3";
    public const string ARMOR_RECIPE_DATA_PATH              = @"\natives\STM\GameDesign\Common\Equip\ArmorRecipeData.user.3";
    public const string ARMOR_SERIES_DATA_PATH              = @"\natives\STM\GameDesign\Common\Equip\ArmorSeriesData.user.3";
    public const string ARMOR_LAYERED_DATA_PATH             = @"\natives\STM\GameDesign\Common\Equip\OuterArmorData.user.3";
    public const string ARMOR_UPGRADE_DATA_PATH             = @"\natives\STM\GameDesign\Common\Equip\ArmorUpgradeData.user.3";
    public const string CHARGE_BLADE_PARAM_PATH             = @"\natives\STM\GameDesign\Player\ActionData\Wp09\GlobalParam\Wp09GlobalActionParam.user.3";
    public const string DECORATION_DATA_PATH                = @"\natives\STM\GameDesign\Common\Equip\AccessoryData.user.3";
    public const string FISH_RANDOM_SIZES_PATH              = @"\natives\STM\GameDesign\Enemy\CommonData\Data\EmCommonRandomSizeFish.user.3";
    public const string GIMMICK_REWARD_DATA_PATH            = @"\natives\STM\GameDesign\Gimmick\Common\GimmickRewardData.user.3";
    public const string ITEM_DATA_PATH                      = @"\natives\STM\GameDesign\Common\Item\ItemData.user.3";
    public const string ITEM_RECIPE_DATA_PATH               = @"\natives\STM\GameDesign\Common\Item\ItemRecipe.user.3";
    public const string ITEM_SHOP_DATA_PATH                 = @"\natives\STM\GameDesign\Facility\ItemShopData.user.3";
    public const string KINSECT_RECIPE_DATA_PATH            = @"\natives\STM\GameDesign\Common\Equip\RodInsectRecipeData.user.3";
    public const string MEDAL_DATA_PATH                     = @"\natives\STM\GameDesign\HunterProfile\UserData\MedalData.user.3";
    public const string MONSTER_RANDOM_SIZES_PATH           = @"\natives\STM\GameDesign\Enemy\CommonData\Data\EmCommonRandomSize.user.3";
    public const string OTOMO_ARMOR_DATA_PATH               = @"\natives\STM\GameDesign\Otomo\DataParam\OtomoArmorData.user.3";
    public const string OTOMO_ARMOR_SERIES_DATA_PATH        = @"\natives\STM\GameDesign\Otomo\DataParam\OtomoEquipSeriesData.user.3";
    public const string OTOMO_ARMOR_LAYERED_DATA_PATH       = @"\natives\STM\GameDesign\Otomo\DataParam\OtomoOuterArmorData.user.3";
    public const string OTOMO_RECIPE_DATA_PATH              = @"\natives\STM\GameDesign\Facility\Data\OtomoEquipRecipe.user.3";
    public const string PENDANT_DATA_PATH                   = @"\natives\STM\GameDesign\Common\Equip\Charm.user.3";
    public const string POPUP_CAMP_PATH                     = @"\natives\STM\System\SystemSetting\CampManagerSetting.user.3";
    public const string PLAYER_ACTIVE_SKILL_PARAM_PATH      = @"\natives\STM\GameDesign\Player\ActionData\Common\GlobalParam\Part\PlayerActiveSkillParam.user.3";
    public const string PLAYER_GLOBAL_PARAM_PATH            = @"\natives\STM\GameDesign\Player\ActionData\Common\GlobalParam\PlayerGlobalParam.user.3";
    public const string PLAYER_ITEM_PARAM_PATH              = @"\natives\STM\GameDesign\Player\ActionData\Common\GlobalParam\PlayerItemParam.user.3";
    public const string PLAYER_SKILL_PARAM_PATH             = @"\natives\STM\GameDesign\Player\ActionData\Common\GlobalParam\Part\PlayerSkillParam.user.3";
    public const string PLAYER_STATUS_PARAM_PATH            = @"\natives\STM\GameDesign\Player\ActionData\Common\GlobalParam\Part\PlayerStatusParam.user.3";
    public const string SKILL_COMMON_DATA_PATH              = @"\natives\STM\GameDesign\Common\Equip\SkillCommonData.user.3";
    public const string TALISMAN_DATA_PATH                  = @"\natives\STM\GameDesign\Common\Equip\AmuletData.user.3";
    public const string TALISMAN_GENERATED_SKILLS_DATA_PATH = @"\natives\STM\GameDesign\Common\Equip\RandomAmuletLotSkillTable.user.3";
    public const string TALISMAN_GENERATED_SLOTS_DATA_PATH  = @"\natives\STM\GameDesign\Common\Equip\RandomAmuletAccSlot.user.3";
    public const string TALISMAN_RECIPE_DATA_PATH           = @"\natives\STM\GameDesign\Common\Equip\AmuletRecipeData.user.3";
    public const string TITLE_CONJUNCTION_DATA_PATH         = @"\natives\STM\GameDesign\HunterProfile\UserData\Title_Conjunction.user.3";
    public const string TITLE_WORD_DATA_PATH                = @"\natives\STM\GameDesign\HunterProfile\UserData\Title_WordData.user.3";

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

    public static IEnumerable<string> GetAllWeaponVisualParamPaths(string platform = "STM") {
        return from file in Directory.EnumerateFiles($@"{CHUNK_PATH}\natives\STM\GameDesign\Equip\_Prefab\Weapon", "*wvp.user.3", SearchOption.AllDirectories)
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