﻿using System.Diagnostics.CodeAnalysis;

namespace RE_Editor.Generator;

public partial class GenerateFiles {
    public const string ROOT_STRUCT_NAMESPACE = "chainsaw";

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression")]
    private static readonly List<string> WHITELIST = [
        "Chainsaw_ItemDefinitionUserData",
        "Chainsaw_InventoryCatalogUserData",
        "Chainsaw_ItemCraftSettingUserData",
        "Chainsaw_HarpoonShellInfoUserData",
        "Chainsaw_WeaponEquipParamCatalogUserData",
        "Chainsaw_WeaponCustomUserdata",
        "Chainsaw_WeaponDetailCustomUserdata",
        "Chainsaw_BulletShellInfoUserData",
        "Chainsaw_InGameShopItemSettingUserdata",
        "Chainsaw_BombShellInfoUserData",
        "Chainsaw_GrenadeShellInfoUserData",
        "Chainsaw_RandomDrop_CommonDropTableUserdata",

        // Needs `share.PrefabController`.
        //"Chainsaw_BombShellGeneratorUserData",
        //"Chainsaw_GrenadeShellGeneratorUserData",

        // Inherited.
        "Chainsaw_ItemUseResult",
        "Chainsaw_ItemCraftResult",
        "Chainsaw_ItemCraftResultSetting",
        "Chainsaw_ItemCraftMaterial",
        "Chainsaw_ItemCraftBonusSetting",
        "Chainsaw_ItemCraftRecipe",
        "Chainsaw_ItemCraftSettingUserdata",
        "Chainsaw_WeaponAdaptiveFeedBackUserData",
        "Chainsaw_WeaponAdaptiveFeedBackParam",

        // Sub-parts not generated by themselves.
        "Chainsaw_WeaponPartsCustom",
        "Chainsaw_CommonLevelInWeapon",
        "Chainsaw_CustomLevelInWeapon",
        "Chainsaw_IndividualLevelInWeapon",
        "Chainsaw_LimitBreakLevelInWeapon",
        "Chainsaw_WeaponItem",
        "Chainsaw_UniqueItem",
        "Chainsaw_ItemCraftGenerateNumUniqueSetting",
        "Chainsaw_SpCategory00EvaluationSetting",
        "Chainsaw_SpCategory01EvaluationSetting",
        "Chainsaw_SpCategory02EvaluationSetting",
        "Chainsaw_SpCategory03EvaluationSetting",

        // For debugging:
        "Chainsaw_Ch4fez0ParamUserData", // Has a `Struct` `System.ValueTuple`1<System.Single>`.
    ];
}