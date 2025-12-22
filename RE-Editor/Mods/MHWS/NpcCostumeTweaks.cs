using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
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
                new("Alma", "New World Commission", charBaseFile, $@"{charDir}\NPC102_00_001_02_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\New World Commission.jpg"),
                new("Alma", "Spring Blossom Kimono", charBaseFile, $@"{charDir}\NPC102_00_001_03_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\Spring Blossom Kimono.jpg"),
                new("Alma", "Summer Poncho", charBaseFile, $@"{charDir}\NPC102_00_001_04_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\Summer Poncho.jpg"),
                new("Alma", "Autumn Witch", charBaseFile, $@"{charDir}\NPC102_00_001_05_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\Autumn Witch.jpg"),
                new("Alma", "Featherskirt Seikret Dress", charBaseFile, $@"{charDir}\NPC102_00_001_06_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\Featherskirt Seikret Dress.jpg"),
                new("Alma", "Chun-Li Outfit - SF6", charBaseFile, $@"{charDir}\NPC102_00_001_07_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\Chun-Li Outfit - SF6.jpg"),
                new("Alma", "Cammy Outfit - SF6", charBaseFile, $@"{charDir}\NPC102_00_001_08_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\Cammy Outfit - SF6.jpg")
            });
        }

        { // Gemma
            const string charDir      = @"natives\STM\GameDesign\NPC\Character\Main\NPC102_00_010\Data\";
            const string charBaseFile = $@"{charDir}\NPC102_00_010_VisualSetting.user.3";
            outfits.AddRange(new List<OutfitData> {
                new("Gemma", "Summer Coveralls", charBaseFile, $@"{charDir}\NPC102_00_010_01_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Gemma\Summer Coveralls.jpg"),
                new("Gemma", "Redveil Seikret Dress", charBaseFile, $@"{charDir}\NPC102_00_010_02_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Gemma\Redveil Seikret Dress.jpg")
            });
        }

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var outfit in outfits) {
            mods.Add(baseMod
                     .SetName($"{outfit.target} ({outfit.name})")
                     .SetAdditionalFiles(new() {{outfit.baseFile, outfit.sourceFile}})
                     .SetImage(outfit.pic));
        }

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    private readonly struct OutfitData {
        public readonly string     target;
        public readonly string     name;
        public readonly string     baseFile;
        public readonly ReDataFile sourceFile;
        public readonly string     pic;

        public OutfitData(string target, string name, string baseFile, string sourceFile, string pic) {
            this.target     = target;
            this.name       = name;
            this.baseFile   = baseFile;
            this.sourceFile = Load(sourceFile);
            this.pic        = pic;
        }

        private ReDataFile Load(string sourceFile) {
            var raDataFile = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{sourceFile}");
            var entry      = raDataFile.rsz.GetEntryObject<App_user_data_NpcVisualSetting>();
            var parts      = entry.ModelData[0].ModelInfo[0].PartsList;
            foreach (var part in parts) {
                switch (target) {
                    case "Alma" when name == "Summer Poncho": {
                        if (part.PartsNo is 2 or 7 or 9) { // Turn off the poncho & necklace.
                            part.IsEnabled = false;
                        }
                        break;
                    }
                    case "Gemma" when name == "Summer Coveralls": {
                        if (part.PartsNo is 9) { // Turn off the jacket.
                            part.IsEnabled = false;
                        }
                        break;
                    }
                }
            }
            return raDataFile;
        }
    }
}