using System.Diagnostics.CodeAnalysis;
using RE_Editor.Common;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Models;
using RE_Editor.Common.Structs;

namespace RE_Editor.Generator.Models;

public class StructType(string name, string? parent, string hash, StructJson structInfo) {
    public readonly string     name       = name;
    public readonly string?    parent     = parent;
    public readonly string     hash       = hash;
    public readonly StructJson structInfo = structInfo;
    public          int        useCount;

    public void UpdateUsingCounts(GenerateFiles generator, List<string> history) {
        if (history.Contains(structInfo.name!)) return;
        history.Add(structInfo.name!);

        if (parent != null) {
            generator.structTypes[parent].useCount++;
            generator.structTypes[parent].UpdateUsingCounts(generator, history);
        }

        if (useCount > 0 && parent?.ToConvertedTypeName() == "Ace_Bitset") {
            var enumType = structInfo.GetGenericParam()?.ToConvertedTypeName()!;
            var enumData = generator.enumTypes[enumType];
            enumData.useCount++;
        }

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var field in structInfo.fields!) {
            if (string.IsNullOrEmpty(field.name)) continue;

            if (parent != null) {
                // Can't be null as the json includes parent's fields in children.
                if (generator.structTypes[parent].structInfo.fieldNameMap.TryGetValue(field.name, out var parentField)) {
                    parentField.virtualCount++;
                    field.overrideCount++;
                }
            }

            if (string.IsNullOrEmpty(field.originalType)) continue;
            if (GenerateFiles.UNSUPPORTED_DATA_TYPES.Contains(field.type!)) continue;
            if (GenerateFiles.UNSUPPORTED_OBJECT_TYPES.Any(s => field.originalType!.Contains(s))) continue;
            var typeName = field.originalType?.ToConvertedTypeName();
            if (typeName == null) continue;
            if (field.originalType!.GetViaType() != null) continue;

            if (field.twoGenericsInfo != null || (field.originalType!.IndexOf('`') > -1 && field.originalType!.Substring(field.originalType!.IndexOf('`'), 3) == "`2<")) {
                field.twoGenericsInfo ??= new(field);
                var twoGenericsInfo = field.twoGenericsInfo.Value;
                UpdateCountsForType(generator, history, twoGenericsInfo.type1.convertedName);
                UpdateCountsForType(generator, history, twoGenericsInfo.type2.convertedName);
            }
            UpdateCountsForType(generator, history, typeName);
        }
    }

    private static void UpdateCountsForType(GenerateFiles generator, List<string> history, string typeName) {
        if (generator.structTypes.TryGetValue(typeName, out var fieldType)) {
            fieldType.useCount++;
            fieldType.UpdateUsingCounts(generator, history);
        }
        if (generator.enumTypes.TryGetValue(typeName, out var enumType)) {
            enumType.useCount++;
        }
#if MHWS
        if (typeName.EndsWith("_Serializable")) {
            generator.enumTypes[typeName.ToFixedEnumName()!].useCount++;
        }
#endif
    }

    public void UpdateButtons(GenerateFiles generator, List<string> history) {
        if (history.Contains(structInfo.name!)) return;
        history.Add(structInfo.name!);

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var field in structInfo.fields!) {
            if (string.IsNullOrEmpty(field.name)) continue;
            if (GenerateFiles.UNSUPPORTED_DATA_TYPES.Contains(field.type!)) continue;
            if (GenerateFiles.UNSUPPORTED_OBJECT_TYPES.Any(s => field.originalType!.Contains(s))) continue;

            if (parent != null) {
                generator.structTypes[parent].UpdateButtons(generator, history);
            }
            if (parent != null && generator.structTypes[parent].structInfo.fieldNameMap.TryGetValue(field.name, out var parentField)) {
                field.buttonType      = parentField.buttonType;
                field.buttonPrimitive = parentField.buttonPrimitive;
            } else {
                field.buttonType      = GetButtonType(generator, field);
                field.buttonPrimitive = GetButtonPrimitive(field.buttonType);
            }
        }
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private DataSourceType? GetButtonType(GenerateFiles generator, StructJson.Field field) {
        // This part check the class + field name.
        var fullName = $"{name}.{field.name.ToConvertedFieldName()}";
#pragma warning disable IDE0066
#pragma warning disable CS1522
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (fullName) {
#if DD2
            // Many of these don't seem to be the enum type, probably because the enum doesn't allow zero but the fields do.
            case "App_EnhanceParamBase.ItemId":
            case "App_EnhanceParamBase.NeedItemId0":
            case "App_EnhanceParamBase.NeedItemId1":
            case "App_Gm80_001Param_ItemParam.ItemId":
            case "App_ItemCommonParam.ItemDropId":
            case "App_ItemDataParam.DecayedItemId":
            case "App_ItemDropParam_Table_Item.Id":
            case "App_ItemShopParamBase.ItemId":
                return DataSourceType.ITEMS;
#elif DRDR
            case "App_solid_gamemastering_rItemLayout__LayoutInfo.ITEM_NO":
            case "App_solid_gamemastering_rItemLayout__LayoutInfo.CHANGE_ITEM_NO":
            case "Solid_MT2RE_rItemAttackTable__CommonAttackParam.MItemNo":
                return DataSourceType.ITEMS;
#elif MHWS
            case "App_ArmorDef_SERIES_Serializable.Value": return DataSourceType.ARMOR_SERIES;
            case "App_HunterDef_Skill_Serializable.Value": return DataSourceType.SKILLS;
            case "App_HunterProfileDef_MEDAL_ID_Serializable.Value": return DataSourceType.MEDALS;
#endif
        }
#pragma warning restore CS1522
#pragma warning restore IDE0066

        var originalType = field.originalType;
        if (originalType == null) return null;

#if MHWS
        if (originalType.Contains("_Serializable") && field is {array: false, type: nameof(Object)} && generator.structTypes[field.originalType!.ToConvertedTypeName()!].structInfo.fields![0].type != nameof(Object)) {
            originalType = originalType.ToFixedEnumName()!;
        }
#endif

        // And this check the original type.
        return originalType.Replace("[]", "") switch {
#if DD2
            "app.ItemIDEnum" => DataSourceType.ITEMS,
#elif DRDR
            "app.MTData.ITEM_NO" => DataSourceType.ITEMS,
#elif MHR
            "snow.data.ContentsIdSystem.ItemId" => DataSourceType.ITEMS,
            "snow.data.DataDef.PlEquipSkillId" => DataSourceType.SKILLS,
            "snow.data.DataDef.PlHyakuryuSkillId" => DataSourceType.RAMPAGE_SKILLS,
            "snow.data.DataDef.PlKitchenSkillId" => DataSourceType.DANGO_SKILLS,
            "snow.data.DataDef.PlWeaponActionId" => DataSourceType.SWITCH_SKILLS,
#elif MHWS
            "app.ArmorDef.SERIES_Fixed" => DataSourceType.ARMOR_SERIES,
            "app.EnemyDef.ID_Fixed" => DataSourceType.ENEMIES,
            "app.HunterDef.Skill_Fixed" => DataSourceType.SKILLS,
            "app.HunterProfileDef.MEDAL_ID_Fixed" => DataSourceType.MEDALS,
            "app.ItemDef.ID_Fixed" => DataSourceType.ITEMS,
#elif RE2
            "app.ropeway.gamemastering.Item.ID" => DataSourceType.ITEMS,
            "app.ropeway.EquipmentDefine.WeaponType" => DataSourceType.WEAPONS,
#elif RE3
            "offline.EquipmentDefine.WeaponType" => DataSourceType.WEAPONS,
            "offline.gamemastering.Item.ID" => DataSourceType.ITEMS,
#elif RE4
            "chainsaw.ItemID" => DataSourceType.ITEMS,
            "chainsaw.WeaponID" => DataSourceType.WEAPONS,
#endif
            _ => null
        };
    }

    private static string? GetButtonPrimitive(DataSourceType? fieldButtonType) {
        return fieldButtonType switch {
#if MHWS
            DataSourceType.ARMOR_SERIES => "int",
            DataSourceType.ENEMIES => "int",
            DataSourceType.MEDALS => "int",
            DataSourceType.SKILLS => "int",
#endif
            null => null,
            _ => "uint"
        };
    }

    public void UpdateFields(GenerateFiles generator, List<string> history) {
        if (history.Contains(structInfo.name!)) return;
        history.Add(structInfo.name!);

        if (parent != null) {
            var parentType = generator.structTypes[parent!];
            parentType.UpdateFields(generator, history);

            // Make our types match parent field types.
            var matchingFields = from field in structInfo.fields!
                                 from parentField in parentType.structInfo.fields!
                                 where field.name == parentField.name
                                 select new KeyValuePair<StructJson.Field, StructJson.Field>(field, parentField);

            foreach (var (field, parentField) in matchingFields) {
                field.type         = parentField.type;
                field.originalType = parentField.originalType;
            }
        }

        if (structInfo.fields != null) {
            var hasStructFields = false;
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var field in structInfo.fields) {
                if (field.type == "Struct") {
                    // Flag for later as we can't do it here without triggering a CME.
                    hasStructFields = true;
                }
                if (string.IsNullOrEmpty(field.originalType) && structInfo.name?.StartsWith("via") == true) {
                    switch (field.type) {
                        case "Data":
                            if (field.size == 4) {
                                field.type         = "F32";
                                field.originalType = "System.Single";

                                if (structInfo.name == "via.AnimationCurve" && field.name == "v6") {
                                    field.type         = "U32";
                                    field.originalType = "System.UInt32";
                                }
                            } else {
                                field.type         = nameof(UIntArray);
                                field.originalType = nameof(UIntArray);
                            }
                            break;
                        case "String":
                            field.originalType = "System.String";
                            break;
                    }
                    if (structInfo.name == "via.physics.RequestSetColliderUserData" && field.name == "v1") {
                        field.type         = "Object";
                        field.originalType = "via.physics.UserData";
                    }
                }
            }
            // For these, to make it simple, we just copy and expand the struct as if it was just done as fields in our type.
            List<StructJson.Field> newStructFields = [];
            if (hasStructFields) {
                foreach (var field in structInfo.fields) {
                    // Skip if not a struct. Also skip if it's a struct array because these are embedded and need to be read in-place. Can't flatten arrays.
                    if (field.type != "Struct" || field.array) {
                        newStructFields.Add(field);
                        continue;
                    }
                    var originalFieldName = field.name.ToConvertedFieldName()!;
                    var structTypeName    = field.originalType.ToConvertedTypeName()!;
                    if (!generator.structTypes.TryGetValue(structTypeName, out var structType)) {
                        Global.Log($"Warning: Struct {structInfo.name} has a struct field with a type of {field.originalType} but it can't be found in the struct map.");
                        continue;
                    }
                    foreach (var structField in structType.structInfo.fields!) {
                        var structFieldName = structField.name.ToConvertedFieldName()!;
                        var newName         = $"{originalFieldName}_{structFieldName}";
                        var newField        = structField.Copy();
                        newField.name = newName;
                        newStructFields.Add(newField);
                    }
                }
                structInfo.fields = newStructFields;
            }
        }
    }
}