using System.Diagnostics.CodeAnalysis;

namespace RE_Editor.Generator;

public partial class GenerateFiles {
    public const string ROOT_STRUCT_NAMESPACE = "app";

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression")]
    private static readonly List<string> WHITELIST = [
        "App_user_data_AmuletData",
        "App_user_data_AmuletRecipeData",
        "App_user_data_AmuletRecipeData_cData",
        "App_user_data_ArmorData",
        "App_user_data_ArmorData_cData",
        "App_user_data_ArmorRecipeData_cData",
        "App_user_data_ArmorSeriesData",
        "App_user_data_ArmorSeriesData_cData",
        "App_user_data_CampManagerSetting",
        "App_user_data_EmParamRandomSize",
        "App_user_data_EmParamRandomSizeFish",
        "App_user_data_EnemyCategoryList",
        "App_user_data_EnemyReportBossData",
        "App_user_data_EnemyReportBossData_cData",
        "App_user_data_ItemData",
        "App_user_data_ItemData_cData",
        "App_user_data_cItemRecipe",
        "App_user_data_cItemRecipe_cData",
        "App_user_data_ItemShopData",
        "App_user_data_ItemShopData_cData",
        "App_user_data_OuterArmorData",
        "App_user_data_OuterArmorData_cData",
        "App_user_data_OtomoEquipRecipe",
        "App_user_data_OtomoEquipRecipe_cData",
        "App_user_data_PlayerGlobalParam",
        "App_user_data_PlayerItemParam",
        "App_user_data_PlayerSkillParam",
        "App_user_data_PlayerStatusParam",
        "App_user_data_RodInsectRecipeData",
        "App_user_data_RodInsectRecipeData_cData",
        "App_user_data_WeaponData",
        "App_user_data_WeaponData_cData",
        "App_user_data_WeaponRecipeData",
        "App_user_data_WeaponRecipeData_cData",
        "App_user_data_Wp01ActionParam",
        "App_user_data_Wp02ActionParam",
        "App_user_data_Wp03ActionParam",
        "App_user_data_Wp04ActionParam",
        "App_user_data_Wp05ActionParam",
        "App_user_data_Wp06ActionParam",
        "App_user_data_Wp07ActionParam",
        "App_user_data_Wp08ActionParam",
        "App_user_data_Wp09ActionParam",
        "App_user_data_Wp10ActionParam",
        "App_user_data_Wp11ActionParam",
        "App_user_data_Wp12ActionParam",
        "App_user_data_Wp13ActionParam",
        // For debugging:
        "App_cEnemyCheckBox", // Has a `_Serializable` field where the wrapper is another object.
        "App_user_data_EmParamParts_cMultiParts", // Has a `_Serializable` field that doesn't target `_Fixed`.
        "App_user_data_NpcDesireLotteryTable_Health", // Has a `_Serializable` field with a button type that doesn't have an explicit `buttonPrimitive` defined.
    ];
}