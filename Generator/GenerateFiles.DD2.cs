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

    // Because REF dumps with a hard-coded `via.AnimationCurve` (and a few others) to fix some issue with RE4 dump, and it causes issues here.
    // So we need to override it with the original.
    private static readonly Dictionary<uint, StructJson> STRUCT_OVERRIDES = JsonConvert.DeserializeObject<Dictionary<string, StructJson>>("""
                                                                                                                                          {
                                                                                                                                              "eab06d4b": {
                                                                                                                                                  "crc": "d8a2551d",
                                                                                                                                                  "fields": [
                                                                                                                                                      {
                                                                                                                                                          "align": 16,
                                                                                                                                                          "array": true,
                                                                                                                                                          "name": "v0",
                                                                                                                                                          "native": true,
                                                                                                                                                          "original_type": "",
                                                                                                                                                          "size": 16,
                                                                                                                                                          "type": "Data"
                                                                                                                                                      },
                                                                                                                                                      {
                                                                                                                                                          "align": 4,
                                                                                                                                                          "array": false,
                                                                                                                                                          "name": "v1",
                                                                                                                                                          "native": true,
                                                                                                                                                          "original_type": "",
                                                                                                                                                          "size": 4,
                                                                                                                                                          "type": "Data"
                                                                                                                                                      },
                                                                                                                                                      {
                                                                                                                                                          "align": 4,
                                                                                                                                                          "array": false,
                                                                                                                                                          "name": "v2",
                                                                                                                                                          "native": true,
                                                                                                                                                          "original_type": "",
                                                                                                                                                          "size": 4,
                                                                                                                                                          "type": "Data"
                                                                                                                                                      },
                                                                                                                                                      {
                                                                                                                                                          "align": 4,
                                                                                                                                                          "array": false,
                                                                                                                                                          "name": "v3",
                                                                                                                                                          "native": true,
                                                                                                                                                          "original_type": "",
                                                                                                                                                          "size": 4,
                                                                                                                                                          "type": "Data"
                                                                                                                                                      },
                                                                                                                                                      {
                                                                                                                                                          "align": 4,
                                                                                                                                                          "array": false,
                                                                                                                                                          "name": "v4",
                                                                                                                                                          "native": true,
                                                                                                                                                          "original_type": "",
                                                                                                                                                          "size": 4,
                                                                                                                                                          "type": "Data"
                                                                                                                                                      },
                                                                                                                                                      {
                                                                                                                                                          "align": 4,
                                                                                                                                                          "array": false,
                                                                                                                                                          "name": "v5",
                                                                                                                                                          "native": true,
                                                                                                                                                          "original_type": "",
                                                                                                                                                          "size": 4,
                                                                                                                                                          "type": "Data"
                                                                                                                                                      },
                                                                                                                                                      {
                                                                                                                                                          "align": 4,
                                                                                                                                                          "array": false,
                                                                                                                                                          "name": "v6",
                                                                                                                                                          "native": true,
                                                                                                                                                          "original_type": "",
                                                                                                                                                          "size": 4,
                                                                                                                                                          "type": "Data"
                                                                                                                                                      }
                                                                                                                                                  ],
                                                                                                                                                  "name": "via.AnimationCurve",
                                                                                                                                                  "parent": "System.Object"
                                                                                                                                              }
                                                                                                                                          }
                                                                                                                                          """)!.KeyFromHexString();
}