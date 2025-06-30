#nullable enable
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
public class NpcOverNpc : IMod {
    public const string NAME = "NPC Over NPC";

    public static readonly List<string> NPC_OVERRIDES_TO_MOVE_TO_MAIN = [
        "Alma",
        "Nata"
    ];

    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string description = "NPC over NPC options.";
        const string version     = "1.4";

        var npcData = new NpcTweaksData();

        var baseMod = new NexusMod {
            Version = version,
            Desc    = description
        };

        var mods = CreateNpcOverNpcModsByDest(version, baseMod, npcData, blacklist: NPC_OVERRIDES_TO_MOVE_TO_MAIN);

        ModMaker.WriteMods(mainWindow, mods, NAME, copyLooseToFluffy: false, workingDir: "Q:");
    }

    public static List<NexusMod> CreateNpcOverNpcModsBySource(string version, NexusMod baseMod, NpcTweaksData npcData, List<string>? whitelist = null, List<string>? blacklist = null) {
        List<NexusMod> mods = [];

        foreach (var (sourceNpcName, sourceNpcData) in npcData.npcDataByName) {
            if (!sourceNpcData.IsAllowed()) continue;
            var sourceVisualFile = sourceNpcData.rootVisualFile;
            if (sourceVisualFile == null) continue;
            var moddedVisualSource = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{sourceVisualFile}");
            if (!NpcTweaks.ChangeVisualSettings(moddedVisualSource.rsz.objectData, NpcTweaksData.IsAllowed)) continue;

            var bySourceName = $"Source NPC: {sourceNpcName}";
            if (whitelist == null || !whitelist.Contains(sourceNpcName)) continue;

            mods.Add(new() {
                Name          = bySourceName,
                AddonFor      = NAME,
                Version       = version,
                Desc          = NpcTweaks.PLACEHOLDER_ENTRY_TEXT,
                Files         = [],
                SkipPak       = true,
                AlwaysInclude = true
            });

            // Named NPCs.
            List<NexusMod> npcSpecificMods = [];
            foreach (var (destNpcName, destNpcData) in npcData.npcDataByName) {
                if (sourceNpcName == destNpcName) continue;
                if (!destNpcData.IsAllowed()) continue;

                var moddedVisualSourceToUse = GetModdedVisualSourceToUse(destNpcName, moddedVisualSource, sourceVisualFile);

                Dictionary<string, object> files = [];
                foreach (var file in destNpcData.visualSettingsData.Keys) {
                    files[file] = moddedVisualSourceToUse;
                }

                var mod = baseMod
                          .SetName($"{sourceNpcName} Over {destNpcName} (2)")
                          .SetAddonFor(bySourceName)
                          .SetFiles([])
                          .SetAdditionalFiles(files);
                mods.Add(mod);
                npcSpecificMods.Add(mod);
            }

            // Unnamed NPCs.
            {
                Dictionary<string, object> files = [];
                foreach (var (file, data) in npcData.unnamedData.visualSettingsData) {
                    var visualSettings = data.rsz.GetEntryObject<App_user_data_NpcVisualSetting>();
                    if (NpcTweaksData.IsAllowed(visualSettings)) {
                        files[file] = moddedVisualSource; // Don't have a name, so we aren't correcting anything.
                    }
                }

                mods.Add(baseMod
                         .SetName($"_{sourceNpcName} Over Unnamed NPCs")
                         .SetAddonFor(bySourceName)
                         .SetFiles([])
                         .SetAdditionalFiles(files));
            }

            // All in section.
            {
                Dictionary<string, object> files = [];
                foreach (var mod in npcSpecificMods) {
                    foreach (var (file, data) in mod.AdditionalFiles!) {
                        files[file] = data;
                    }
                }

                mods.Add(baseMod
                         .SetName($"_{sourceNpcName} Over All Except Unnamed")
                         .SetAddonFor(bySourceName)
                         .SetFiles([])
                         .SetAdditionalFiles(files));
            }
        }

        return mods;
    }

    public static List<NexusMod> CreateNpcOverNpcModsByDest(string version, NexusMod baseMod, NpcTweaksData npcData, List<string>? whitelist = null, List<string>? blacklist = null) {
        List<NexusMod>               mods                    = [];
        Dictionary<string, NexusMod> destinationPlaceholders = [];

        foreach (var (sourceNpcName, sourceNpcData) in npcData.npcDataByName) {
            if (!sourceNpcData.IsAllowed()) continue;
            var sourceVisualFile = sourceNpcData.rootVisualFile;
            if (sourceVisualFile == null) continue;
            var moddedVisualSource = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{sourceVisualFile}");
            if (!NpcTweaks.ChangeVisualSettings(moddedVisualSource.rsz.objectData, NpcTweaksData.IsAllowed)) continue;

            foreach (var (destNpcName, destNpcData) in npcData.npcDataByName) {
                if (sourceNpcName == destNpcName) continue;
                if (!destNpcData.IsAllowed()) continue;

                if (whitelist != null && !whitelist.Contains(destNpcName)) continue;
                if (blacklist != null && blacklist.Contains(destNpcName)) continue;

                var moddedVisualSourceToUse = GetModdedVisualSourceToUse(destNpcName, moddedVisualSource, sourceVisualFile);

                Dictionary<string, object> files = [];
                foreach (var file in destNpcData.visualSettingsData.Keys) {
                    files[file] = moddedVisualSourceToUse;
                }

                var destGroup = $"Target NPC: {destNpcName}";
                if (!destinationPlaceholders.ContainsKey(destNpcName)) {
                    destinationPlaceholders[destNpcName] = new() {
                        Name          = destGroup,
                        AddonFor      = NAME,
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

    private static ReDataFile GetModdedVisualSourceToUse(string destNpcName, ReDataFile baseModdedVisualSource, string sourceVisualFile) {
        if (destNpcName == "Nata") {
            var moddedVisualSourceToUse = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{sourceVisualFile}");
            NpcTweaks.ChangeVisualSettings(moddedVisualSourceToUse.rsz.objectData, NpcTweaksData.IsAllowed);
            var visualSettings = moddedVisualSourceToUse.rsz.GetEntryObject<App_user_data_NpcVisualSetting>();
            visualSettings.STRUCT__OverwriteBodySize__HasValue = true;
            visualSettings.STRUCT__OverwriteBodySize__Value    = 150;
            return moddedVisualSourceToUse;
        }
        return baseModdedVisualSource;
    }
}