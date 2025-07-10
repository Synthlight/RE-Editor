#nullable enable
using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class NpcOverUnknownNpc : IMod {
    public const string NAME = "NPC Over NPC (No Names)";

    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string description = "NPC over NPC options.";

        var npcData = new NpcTweaksData();

        var baseMod = new NexusMod {
            Version = NpcTweaks.VERSION,
            Desc    = description
        };

        var mods = CreateNpcOverNpcModsByDest(NpcTweaks.VERSION, baseMod, npcData);

        ModMaker.WriteMods(mainWindow, mods, NAME, copyLooseToFluffy: false, workingDir: "Q:");
    }

    public static List<NexusMod> CreateNpcOverNpcModsByDest(string version, NexusMod baseMod, NpcTweaksData npcData) {
        List<NexusMod>               mods                    = [];
        Dictionary<string, NexusMod> destinationPlaceholders = [];

        foreach (var (sourceNpcName, sourceNpcData) in npcData.npcDataByName) {
            if (!sourceNpcData.IsAllowed()) continue;
            var sourceVisualFile = sourceNpcData.rootVisualFile;
            if (sourceVisualFile == null) continue;
            var moddedVisualSource = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{sourceVisualFile}");
            if (!NpcTweaks.ChangeVisualSettings(moddedVisualSource.rsz.objectData, NpcTweaksData.IsAllowed)) continue;

            foreach (var (destNpcId, destNpcData) in npcData.npcDataByUnknownNpcId) {
                var destNpcName = npcData.nameByNpcId.GetValueOrDefault(destNpcId);
                if (sourceNpcName == destNpcName) continue;
                if (!destNpcData.IsAllowed()) continue;

                if (destNpcName != null && destNpcName != "#Rejected#" && destNpcName != "{0}") continue; // Only include unnamed NPCs.
                destNpcName = destNpcId.ToString();

                var moddedVisualSourceToUse = NpcOverNpc.GetModdedVisualSourceToUse(destNpcName, moddedVisualSource, sourceVisualFile);

                Dictionary<string, object> files = [];
                foreach (var file in destNpcData.visualSettingsData.Keys) {
                    files[file] = moddedVisualSourceToUse;
                }

                var destGroup = $"Target NPC: {destNpcName}";
                if (!destinationPlaceholders.ContainsKey(destNpcName)) {
                    destinationPlaceholders[destNpcName] = new() {
                        Name          = destGroup,
                        AddonFor      = NpcOverNpc.NAME,
                        Version       = version,
                        Desc          = NpcTweaks.PLACEHOLDER_ENTRY_TEXT,
                        Files         = [],
                        SkipPak       = true,
                        AlwaysInclude = true
                    };
                }

                mods.Add(baseMod
                         .SetName($"{sourceNpcName} Over {destNpcName}")
                         .SetAddonFor(destGroup)
                         .SetFiles([])
                         .SetAdditionalFiles(files));
            }
        }

        mods.AddRange(destinationPlaceholders.Values); // Don't forget to add the root menu placeholders.

        return mods;
    }
}