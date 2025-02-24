using System.Diagnostics.CodeAnalysis;

namespace RE_Editor.Common;

public static partial class PathHelper {
    public const string CONFIG_NAME      = "MHR";
    public const string CHUNK_PATH       = @"V:\MHR\re_chunk_000";
    public const string GAME_PATH        = @"O:\SteamLibrary\steamapps\common\MonsterHunterRise";
    public const string EXE_PATH         = $@"{GAME_PATH}\MonsterHunterRise_dump.exe";
    public const string IL2CPP_DUMP_PATH = $@"{GAME_PATH}\il2cpp_dump.json";
    public const string ENUM_HEADER_PATH = $@"{GAME_PATH}\Enums_Internal.hpp";
    public const string REFRAMEWORK_PATH = @"R:\Games\Monster Hunter Rise\REFramework";
    public const string MODS_PATH        = @"R:\Games\Monster Hunter Rise\Mods";
    public const string FLUFFY_MODS_PATH = @"R:\Games\Monster Hunter Rise\modmanager\Games\MHRISE\Mods";

    public const string NEXUS_URL              = "https://www.nexusmods.com/monsterhunterrise/mods/114";
    public const string JSON_VERSION_CHECK_URL = $"http://brutsches.com/{CONFIG_NAME}-Editor.version.json";
    public const string WIKI_URL               = NEXUS_URL;

    public static readonly string[] TEST_PATHS = [
        @"\natives\STM"
    ];

    public const string WIKI_PATH = @"R:\Games\Monster Hunter Rise\Wiki Dump";

    public const string GUILD_CARD_TITLE_DATA             = @"\natives\STM\data\Define\Common\HunterRecord\AchievementUserDataAsset.user.2";
    public const string ARMOR_BASE_PATH                   = @"\natives\STM\data\Define\Player\Armor\ArmorBaseData.user.2";
    public const string ARMOR_RECIPE_PATH                 = @"\natives\STM\data\Define\Player\Armor\ArmorProductData.user.2";
    public const string LAYERED_ARMOR_BASE_PATH           = @"\natives\STM\data\Define\Player\Armor\PlOverwearBaseData.user.2";
    public const string LAYERED_ARMOR_RECIPE_PATH         = @"\natives\STM\data\Define\Player\Armor\PlOverwearProductUserData.user.2";
    public const string AUGMENT_ARMOR_MATERIAL_BASE_PATH  = @"\natives\STM\data\Define\Player\Equip\CustomBuildup\CustomBuildupArmorMaterialUserData.user.2";
    public const string AUGMENT_ARMOR_ENABLE_BASE_PATH    = @"\natives\STM\data\Define\Player\Equip\CustomBuildup\CustomBuildupArmorOpenUserData.user.2";
    public const string AUGMENT_WEAPON_MATERIAL_BASE_PATH = @"\natives\STM\data\Define\Player\Equip\CustomBuildup\CustomBuildupWeaponMaterialUserData.user.2";
    public const string AUGMENT_WEAPON_ENABLE_BASE_PATH   = @"\natives\STM\data\Define\Player\Equip\CustomBuildup\CustomBuildupWeaponOpenUserData.user.2";
    public const string DECORATION_PATH                   = @"\natives\STM\data\Define\Player\Equip\Decorations\DecorationsBaseData.user.2";
    public const string DECORATION_RECIPE_PATH            = @"\natives\STM\data\Define\Player\Equip\Decorations\DecorationsProductData.user.2";
    public const string RAMPAGE_DECORATION_PATH           = @"\natives\STM\data\Define\Player\Equip\HyakuryuDeco\HyakuryuDecoBaseData.user.2";
    public const string RAMPAGE_DECORATION_RECIPE_PATH    = @"\natives\STM\data\Define\Player\Equip\HyakuryuDeco\HyakuryuDecoProductData.user.2";
    public const string GUN_LANCE_BASE_DATA_PATH          = @"\natives\STM\data\Define\Player\Weapon\GunLance\GunLanceBaseData.user.2";
    public const string INSECT_GLAIVE_BASE_DATA_PATH      = @"\natives\STM\data\Define\Player\Weapon\InsectGlaive\InsectGlaiveBaseData.user.2";
    public const string PLAYER_SKILL_PATH                 = @"\natives\STM\data\Define\Player\Skill\PlEquipSkill\PlEquipSkillBaseData.user.2";
    public const string RAMPAGE_SKILL_RECIPE_PATH         = @"\natives\STM\data\Define\Player\Skill\PlHyakuryuSkill\HyakuryuSkillRecipeData.user.2";
    public const string CAT_ARMOR_RECIPE_PATH             = @"\natives\STM\data\Define\Otomo\Equip\Armor\OtAirouArmorProductData.user.2";
    public const string DOG_ARMOR_RECIPE_PATH             = @"\natives\STM\data\Define\Otomo\Equip\Armor\OtDogArmorProductData.user.2";
    public const string CAT_DOG_LAYERED_ARMOR_RECIPE_PATH = @"\natives\STM\data\Define\Otomo\Equip\Overwear\OtOverwearRecipeData.user.2";
    public const string CAT_WEAPON_RECIPE_PATH            = @"\natives\STM\data\Define\Otomo\Equip\Weapon\OtAirouWeaponProductData.user.2";
    public const string DOG_WEAPON_RECIPE_PATH            = @"\natives\STM\data\Define\Otomo\Equip\Weapon\OtDogWeaponProductData.user.2";
    public const string ITEM_DATA_PATH                    = @"\natives\STM\data\System\ContentsIdSystem\Item\Normal\ItemData.user.2";
    public const string PETALACE_DATA_PATH                = @"\natives\STM\data\System\ContentsIdSystem\LvBuffCage\Normal\NormalLvBuffCageBaseData.user.2";

    public static IEnumerable<string> GetAllWeaponFilePaths(WeaponDataType type, string platform = "STM") {
        return Global.WEAPON_TYPES.Select(s => @$"\natives\{platform}\data\Define\Player\Weapon\{s}\{s}{type}Data.user.2");
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum WeaponDataType {
        Base,
        Change,
        OverwearBase,
        OverwearProduct,
        Process,
        Product,
        UpdateTree
    }
}