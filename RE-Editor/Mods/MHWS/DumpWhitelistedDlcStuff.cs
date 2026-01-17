using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

// ReSharper disable once InconsistentNaming
public class DumpWhitelistedDlcStuff : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        var dlcProductData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\DLC\UserData\DlcProductIdList.user.3").rsz.GetEntryObject<App_user_data_DlcProductIdList>().Values.Cast<App_user_data_DlcProductIdList_cData>().ToList();

        foreach (var dlc in dlcProductData.OrderBy(data => data.Name_)) {
            if (dlc.Name_.StartsWith("Alma Outfit")
                || dlc.Name_.StartsWith("Erik Outfit")
                || dlc.Name_.StartsWith("Erik Outfit")
                || dlc.Name_.StartsWith("Felyne Layered")
                || dlc.Name_.StartsWith("Gemma Outfit")
                || dlc.Name_.StartsWith("Hairstyle")
                || dlc.Name_.StartsWith("Hunter Layered Armor")
                || dlc.Name_.StartsWith("Layered")
                || dlc.Name_.StartsWith("Seikret Decoration")) {
                var enumName = dlc.ID_Unwrapped.ToString();
                Debug.WriteLine($"LOG(\"Unlocking {dlc.Name_}\"); dlcCache[{enumName}] = true;");
            }
        }
    }
}