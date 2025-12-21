using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class NpcCostumeTweaks : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "NPC Costume Tweaks";
        const string description = "Change NPC base outfits to DLC ones.";
        const string version     = "1.0";

        var baseMod = new NexusMod {
            NameAsBundle = name,
            Version      = version,
            Desc         = description,
            Files        = [],
        };

        List<NexusMod>   mods    = [];
        List<OutfitData> outfits = [];

        { // Alma
            const string charDir      = @"natives\STM\GameDesign\NPC\Character\Main\NPC102_00_001\Data";
            const string charBaseFile = $@"{charDir}\NPC102_00_001_00_VisualSetting.user.3";
            outfits.AddRange(new List<OutfitData> {
                new("Alma", "New World Commission", charBaseFile, $@"{charDir}\NPC102_00_001_02_VisualSetting.user.3", @"R:\Games\Monster Hunter Wilds\Mods\NPC Costume Tweaks\Alma\New World Commission.jpg"),
                new("Alma", "Spring Blossom Kimono", charBaseFile, $@"{charDir}\NPC102_00_001_03_VisualSetting.user.3", @"R:\Games\Monster Hunter Wilds\Mods\NPC Costume Tweaks\Alma\Spring Blossom Kimono.jpg"),
                new("Alma", "Summer Poncho", charBaseFile, $@"{charDir}\NPC102_00_001_04_VisualSetting.user.3", @"R:\Games\Monster Hunter Wilds\Mods\NPC Costume Tweaks\Alma\Summer Poncho.jpg"),
                new("Alma", "Autumn Witch", charBaseFile, $@"{charDir}\NPC102_00_001_05_VisualSetting.user.3", @"R:\Games\Monster Hunter Wilds\Mods\NPC Costume Tweaks\Alma\Autumn Witch.jpg"),
                new("Alma", "Featherskirt Seikret Dress", charBaseFile, $@"{charDir}\NPC102_00_001_06_VisualSetting.user.3", @"R:\Games\Monster Hunter Wilds\Mods\NPC Costume Tweaks\Alma\Featherskirt Seikret Dress.jpg"),
                new("Alma", "Chun-Li Outfit - SF6", charBaseFile, $@"{charDir}\NPC102_00_001_07_VisualSetting.user.3", @"R:\Games\Monster Hunter Wilds\Mods\NPC Costume Tweaks\Alma\Chun-Li Outfit - SF6.jpg"),
                new("Alma", "Cammy Outfit - SF6", charBaseFile, $@"{charDir}\NPC102_00_001_08_VisualSetting.user.3", @"R:\Games\Monster Hunter Wilds\Mods\NPC Costume Tweaks\Alma\Cammy Outfit - SF6.jpg")
            });
        }

        { // Gemma
            const string charDir      = @"natives\STM\GameDesign\NPC\Character\Main\NPC102_00_010\Data\";
            const string charBaseFile = $@"{charDir}\NPC102_00_010_VisualSetting.user.3";
            outfits.AddRange(new List<OutfitData> {
                new("Gemma", "Summer Coveralls", charBaseFile, $@"{charDir}\NPC102_00_010_01_VisualSetting.user.3", @"R:\Games\Monster Hunter Wilds\Mods\NPC Costume Tweaks\Gemma\Summer Coveralls.jpg"),
                new("Gemma", "Redveil Seikret Dress", charBaseFile, $@"{charDir}\NPC102_00_010_02_VisualSetting.user.3", @"R:\Games\Monster Hunter Wilds\Mods\NPC Costume Tweaks\Gemma\Redveil Seikret Dress.jpg")
            });
        }

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var outfit in outfits) {
            mods.Add(baseMod
                     .SetName($"{outfit.target} ({outfit.name})")
                     .SetAdditionalFiles(new() {{outfit.baseFile, @$"{PathHelper.CHUNK_PATH}\{outfit.sourceFile}"}})
                     .SetImage(outfit.pic));
        }

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    private struct OutfitData(string target, string name, string baseFile, string sourceFile, string pic) {
        public readonly string target     = target;
        public readonly string name       = name;
        public readonly string baseFile   = baseFile;
        public readonly string sourceFile = sourceFile;
        public readonly string pic        = pic;
    }
}