using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Constants;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Mods.MHWS;
using RE_Editor.Util;

namespace RE_Editor.Mods;

/**
 * Rules for MHW Bingo:
 *
 * No pop up camp manager allowed
 * No traders allowed
 * No limited bounties allowed
 * No Palico
 * No Support Ship Shop
 * Only High Rank Hunts allowed
 * Square's referring to monsters are all large monsters
 * Talismans and Mantles are banned
 * No supply items
 * Investigations and Field surveys are not allowed
 *
 *
 *
 * Current feature list:
 *  - Monsters drop:
 *    - Gold cert
 *    - Silver cert
 *    - Hard (red) armor sphere
 *    - Monster specific cert, or a mega potion if there isn't one.
 *    - They'll always appear in this order, so if something is giving you a MP when it should be giving you some cert, let me know.
 *   - Recipes are set so:
 *    - Weapons/armor/weapon upgrades are all 10k.
 *    - Weapons/armor use a monster cert, or a gold cert for the rest.
 *    - Kinsects craft/upgrade with one silver cert.
 *    - Hope/expedition/ore and other non-monster tree weapons require 3 golden certs to upgrade.
 *     - Exceptions:
 *      - "Paralysis Tree" changed to Lala B. (It uses Lala parts for most of it, just doesn't have the mon in the name.)
 *
 *  To do:
 *  - Make carves mega potions (small & large), remove certs from part breaks (not sure if you can get any that way).
 */
[UsedImplicitly]
public class BingoBrawlers : IMod {
    [UsedImplicitly]
    public static void Make() {
        const string name        = "Bingo Brawlers";
        const string description = "Bingo Brawlers stuff.";
        const string version     = "1.0";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description
        };

        /*
        @"\natives\STM\GameDesign\Mission\_UserData\_Reward\AddRewardData.user.3",
        @"\natives\STM\GameDesign\Mission\_UserData\_Reward\CommonRewardData.user.3",
        @"\natives\STM\GameDesign\Mission\_UserData\_Reward\QuestRewardSetting.user.3",
        @"\natives\STM\GameDesign\Mission\_UserData\_Reward\QuestRewardVariousData.user.3",
        @"\natives\STM\GameDesign\Mission\_UserData\_Reward\RewardLotProbabilityTable.user.3",
        @"\natives\STM\GameDesign\Mission\_UserData\_Reward\RewardNumTable.user.3",
        */

        Dictionary<string, Dictionary<int, App_WeaponDef_SERIES_Fixed>> weaponToRecipeIdToSeriesIdMap  = [];
        List<int>                                                       recipeIdsOfLastUpgradeInSeries = [];

        foreach (var weaponType in Global.WEAPON_TYPES) {
            var recipePath = @$"\natives\STM\GameDesign\Common\Weapon\{weaponType}Recipe.user.3";
            var recipeData = ReDataFile.Read(PathHelper.CHUNK_PATH + recipePath).rsz.objectData.OfType<App_user_data_WeaponRecipeData_cData>().ToList();
            var treePath   = @$"\natives\STM\GameDesign\Common\Weapon\{weaponType}Tree.user.3";
            var treeData   = ReDataFile.Read(PathHelper.CHUNK_PATH + treePath).rsz.GetEntryObject<App_user_data_WeaponTree>();

            // Build a list of recipe IDs to weapon series IDs.
            var recipeIdToSeriesId = (from recipe in recipeData
                                      from tree in treeData.WeaponTreeList
                                      where recipe.GetUsedEnumIdValue() == tree.WeaponID
                                      let rowData = treeData.RowDataList.First(row => row.RowLevel == tree.RowDataLevel)
                                      let seriesIdName = Enum.GetName(rowData.Series)
                                      let seriesIdFixed = Enum.Parse<App_WeaponDef_SERIES_Fixed>(seriesIdName)
                                      select new {recipe.RecipeNo, seriesIdFixed}).ToDictionary(a => a.RecipeNo, a => a.seriesIdFixed);
            weaponToRecipeIdToSeriesIdMap[weaponType] = recipeIdToSeriesId;

            recipeIdsOfLastUpgradeInSeries = (from tree in treeData.WeaponTreeList
                                              orderby tree.ColumnDataLevel, tree.RowDataLevel
                                              group tree by tree.RowDataLevel
                                              into g
                                              from weapon in g.TakeLast(1) // Get only the last entry, the final upgrade.
                                              from recipe in recipeData
                                              where recipe.GetUsedEnumIdValue() == weapon.WeaponID
                                              select recipe.RecipeNo).ToList();
        }

