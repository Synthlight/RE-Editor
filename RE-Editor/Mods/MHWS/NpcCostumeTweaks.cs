#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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

        const string almaCharDir = @"natives\STM\GameDesign\NPC\Character\Main\NPC102_00_001\Data";
        { // Alma
            const string charBaseFile = $@"{almaCharDir}\NPC102_00_001_00_VisualSetting.user.3";
            outfits.AddRange(new List<OutfitData> {
                new("Alma", "New World Commission (Replace Base)", charBaseFile, $@"{almaCharDir}\NPC102_00_001_02_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\New World Commission.jpg"),
                new("Alma", "Spring Blossom Kimono (Replace Base)", charBaseFile, $@"{almaCharDir}\NPC102_00_001_03_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\Spring Blossom Kimono.jpg"),
                new("Alma", "Summer Poncho (-Stuff, Witch Hair) (Replace Base)", charBaseFile, $@"{almaCharDir}\NPC102_00_001_04_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\Summer Poncho.jpg", SummerPonchoWithoutStuffAndWitchHair),
                new("Alma", "Summer Poncho (-Stuff, Default Hair) (Replace Base)", charBaseFile, $@"{almaCharDir}\NPC102_00_001_04_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\Summer Poncho.jpg", SummerPonchoWithoutStuffAndDefaultHair),
                new("Alma", "Autumn Witch (Replace Base)", charBaseFile, $@"{almaCharDir}\NPC102_00_001_05_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\Autumn Witch.jpg"),
                new("Alma", "Featherskirt Seikret Dress (Replace Base)", charBaseFile, $@"{almaCharDir}\NPC102_00_001_06_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\Featherskirt Seikret Dress.jpg"),
                new("Alma", "Chun-Li Outfit - SF6 (Replace Base)", charBaseFile, $@"{almaCharDir}\NPC102_00_001_07_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\Chun-Li Outfit - SF6.jpg"),
                new("Alma", "Cammy Outfit - SF6 (Replace Base)", charBaseFile, $@"{almaCharDir}\NPC102_00_001_08_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Alma\Cammy Outfit - SF6.jpg")
            });
        }

        const string gemmaCharDir = @"natives\STM\GameDesign\NPC\Character\Main\NPC102_00_010\Data\";
        { // Gemma
            const string charBaseFile = $@"{gemmaCharDir}\NPC102_00_010_VisualSetting.user.3";
            outfits.AddRange(new List<OutfitData> {
                new("Gemma", "Summer Coveralls (-Jacket) (Replace Base)", charBaseFile, $@"{gemmaCharDir}\NPC102_00_010_01_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Gemma\Summer Coveralls.jpg", SummerCoverallsWithoutJacket),
                new("Gemma", "Redveil Seikret Dress (Replace Base)", charBaseFile, $@"{gemmaCharDir}\NPC102_00_010_02_VisualSetting.user.3", $@"{PathHelper.MODS_PATH}\{name}\Gemma\Redveil Seikret Dress.jpg")
            });
        }

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var outfit in outfits) {
            mods.Add(baseMod
                     .SetName($"{outfit.target} ({outfit.name})")
                     .SetNameAsBundle($"{name} (Replace Base)")
                     .SetAdditionalFiles(new() {{outfit.baseFile, outfit.sourceFile}})
                     .SetImage(outfit.pic));
        }

        mods.AddRange(new List<NexusMod> {
            baseMod
                .SetName("Alma (Summer Poncho (-Stuff, Default Hair))")
                .SetNameAsBundle($"{name} (Modify Original)")
                .SetFiles([$@"{almaCharDir}\NPC102_00_001_04_VisualSetting.user.3"])
                .SetAction(list => { SummerPonchoWithoutStuffAndDefaultHair(list.OfType<App_user_data_NpcVisualSetting>().First()); })
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Alma\Summer Poncho.jpg"),
            baseMod
                .SetName("Gemma (Summer Coveralls (-Jacket))")
                .SetNameAsBundle($"{name} (Modify Original)")
                .SetFiles([$@"{gemmaCharDir}\NPC102_00_010_01_VisualSetting.user.3"])
                .SetAction(list => { SummerCoverallsWithoutJacket(list.OfType<App_user_data_NpcVisualSetting>().First()); })
                .SetImage($@"{PathHelper.MODS_PATH}\{name}\Gemma\Summer Coveralls.jpg")
        });

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    private static void SummerPonchoWithoutStuffAndWitchHair(App_user_data_NpcVisualSetting visualSettings) {
        foreach (var part in visualSettings.ModelData[0].ModelInfo[0].PartsList) {
            if (part.PartsNo is 2 or 9 or // Poncho
                7 or // Necklace
                8) { // Hat
                part.IsEnabled = false;
            }
        }

        visualSettings.ModelData[0].FaceInfo[0].Prefab[0].Name = "GameDesign/NPC/_Prefab/Model/HumanFace/ch00_500_0003.pfb"; // Autumn Witch head/hair.
        // These work (as Felicita), but you can't use accessories (glasses), and the skin color is wrong.
        /*
        entry.ModelData[0].FaceInfo[0].Prefab[0].Name = "GameDesign/NPC/_Prefab/Model/HumanFace/ch00_900_0001.pfb";
        entry.CharacterEditAppearance.Add(new(App_user_data_CharacterEditAppearance.HASH, entry.rsz) {
            Value = "GameDesign/NPC/Character/Main/NPC112_00_040/Data/NPC112_00_040_CharaEdit.user"
        });
        */
    }

    private static void SummerPonchoWithoutStuffAndDefaultHair(App_user_data_NpcVisualSetting visualSettings) {
        foreach (var part in visualSettings.ModelData[0].ModelInfo[0].PartsList) {
            if (part.PartsNo is 2 or 9 or // Poncho
                7 or // Necklace
                8) { // Hat
                part.IsEnabled = false;
            }
        }

        visualSettings.ModelData[0].FaceInfo[0].Prefab[0].Name = "GameDesign/NPC/_Prefab/Model/HumanFace/ch00_500_0000.pfb"; // Default head/hair.
    }

    private static void SummerCoverallsWithoutJacket(App_user_data_NpcVisualSetting visualSettings) {
        foreach (var part in visualSettings.ModelData[0].ModelInfo[0].PartsList) {
            if (part.PartsNo is 9) { // Turn off the jacket.
                part.IsEnabled = false;
            }
        }
    }

    private readonly struct OutfitData {
        public readonly string     target;
        public readonly string     name;
        public readonly string     baseFile;
        public readonly ReDataFile sourceFile;
        public readonly string     pic;

        public OutfitData(string target, string name, string baseFile, string sourceFile, string pic, Action<App_user_data_NpcVisualSetting>? tweaks = null) {
            this.target     = target;
            this.name       = name;
            this.baseFile   = baseFile;
            this.sourceFile = Load(sourceFile, tweaks);
            this.pic        = pic;
        }

        private ReDataFile Load(string sourceFile, Action<App_user_data_NpcVisualSetting>? tweaks) {
            var reDataFile = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{sourceFile}");
            var entry      = reDataFile.rsz.GetEntryObject<App_user_data_NpcVisualSetting>();
            tweaks?.Invoke(entry);
            return reDataFile;
        }
    }
}