using System.Collections.Generic;
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

        List<string> skipped = [];

        foreach (var dlc in dlcProductData.OrderBy(data => data.Name_)) {
            var enumName = dlc.ID_Unwrapped.ToString();
            if (dlc.Name_.StartsWith("31 ICE CREAM")
                || dlc.Name_.StartsWith("Alma Outfit")
                || dlc.Name_.StartsWith("BGM")
                || dlc.Name_.StartsWith("Camp Gear")
                || dlc.Name_.StartsWith("Erik Outfit")
                || dlc.Name_.StartsWith("Erik Outfit")
                || dlc.Name_.StartsWith("Felyne Layered")
                || dlc.Name_.StartsWith("Gemma Outfit")
                || dlc.Name_.StartsWith("Gesture")
                || dlc.Name_.StartsWith("Hairstyle")
                || dlc.Name_.StartsWith("Hunter Layered Armor")
                || dlc.Name_.StartsWith("KFC Collab")
                || dlc.Name_.StartsWith("Layered")
                || dlc.Name_.StartsWith("Makeup")
                || dlc.Name_.StartsWith("MH Championship")
                || dlc.Name_.StartsWith("MH Festa")
                || dlc.Name_.StartsWith("Nameplate")
                || dlc.Name_.StartsWith("Pendant")
                || dlc.Name_.StartsWith("Photo")
                || dlc.Name_.StartsWith("Quest Completion")
                || dlc.Name_.StartsWith("Seikret Decoration")
                || dlc.Name_.StartsWith("Sticker")
                || dlc.Name_.StartsWith("Talisman")
                || dlc.Name_.StartsWith("Tent Customization")
                // Packs that the above includes.
                || dlc.Name_.StartsWith("Blossomdance DLC Pack")
                || dlc.Name_.StartsWith("Dreamspell DLC Pack")
                || dlc.Name_.StartsWith("Flamefete DLC Pack")
                || dlc.Name_.StartsWith("Lumenhymn DLC Pack")
                || dlc.Name_.StartsWith("Monster Hunter Wilds Cosmetic DLC")) {
                Debug.WriteLine($"LOG(\"Unlocking {dlc.Name_}\"); dlcCache[{enumName}] = true;");
            } else {
                skipped.Add(dlc.Name_);
            }
        }

        foreach (var dlcName in skipped) {
            Debug.WriteLine($"Skipped: {dlcName}");
        }
    }
}