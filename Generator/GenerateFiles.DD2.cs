using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using RE_Editor.Common;
using RE_Editor.Common.Models;

namespace RE_Editor.Generator;

public partial class GenerateFiles {
    public const string ROOT_STRUCT_NAMESPACE = "app";

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression")]
    private static readonly List<string> WHITELIST = [
        "App_ArmorEnhanceParam",
        "App_DropPartsUserData",
        "App_GameSystemUserData",
        "App_GimmickData",
        "App_GimmickParamBase",
        "App_Gm80_001Param",
        "App_HumanSpeedParameter",
        "App_HumanStaminaParameter",
        "App_HumanStaminaParameterAdditional",
        "App_HumanStaminaParameterAdditionalData",
        "App_ItemArmorData",
        "App_ItemData",
        "App_ItemParameters",
        "App_ItemShopData",
        "App_ItemWeaponData",
        "App_Job03Parameter",
        "App_Job04Parameter",
        "App_Job06Parameter",
        "App_Job07Parameter",
        "App_Job08Parameter",
        "App_ShellAdditionalParameter",
        "App_TopsSwapData",
        "App_WeaponCatalogData",
        "App_WeaponEnhanceParam",
        "App_WeaponSetting",
        "App_Job04Parameter_CuttingWindParameter_LevelParameter",
    ];

    // In-case some structs generate badly.
    // NOTE: CRCs here will be overwritten to preserve game update compatibility.
    private static readonly Dictionary<uint, StructJson> STRUCT_OVERRIDES = JsonConvert.DeserializeObject<Dictionary<string, StructJson>>("""
                                                                                                                                          {
                                                                                                                                          }
                                                                                                                                          """)!.KeyFromHexString();
}