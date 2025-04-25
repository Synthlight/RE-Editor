using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
[SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
[SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
[SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
public class DumpData : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        DumpDecayData();
        DumpGimmickData();
        DumpStyleToMeshData();
    }

    private static void DumpDecayData() {
        var dataFile = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{PathHelper.ITEM_DATA_PATH}");
        var data     = dataFile.rsz.GetEntryObject<App_ItemData>();

        var decayItemsWhichAreBad = new Dictionary<uint, string>();
        var itemsWithBadDecay     = new Dictionary<uint, string>();
        foreach (var itemData in data.Params) {
            var itemName      = DataHelper.ITEM_NAME_LOOKUP[Global.LangIndex.eng].TryGet((uint) itemData.Id, "");
            var decayItemName = DataHelper.ITEM_NAME_LOOKUP[Global.LangIndex.eng].TryGet((uint) itemData.DecayedItemId, "");
            if (decayItemName.StartsWith("Rotten") || decayItemName.StartsWith("Dried")) {
                decayItemsWhichAreBad.TryAdd((uint) itemData.DecayedItemId, decayItemName);
                itemsWithBadDecay.TryAdd((uint) itemData.Id, itemName);
            }
        }
        File.WriteAllText($@"{PathHelper.MODS_PATH}\..\decayItemsWhichAreBad.json", JsonConvert.SerializeObject(decayItemsWhichAreBad, Formatting.Indented));
        File.WriteAllText($@"{PathHelper.MODS_PATH}\..\itemsWithBadDecay.json", JsonConvert.SerializeObject(itemsWithBadDecay, Formatting.Indented));
    }

    private static void DumpGimmickData() {
        var chestContents = new Dictionary<int, List<string>>();
        var dataFile      = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\natives/STM/AppSystem/Gimmick/UserData/Gm80_001_GmData.user.2");
        foreach (var obj in dataFile.rsz.objectData) {
            if (obj is App_Gm80_001Param param) {
                var items = param.ItemList;
                if (items.Count == 0) continue;

                var paramId = param.ParamId;
                if (!chestContents.ContainsKey(paramId)) {
                    chestContents[paramId] = [];
                }

                foreach (var item in items) {
                    var itemId   = item.ItemId;
                    var itemText = $"{itemId}::{DataHelper.ITEM_NAME_LOOKUP[Global.LangIndex.eng].TryGet((uint) itemId)}";
                    if (!chestContents[paramId].Contains(itemText)) {
                        chestContents[paramId].Add(itemText);
                    }
                }
            }
        }
        File.WriteAllText($@"{PathHelper.MODS_PATH}\..\Gm80_001_GmData.json", JsonConvert.SerializeObject(chestContents, Formatting.Indented));
    }
    private static void DumpStyleToMeshData() {
        /*
        var armorData      = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{PathHelper.ARMOR_DATA_PATH}").rsz.GetEntryObject<App_ItemArmorData>();
        var swapHelmData   = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{PathHelper.SWAP_DATA_TOPS_PATH}").rsz.GetEntryObject<App_HelmSwapData>();
        var swapMantleData = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{PathHelper.SWAP_DATA_TOPS_PATH}").rsz.GetEntryObject<App_MantleSwapData>();
        var swapTopData    = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{PathHelper.SWAP_DATA_TOPS_PATH}").rsz.GetEntryObject<App_TopsSwapData>();
        var swapPantsData  = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{PathHelper.SWAP_DATA_TOPS_PATH}").rsz.GetEntryObject<App_PantsSwapData>();

        var decayItemsWhichAreBad = new Dictionary<uint, string>();
        var itemsWithBadDecay     = new Dictionary<uint, string>();
        foreach (var itemData in data.Params) {
            var itemName      = DataHelper.ITEM_NAME_LOOKUP[Global.LangIndex.eng].TryGet((uint) itemData.Id, "");
            var decayItemName = DataHelper.ITEM_NAME_LOOKUP[Global.LangIndex.eng].TryGet((uint) itemData.DecayedItemId, "");
            if (decayItemName.StartsWith("Rotten") || decayItemName.StartsWith("Dried")) {
                decayItemsWhichAreBad.TryAdd((uint) itemData.DecayedItemId, decayItemName);
                itemsWithBadDecay.TryAdd((uint) itemData.Id, itemName);
            }
        }
        File.WriteAllText($@"{PathHelper.MODS_PATH}\..\decayItemsWhichAreBad.json", JsonConvert.SerializeObject(decayItemsWhichAreBad, Formatting.Indented));
        File.WriteAllText($@"{PathHelper.MODS_PATH}\..\itemsWithBadDecay.json", JsonConvert.SerializeObject(itemsWithBadDecay, Formatting.Indented));
        */
    }
}