        var mods = new List<INexusMod> {
            baseMod
                .SetName($"{name} - Recipe Changes")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Recipe)
                                    .Append(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Base))
                                    .Append(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Tree))
                                    .Append([
                                        PathHelper.ARMOR_RECIPE_DATA_PATH,
                                        PathHelper.ARMOR_SERIES_DATA_PATH,
                                        PathHelper.KINSECT_RECIPE_DATA_PATH,
                                    ]))
                .SetAction(list => ModRecipes(list, weaponToRecipeIdToSeriesIdMap, recipeIdsOfLastUpgradeInSeries)),
            baseMod
                .SetName($"{name} - Drop Changes")
                .SetFiles([])
                .SetAdditionalFiles(new() {
                    {@"reframework\autorun\BB_Drop_Changes.lua", ModResources.BingoBrawlers_Drop_Changes}
                })
                .SetSkipPak(true),
        };

        ModMaker.WriteMods(mods, name, copyLooseToFluffy: true, noPakZip: true);
    }

    public static void ModRecipes(List<RszObject> rszObjectData, Dictionary<string, Dictionary<int, App_WeaponDef_SERIES_Fixed>> weaponToRecipeIdToSeriesIdMap, List<int> recipeIdsOfLastUpgradeInSeries) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_ArmorSeriesData_cData armor: // Armor
                    armor.Price = 10000;
                    break;
                case App_user_data_ArmorRecipeData_cData armorRecipe: // Armor Recipe
                    armorRecipe.KeyEnemyId     = (int) App_EnemyDef_ID_Fixed.INVALID;
                    armorRecipe.KeyItemId      = (int) App_ItemDef_ID_Fixed.NONE;
                    armorRecipe.KeyStoryNo     = App_MissionIDList_ID_Fixed.INVALID;
                    armorRecipe.FlagHunterRank = 0;
                    foreach (var item in armorRecipe.Item) {
                        item.Value = (int) App_ItemDef_ID_Fixed.NONE;
                    }
                    foreach (var itemNum in armorRecipe.ItemNum) {
                        itemNum.Value = 0;
                    }
                    var requiredArmorItem = GetRequiredItemBy((App_ArmorDef_SERIES_Fixed) armorRecipe.SeriesId);
                    if (requiredArmorItem != App_ItemDef_ID_Fixed.INVALID) {
                        armorRecipe.Item[0].Value    = (int) requiredArmorItem;
                        armorRecipe.ItemNum[0].Value = 1;
                    }
                    break;
                case App_user_data_RodInsectRecipeData_cData kinsectRecipe: // Insect Recipe
                    kinsectRecipe.KeyEnemyId     = (int) App_EnemyDef_ID_Fixed.INVALID;
                    kinsectRecipe.KeyItemId      = (int) App_ItemDef_ID_Fixed.NONE;
                    kinsectRecipe.KeyStoryNo     = App_MissionIDList_ID_Fixed.INVALID;
                    kinsectRecipe.FlagHunterRank = 0;
                    foreach (var item in kinsectRecipe.ItemId) {
                        item.Value = (int) App_ItemDef_ID_Fixed.NONE;
                    }
                    foreach (var itemNum in kinsectRecipe.ItemNum) {
                        itemNum.Value = 0;
                    }
                    kinsectRecipe.ItemId[0].Value  = (int) ItemConstants.SILVER_MELDING_TICKET;
                    kinsectRecipe.ItemNum[0].Value = 1;
                    break;
                case App_user_data_WeaponData_cData weapon: // Weapon
                    weapon.Price = 10000;
                    break;
                case App_user_data_WeaponRecipeData_cData weaponRecipe: // Weapon Recipe
                    weaponRecipe.KeyEnemyId     = (int) App_EnemyDef_ID_Fixed.INVALID;
                    weaponRecipe.KeyItemId      = (int) App_ItemDef_ID_Fixed.NONE;
                    weaponRecipe.KeyStoryNo     = App_MissionIDList_ID_Fixed.INVALID;
                    weaponRecipe.FlagHunterRank = 0;
                    foreach (var item in weaponRecipe.Item) {
                        item.Value = (int) App_ItemDef_ID_Fixed.NONE;
                    }
                    foreach (var itemNum in weaponRecipe.ItemNum) {
                        itemNum.Value = 0;
                    }
                    weaponRecipe.CanShortcut = false;

                    var weaponType         = weaponRecipe.GetWeaponType();
                    var seriesId           = weaponToRecipeIdToSeriesIdMap[weaponType][weaponRecipe.RecipeNo];
                    var requiredWeaponItem = GetRequiredItemBy(seriesId);
                    if (requiredWeaponItem != App_ItemDef_ID_Fixed.INVALID) {
                        weaponRecipe.Item[0].Value    = (int) requiredWeaponItem;
                        weaponRecipe.ItemNum[0].Value = 1;
                        if (requiredWeaponItem == ItemConstants.GOLD_MELDING_TICKET && recipeIdsOfLastUpgradeInSeries.Contains(weaponRecipe.RecipeNo)) {
                            weaponRecipe.ItemNum[0].Value = 3;
                        }
                    }
                    break;
                case App_user_data_WeaponTree tree: // Weapon Tree
                    foreach (var columnData in tree.ColumnDataList) {
                        // This won't matter much since we're moving everything to the first two rows.
                        columnData.StoryFlag = App_MissionIDList_ID.INVALID;
                    }
                    var lastTwoByRow = (from entry in tree.WeaponTreeList
                                        orderby entry.ColumnDataLevel, entry.RowDataLevel
                                        group entry by entry.RowDataLevel
                                        into g
                                        select g).ToDictionary(group => group.Key, group => group.TakeLast(2).ToList());

                    var newEntries = new List<App_user_data_WeaponTree_cWeaponTree>(lastTwoByRow.Count * 2);
                    foreach (var (_, lastTwo) in lastTwoByRow) {
                        lastTwo[0].PreDataGuidList  = [];
                        lastTwo[0].NextDataGuidList = [lastTwo[1].Guid.Value];
                        lastTwo[0].ColumnDataLevel  = 0;
                        lastTwo[1].PreDataGuidList  = [lastTwo[0].Guid.Value];
                        lastTwo[1].NextDataGuidList = [];
                        lastTwo[1].ColumnDataLevel  = 1;
                        newEntries.AddRange(lastTwo);
                    }
                    tree.WeaponTreeList = new(newEntries);
                    break;
            }
        }
    }

    public static void ModDrops(List<RszObject> rszObjectData) {
        const uint commonRewardTableId         = 8000u;
        const uint rewardLotProbabilityTableId = 8000u;

        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_QuestGeneralRewardData rewardData:
                    if (rewardData.Values.Count < 10) { // Assume it's the 'Add' reward data file.
                        foreach (var entry in rewardData.Values) {
                            var reward = (App_user_data_QuestGeneralRewardData_cData) entry;
                            if (reward.TableId == 100) {
                                reward.AddLotType_Unwrapped = App_RewardDef_REWARD_ADD_LOT_TYPE_Fixed.REM_ITEM_ALL_GET_3;
                            }
                        }
                    } else {
                        var dataId    = 8000u;
                        var data      = rewardData.Values.Cast<App_user_data_QuestGeneralRewardData_cData>().ToList();
                        var lastIndex = data.Max(entry => entry._Index);

                        data.Add(MakeGeneralRewardData(rewardData.rsz, data, ref dataId, ref lastIndex, commonRewardTableId, ItemConstants.GOLD_MELDING_TICKET, 1, 100));
                        data.Add(MakeGeneralRewardData(rewardData.rsz, data, ref dataId, ref lastIndex, commonRewardTableId, ItemConstants.SILVER_MELDING_TICKET, 1, 100));
                        data.Add(MakeGeneralRewardData(rewardData.rsz, data, ref dataId, ref lastIndex, commonRewardTableId, ItemConstants.HEAVY_ARMOR_SPHERE, 1, 100));
                        data.Add(MakeGeneralRewardData(rewardData.rsz, data, ref dataId, ref lastIndex, commonRewardTableId, App_ItemDef_ID_Fixed.NONE, 0, 0));
                        data.Add(MakeGeneralRewardData(rewardData.rsz, data, ref dataId, ref lastIndex, commonRewardTableId, App_ItemDef_ID_Fixed.NONE, 0, 0));
                        data.Add(MakeGeneralRewardData(rewardData.rsz, data, ref dataId, ref lastIndex, commonRewardTableId, App_ItemDef_ID_Fixed.NONE, 0, 0));
                        data.Add(MakeGeneralRewardData(rewardData.rsz, data, ref dataId, ref lastIndex, commonRewardTableId, App_ItemDef_ID_Fixed.NONE, 0, 0));
                        data.Add(MakeGeneralRewardData(rewardData.rsz, data, ref dataId, ref lastIndex, commonRewardTableId, App_ItemDef_ID_Fixed.NONE, 0, 0));
                        data.Add(MakeGeneralRewardData(rewardData.rsz, data, ref dataId, ref lastIndex, commonRewardTableId, App_ItemDef_ID_Fixed.NONE, 0, 0));
                        rewardData.Values = [.. data];
                    }
                    break;
                case App_user_data_QuestRewardSetting_cData questRewardData:
                    questRewardData.CommonRewardTableId    = commonRewardTableId;
                    questRewardData.TargetAddRewardTableId = 100; // Should be random potions.
                    break;
                case App_user_data_QuestRewardVariousData questRewardVariousData:
                    questRewardVariousData.QuestCommonRewardLotProbabilityTableId = rewardLotProbabilityTableId;
                    questRewardVariousData.QuestCommonRewardConfirmNum            = 3;
                    break;
                case App_user_data_RewardLotProbabilityTable rewardLotProbabilityData:
                    var newLot = App_user_data_RewardLotProbabilityTable_cData.Create(rewardLotProbabilityData.rsz);
                    newLot.TableId     = rewardLotProbabilityTableId;
                    newLot.DataId      = rewardLotProbabilityTableId;
                    newLot.AddLotNum   = 1;
                    newLot.Probability = 100;
                    rewardLotProbabilityData.Values.Add(newLot);
                    break;
                case App_user_data_RewardNumTableData rewardNumTableData:
                    var newNumData = App_user_data_RewardNumTableData_cData.Create(rewardNumTableData.rsz);
                    newNumData.TargetNum   = 1;
                    newNumData.ConfirmNum  = 3;
                    newNumData.LotTableNum = rewardLotProbabilityTableId;
                    rewardNumTableData.Values.Add(newNumData);
                    break;
            }
        }
    }

    private static App_user_data_QuestGeneralRewardData_cData MakeGeneralRewardData(RSZ rsz, List<App_user_data_QuestGeneralRewardData_cData> data, ref uint dataId, ref int lastIndex, uint tableId, App_ItemDef_ID_Fixed itemId, short quantity, int probability) {
        var entry = App_user_data_QuestGeneralRewardData_cData.Create(rsz);
        entry._Index                  = lastIndex++;
        entry.DataId                  = dataId++;
        entry.TableId                 = tableId;
        entry.AddLotType_Unwrapped    = App_RewardDef_REWARD_ADD_LOT_TYPE_Fixed.REM_ITEM_ALL_GET_3;
        entry.CommonLotType_Unwrapped = App_RewardDef_REWARD_COMMON_LOT_TYPE_Fixed.REM_ITEM_ALL_GET;
        entry.ItemId_Unwrapped        = itemId;
        entry.Num                     = quantity;
        entry.Probability             = probability;
        return entry;
    }

    private static App_ItemDef_ID_Fixed GetRequiredItemBy(App_ArmorDef_SERIES_Fixed seriesId) {
        switch (seriesId) {
            case ArmorSeriesConstants.AJARAKAN:
            case ArmorSeriesConstants.AJARAKAN_Α:
            case ArmorSeriesConstants.AJARAKAN_Β:
                return ItemConstants.AJARAKAN_CERTIFICATE_S;
            case ArmorSeriesConstants.ARKVELD_Α:
            case ArmorSeriesConstants.ARKVELD_Β:
            case ArmorSeriesConstants.GUARDIAN_ARKVELD: // Not re-fight-able, so just use the regular ticket for now.
            case ArmorSeriesConstants.GUARDIAN_ARKVELD_Α:
            case ArmorSeriesConstants.GUARDIAN_ARKVELD_Β:
                return ItemConstants.ARKVELD_CERTIFICATE_S;
            case ArmorSeriesConstants.BALAHARA:
            case ArmorSeriesConstants.BALAHARA_Α:
            case ArmorSeriesConstants.BALAHARA_Β:
                return ItemConstants.BALAHARA_CERTIFICATE_S;
            case ArmorSeriesConstants.BLANGO_Α:
            case ArmorSeriesConstants.BLANGO_Β:
                return ItemConstants.BLANGONGA_CERTIFICATE_S;
            case ArmorSeriesConstants.CHATACABRA:
            case ArmorSeriesConstants.CHATACABRA_Α:
            case ArmorSeriesConstants.CHATACABRA_Β:
                return ItemConstants.CHATACABRA_CERTIFICATE_S;
            case ArmorSeriesConstants.CONGA:
            case ArmorSeriesConstants.CONGA_Α:
            case ArmorSeriesConstants.CONGA_Β:
                return ItemConstants.CONGALALA_CERTIFICATE_S;
            case ArmorSeriesConstants.DOSHAGUMA:
            case ArmorSeriesConstants.DOSHAGUMA_Α:
            case ArmorSeriesConstants.DOSHAGUMA_Β:
                return ItemConstants.DOSHAGUMA_CERTIFICATE_S;
            case ArmorSeriesConstants.GUARDIAN_DOSHAGUMA:
            case ArmorSeriesConstants.GUARDIAN_DOSHAGUMA_Α:
            case ArmorSeriesConstants.GUARDIAN_DOSHAGUMA_Β:
                return ItemConstants.G_DOSHAGUMA_CERTIFICATE_S;
            case ArmorSeriesConstants.GUARDIAN_EBONY:
            case ArmorSeriesConstants.GUARDIAN_EBONY_Α:
            case ArmorSeriesConstants.GUARDIAN_EBONY_Β:
                return ItemConstants.G_EBONY_ODOGARON_CERTIFICATE_S;
            case ArmorSeriesConstants.GUARDIAN_FULGUR_Α:
            case ArmorSeriesConstants.GUARDIAN_FULGUR_Β:
                return ItemConstants.GUARDIAN_FULGUR_CERTIFICATE_S;
            case ArmorSeriesConstants.GUARDIAN_RATHALOS:
            case ArmorSeriesConstants.GUARDIAN_RATHALOS_Α:
            case ArmorSeriesConstants.GUARDIAN_RATHALOS_Β:
                return ItemConstants.G_RATHALOS_CERTIFICATE_S;
            case ArmorSeriesConstants.GORE_Α:
            case ArmorSeriesConstants.GORE_Β:
                return ItemConstants.GORE_MAGALA_CERTIFICATE_S;
            case ArmorSeriesConstants.GRAVIOS_Α:
            case ArmorSeriesConstants.GRAVIOS_Β:
                return ItemConstants.GRAVIOS_CERTIFICATE_S;
            case ArmorSeriesConstants.GYPCEROS_Α:
            case ArmorSeriesConstants.GYPCEROS_Β:
                return ItemConstants.GYPCEROS_CERTIFICATE_S;
            case ArmorSeriesConstants.HIRABAMI:
            case ArmorSeriesConstants.HIRABAMI_Α:
            case ArmorSeriesConstants.HIRABAMI_Β:
                return ItemConstants.HIRABAMI_CERTIFICATE_S;
            case ArmorSeriesConstants.DAHAAD_Α:
            case ArmorSeriesConstants.DAHAAD_Β:
                return ItemConstants.JIN_DAHAAD_CERTIFICATE_S;
            case ArmorSeriesConstants.LALA_BARINA:
            case ArmorSeriesConstants.LALA_BARINA_Α:
            case ArmorSeriesConstants.LALA_BARINA_Β:
                return ItemConstants.LALA_BARINA_CERTIFICATE_S;
            case ArmorSeriesConstants.NERSCYLLA:
            case ArmorSeriesConstants.NERSCYLLA_Α:
            case ArmorSeriesConstants.NERSCYLLA_Β:
                return ItemConstants.NERSCYLLA_CERTIFICATE_S;
            case ArmorSeriesConstants.NU_UDRA:
            case ArmorSeriesConstants.NU_UDRA_Α:
            case ArmorSeriesConstants.NU_UDRA_Β:
                return ItemConstants.NU_UDRA_CERTIFICATE_S;
            case ArmorSeriesConstants.QUEMATRICE:
            case ArmorSeriesConstants.QUEMATRICE_Α:
            case ArmorSeriesConstants.QUEMATRICE_Β:
                return ItemConstants.QUEMATRICE_CERTIFICATE_S;
            case ArmorSeriesConstants.RATHALOS_Α:
            case ArmorSeriesConstants.RATHALOS_Β:
                return ItemConstants.RATHALOS_CERTIFICATE_S;
            case ArmorSeriesConstants.RATHIAN_Α:
            case ArmorSeriesConstants.RATHIAN_Β:
                return ItemConstants.RATHIAN_CERTIFICATE_S;
            case ArmorSeriesConstants.REY_DAU:
            case ArmorSeriesConstants.REY_DAU_Α:
            case ArmorSeriesConstants.REY_DAU_Β:
                return ItemConstants.REY_DAU_CERTIFICATE_S;
            case ArmorSeriesConstants.ROMPOPOLO:
            case ArmorSeriesConstants.ROMPOPOLO_Α:
            case ArmorSeriesConstants.ROMPOPOLO_Β:
                return ItemConstants.ROMPOPOLO_CERTIFICATE_S;
            case ArmorSeriesConstants.UTH_DUNA:
            case ArmorSeriesConstants.UTH_DUNA_Α:
            case ArmorSeriesConstants.UTH_DUNA_Β:
                return ItemConstants.UTH_DUNA_CERTIFICATE_S;
            case ArmorSeriesConstants.XU_WU:
            case ArmorSeriesConstants.XU_WU_Α:
            case ArmorSeriesConstants.XU_WU_Β:
                return ItemConstants.XU_WU_CERTIFICATE_S;
            case ArmorSeriesConstants.KUT_KU_Α:
            case ArmorSeriesConstants.KUT_KU_Β:
                return ItemConstants.YIAN_KUT_KU_CERTIFICATE_S;
            // Generic:
            // And whilst `default` would cover it, I'm copying the others as I find them to make it easier to identify.
            case ArmorSeriesConstants.ALLOY:
            case ArmorSeriesConstants.ALLOY_Α:
            case ArmorSeriesConstants.ARTIAN_Α:
            case ArmorSeriesConstants.AZUZ_Α:
            case ArmorSeriesConstants.BATTLE_Α:
            case ArmorSeriesConstants.BONE:
            case ArmorSeriesConstants.BONE_Α:
            case ArmorSeriesConstants.BUTTERFLY_Α:
            case ArmorSeriesConstants.CHAINMAIL:
            case ArmorSeriesConstants.CHAINMAIL_Α:
            case ArmorSeriesConstants.COMAQCHI:
            case ArmorSeriesConstants.COMAQCHI_Α:
            case ArmorSeriesConstants.COMAQCHI_Β:
            case ArmorSeriesConstants.COMMISSION_Α:
            case ArmorSeriesConstants.DAMASCUS_Α:
            case ArmorSeriesConstants.DEATH_STENCH_Α:
            case ArmorSeriesConstants.DOBER_Α:
            case ArmorSeriesConstants.DRAGONKING_Α:
            case ArmorSeriesConstants.EXPEDITION_HEADGEAR_Α:
            case ArmorSeriesConstants.FENCERS_EYEPATCH:
            case ArmorSeriesConstants.FEUDAL_SOLDIER:
            case ArmorSeriesConstants.GAJAU:
            case ArmorSeriesConstants.GAJAU_Α:
            case ArmorSeriesConstants.GUILD_ACE_Α:
            case ArmorSeriesConstants.GUILD_KNIGHT:
            case ArmorSeriesConstants.HIGH_METAL_Α:
            case ArmorSeriesConstants.INGOT:
            case ArmorSeriesConstants.INGOT_Α:
            case ArmorSeriesConstants.KING_BEETLE_Α:
            case ArmorSeriesConstants.KRANODATH:
            case ArmorSeriesConstants.KRANODATH_Α:
            case ArmorSeriesConstants.KRANODATH_Β:
            case ArmorSeriesConstants.LEATHER:
            case ArmorSeriesConstants.LEATHER_Α:
            case ArmorSeriesConstants.MELAHOA_Α:
            case ArmorSeriesConstants.MIMIPHYTA_Α:
            case ArmorSeriesConstants.ONI_HORNS_WIG:
            case ArmorSeriesConstants.PIRAGILL:
            case ArmorSeriesConstants.PIRAGILL_Α:
            case ArmorSeriesConstants.PIRAGILL_Β:
            case ArmorSeriesConstants.SILD_Α:
            case ArmorSeriesConstants.SUJA_Α:
            case ArmorSeriesConstants.TALIOTH:
            case ArmorSeriesConstants.TALIOTH_Α:
            case ArmorSeriesConstants.TALIOTH_Β:
            case ArmorSeriesConstants.VESPOID:
            case ArmorSeriesConstants.VESPOID_Α:
            case ArmorSeriesConstants.VESPOID_Β:
            case ArmorSeriesConstants.WYVERIAN_EARS:
            default:
                return ItemConstants.GOLD_MELDING_TICKET;
            case App_ArmorDef_SERIES_Fixed.NONE:
            case App_ArmorDef_SERIES_Fixed.MAX:
                throw new ArgumentOutOfRangeException(nameof(seriesId));
        }
    }

    private static App_ItemDef_ID_Fixed GetRequiredItemBy(App_WeaponDef_SERIES_Fixed seriesId) {
        switch (seriesId) {
            case WeaponSeriesConstants.AJARAKAN_TREE:
                return ItemConstants.AJARAKAN_CERTIFICATE_S;
            case WeaponSeriesConstants.ARKVELD_TREE:
            case WeaponSeriesConstants.G_ARKVELD_TREE: // Not re-fight-able, so just use the regular ticket for now.
                return ItemConstants.ARKVELD_CERTIFICATE_S;
            case WeaponSeriesConstants.BALAHARA_TREE:
                return ItemConstants.BALAHARA_CERTIFICATE_S;
            case WeaponSeriesConstants.BLANGONGA_TREE:
                return ItemConstants.BLANGONGA_CERTIFICATE_S;
            case WeaponSeriesConstants.CHATACABRA_TREE:
                return ItemConstants.CHATACABRA_CERTIFICATE_S;
            case WeaponSeriesConstants.CONGALALA_TREE:
                return ItemConstants.CONGALALA_CERTIFICATE_S;
            case WeaponSeriesConstants.DOSHAGUMA_TREE:
                return ItemConstants.DOSHAGUMA_CERTIFICATE_S;
            case WeaponSeriesConstants.G_DOSHAGUMA_TREE:
                return ItemConstants.G_DOSHAGUMA_CERTIFICATE_S;
            case WeaponSeriesConstants.G_EBONY_TREE:
                return ItemConstants.G_EBONY_ODOGARON_CERTIFICATE_S;
            case WeaponSeriesConstants.G_FULGUR_TREE:
                return ItemConstants.GUARDIAN_FULGUR_CERTIFICATE_S;
            case WeaponSeriesConstants.G_RATHALOS_TREE:
                return ItemConstants.G_RATHALOS_CERTIFICATE_S;
            case WeaponSeriesConstants.GORE_MAGALA_TREE:
                return ItemConstants.GORE_MAGALA_CERTIFICATE_S;
            case WeaponSeriesConstants.GRAVIOS_TREE:
                return ItemConstants.GRAVIOS_CERTIFICATE_S;
            case WeaponSeriesConstants.GYPCEROS_TREE:
                return ItemConstants.GYPCEROS_CERTIFICATE_S;
            case WeaponSeriesConstants.HIRABAMI_TREE:
                return ItemConstants.HIRABAMI_CERTIFICATE_S;
            case WeaponSeriesConstants.JIN_DAHAAD_TREE:
                return ItemConstants.JIN_DAHAAD_CERTIFICATE_S;
            case WeaponSeriesConstants.LALA_BARINA_TREE:
            case WeaponSeriesConstants.PARALYSIS_TREE: // It's made with Lala B. parts.
                return ItemConstants.LALA_BARINA_CERTIFICATE_S;
            case WeaponSeriesConstants.NERSCYLLA_TREE:
                return ItemConstants.NERSCYLLA_CERTIFICATE_S;
            case WeaponSeriesConstants.NU_UDRA_TREE:
                return ItemConstants.NU_UDRA_CERTIFICATE_S;
            case WeaponSeriesConstants.QUEMATRICE_TREE:
                return ItemConstants.QUEMATRICE_CERTIFICATE_S;
            case WeaponSeriesConstants.RATHALOS_TREE:
                return ItemConstants.RATHALOS_CERTIFICATE_S;
            case WeaponSeriesConstants.RATHIAN_TREE:
                return ItemConstants.RATHIAN_CERTIFICATE_S;
            case WeaponSeriesConstants.REY_DAU_TREE:
                return ItemConstants.REY_DAU_CERTIFICATE_S;
            case WeaponSeriesConstants.ROMPOPOLO_TREE:
                return ItemConstants.ROMPOPOLO_CERTIFICATE_S;
            case WeaponSeriesConstants.UTH_DUNA_TREE:
                return ItemConstants.UTH_DUNA_CERTIFICATE_S;
            case WeaponSeriesConstants.XU_WU_TREE:
                return ItemConstants.XU_WU_CERTIFICATE_S;
            case WeaponSeriesConstants.YIAN_KUT_KU_TREE:
                return ItemConstants.YIAN_KUT_KU_CERTIFICATE_S;
            // Generic:
            // And whilst `default` would cover it, I'm copying the others as I find them to make it easier to identify.
            case WeaponSeriesConstants.BONE_TREE:
            case WeaponSeriesConstants.EXPEDITION_TREE:
            case WeaponSeriesConstants.ORE_TREE:
            case WeaponSeriesConstants.SPEARTUNA_TREE:
            case WeaponSeriesConstants.VESPOID_TREE:
            case WeaponSeriesConstants.WATER_ELEMENT_TREE:
            case WeaponSeriesConstants.WORKSHOP_TREE:
            default:
                return ItemConstants.GOLD_MELDING_TICKET;
            case App_WeaponDef_SERIES_Fixed.NONE:
            case App_WeaponDef_SERIES_Fixed.MAX:
                throw new ArgumentOutOfRangeException(nameof(seriesId));
        }
    }
}