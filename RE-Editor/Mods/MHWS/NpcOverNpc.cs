using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
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
        const string version     = "1.2";

        var baseMod = new NexusMod {
            Version = version,
            Desc    = description
        };

        var mods = CreateNpcOverNpcMods(version, baseMod, blacklist: NPC_OVERRIDES_TO_MOVE_TO_MAIN);

        ModMaker.WriteMods(mainWindow, mods, NAME, copyLooseToFluffy: false, workingDir: "Q:");
    }

    public static List<NexusMod> CreateNpcOverNpcMods(string version, NexusMod baseMod, List<string> whitelist = null, List<string> blacklist = null) {
        var npcIdsByName        = NpcTweaks.GetNpcIdsByName();
        var visualSettingsFiles = NpcTweaks.GetAllVisualSettingsFiles();
        var filesByNpcId        = NpcTweaks.GetAllFilesByNpcId(visualSettingsFiles);
        var rootVisualFileByNpc = GetRootVisualFileByNpc(filesByNpcId);
        var validNpcs           = GetValidNpcs(filesByNpcId);

        List<NexusMod>               mods                    = [];
        Dictionary<string, NexusMod> destinationPlaceholders = [];

        foreach (var (sourceNpcName, sourceNpcIds) in npcIdsByName) {
            if (sourceNpcName == NpcTweaks.UNUSED_KEY) continue;
            if (!validNpcs.Contains(sourceNpcIds[0])) continue;
            if (!rootVisualFileByNpc.TryGetValue(sourceNpcIds[0], out var sourceVisualFile)) continue;
            var moddedVisualSource = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{sourceVisualFile}");
            if (!NpcTweaks.ChangeVisualSettings(moddedVisualSource.rsz.objectData, NpcTweaks.IsAllowed)) continue;

            foreach (var (destNpcName, destNpcIds) in npcIdsByName) {
                if (destNpcName == NpcTweaks.UNUSED_KEY) continue;
                if (!validNpcs.Contains(destNpcIds[0])) continue;
                if (sourceNpcName == destNpcName) continue;
                //if (sourceNpcName != "Felicita") continue;
                //if (destNpcName != "Nata" && destNpcName != "Alma") continue;
                if (whitelist != null && !whitelist.Contains(destNpcName)) continue;
                if (blacklist != null && blacklist.Contains(destNpcName)) continue;

                var moddedVisualSourceToUse = moddedVisualSource;
                if (destNpcName == "Nata") {
                    moddedVisualSourceToUse = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{sourceVisualFile}");
                    NpcTweaks.ChangeVisualSettings(moddedVisualSourceToUse.rsz.objectData, NpcTweaks.IsAllowed);
                    var visualSettings = moddedVisualSourceToUse.rsz.GetEntryObject<App_user_data_NpcVisualSetting>();
                    visualSettings.STRUCT__OverwriteBodySize__HasValue = true;
                    visualSettings.STRUCT__OverwriteBodySize__Value    = 150;
                }

                var files = (from npcId in destNpcIds
                             where filesByNpcId.ContainsKey(npcId)
                             from destVisualFile in filesByNpcId[npcId]
                             select new KeyValuePair<string, object>(destVisualFile, moddedVisualSourceToUse)).ToDictionary(pair => pair.Key, pair => pair.Value);

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

    private static Dictionary<App_NpcDef_ID_Fixed, string> GetRootVisualFileByNpc(Dictionary<App_NpcDef_ID_Fixed, List<string>> filesByNpcId) {
        var rootVisualFileByNpc = (from pair in filesByNpcId
                                   let idName = Enum.GetName(pair.Key)
                                   orderby pair.Key
                                   from file in pair.Value
                                   where file.EndsWith($"{idName}_VisualSetting.user.3")
                                   select new KeyValuePair<App_NpcDef_ID_Fixed, string>(pair.Key, file)).ToDictionary(pair => pair.Key, pair => pair.Value);
        return rootVisualFileByNpc;
    }

    private static HashSet<App_NpcDef_ID_Fixed> GetValidNpcs(Dictionary<App_NpcDef_ID_Fixed, List<string>> filesByNpcId) {
        HashSet<App_NpcDef_ID_Fixed> validNpcs = [];
        foreach (var (npcId, files) in filesByNpcId) {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var file in files) {
                var reDataFile = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{file}");
                if (reDataFile.rsz.TryGetEntryObject<App_user_data_NpcVisualSetting>(out var entryObject)) {
                    if (NpcTweaks.IsAllowed(entryObject)) {
                        validNpcs.Add(npcId);
                    }
                }
            }
        }
        return validNpcs;
    }
}