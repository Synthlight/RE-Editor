using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class MeldAllDecorations : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name           = "Meld All Decorations";
        const string description    = "Adds all decorations to the \"Meld Decorations\" list, with various config options.";
        const string version        = "1.0";
        const bool   includeReSorts = false;

        var decoData        = ReDataFile.Read(PathHelper.CHUNK_PATH + PathHelper.DECORATION_DATA_PATH).rsz.objectData.OfType<App_user_data_AccessoryData_cData>().ToList();
        var meldingListFile = ReDataFile.Read(PathHelper.CHUNK_PATH + PathHelper.FACILITY_MELD_DECORATION_DATA_PATH);
        var meldingData     = meldingListFile.rsz.GetEntryObject<App_user_data_MakaAccessoryData>();

        Dictionary<App_EquipDef_ACCESSORY_ID_Fixed, MeldingCost> meldingList  = [];
        Dictionary<App_EquipDef_ACCESSORY_ID_Fixed, Guid>        decoIdToGuid = [];

        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
        foreach (App_user_data_MakaAccessoryData_cData entry in meldingData.Values) {
            if (!meldingList.TryGetValue((App_EquipDef_ACCESSORY_ID_Fixed) entry.AccessoryId, out var data)) {
                meldingList[(App_EquipDef_ACCESSORY_ID_Fixed) entry.AccessoryId] = data = new();
            }
            data.cost      = entry.NeedPoint;
            data.storyFlag = entry.StoryPackage;
        }
        foreach (var deco in decoData) {
            var decoName = DataHelper.DECORATION_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng][deco.Name.Value];
            decoIdToGuid[deco.AccessoryId_Unwrapped] = deco.Name.Value;

            if (!meldingList.ContainsKey(deco.AccessoryId_Unwrapped)) {
                int                             cost;
                App_StoryPackageFlag_TYPE_Fixed storyFlag;

                if (decoName.Contains("[1]")) {
                    cost      = 90;
                    storyFlag = App_StoryPackageFlag_TYPE_Fixed.STORY_000488;
                } else if (decoName.Contains("[2]")) {
                    cost      = 450;
                    storyFlag = App_StoryPackageFlag_TYPE_Fixed.STORY_000451;
                } else if (decoName.Contains("[3]")) {
                    cost      = decoName.Contains('/') ? 2000 : 1000;
                    storyFlag = App_StoryPackageFlag_TYPE_Fixed.STORY_000451;
                } else {
                    throw new("Unable to determine deco cost.");
                }

                meldingList[deco.AccessoryId_Unwrapped] = new() {
                    cost      = cost,
                    storyFlag = storyFlag
                };
            }
        }

        Dictionary<Global.LangIndex, Dictionary<App_EquipDef_ACCESSORY_ID_Fixed, string>> langToDecoToName = [];
        foreach (var lang in Global.LANGUAGES) {
            var map = langToDecoToName[lang] = [];
            foreach (var (id, guid) in decoIdToGuid) {
                map[id] = DataHelper.DECORATION_INFO_LOOKUP_BY_GUID[lang][guid];
            }
        }

        meldingList = meldingList.Sort(pair => langToDecoToName[Global.LangIndex.eng][pair.Key]);

        var index = 0;
        meldingData.Values.Clear();
        foreach (var (id, cost) in meldingList) {
            var data = App_user_data_MakaAccessoryData_cData.Create(meldingData.rsz);
            data._Index       = index++;
            data.AccessoryId  = (int) id;
            data.NeedPoint    = cost.cost;
            data.StoryPackage = cost.storyFlag;
            meldingData.Values.Add(data);
        }

        var baseMod = new NexusMod {
            Version = version,
            Desc    = description,
            Image   = $@"{PathHelper.MODS_PATH}\{name}\Page 1.png"
        };

        var baseLuaMod = new VariousDataTweak {
            Version = version,
            Desc    = description,
            Image   = $@"{PathHelper.MODS_PATH}\{name}\Page 1.png"
        };

        const string baseName = $"{name} (Base)";

        var mods = new List<INexusMod> {
            baseMod
                .SetName(baseName)
                .SetFiles([])
                .SetAdditionalFiles(new() {{PathHelper.FACILITY_MELD_DECORATION_DATA_PATH, meldingListFile}}),
            baseLuaMod
                .SetName($"{name} - Set Cost to 1 (REF)")
                .SetDesc("Changes the price of melding anything to just 1 point.")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.FACILITY_MELD_DECORATION_DATA,
                        Action = Cost1Ref
                    }
                ])
                .SetAddonFor(baseName)
                .SetSkipPak(true),
            baseLuaMod
                .SetName($"{name} - Set Cost to Half (REF)")
                .SetDesc("Changes the price of melding anything to one half the points.")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.FACILITY_MELD_DECORATION_DATA,
                        Action = Cost1RefHalf
                    }
                ])
                .SetAddonFor(baseName)
                .SetSkipPak(true),
            baseLuaMod
                .SetName($"{name} - Set Cost to Quarter (REF)")
                .SetDesc("Changes the price of melding anything to one quarter the points.")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.FACILITY_MELD_DECORATION_DATA,
                        Action = Cost1RefQuarter
                    }
                ])
                .SetAddonFor(baseName)
                .SetSkipPak(true),
            baseLuaMod
                .SetName($"{name} - Unlock All Decos Once You Can Meld (REF)")
                .SetDesc("Changes the story flags to it all unlocks the first time you get access to melding decos like this, in the story.")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.FACILITY_MELD_DECORATION_DATA,
                        Action = UnlockEarlyRef
                    }
                ])
                .SetAddonFor(baseName)
                .SetSkipPak(true)
        };

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        // ReSharper disable once HeuristicUnreachableCode
#pragma warning disable CS0162 // Unreachable code detected
        if (includeReSorts) {
            const string langBaseName = $"{name} - Re-sort Options for Other Languages";

            mods.Add(new NexusMod {
                Name          = langBaseName,
                Version       = version,
                Desc          = NpcTweaks.PLACEHOLDER_ENTRY_TEXT,
                Files         = [],
                AddonFor      = baseName,
                SkipPak       = true,
                AlwaysInclude = true
            });

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var lang in Global.LANGUAGES) {
                if (lang == Global.LangIndex.eng) continue;

                var sortedList = (from entry in meldingList.Keys
                                  orderby langToDecoToName[lang][entry]
                                  select entry).ToList();

                mods.Add(baseLuaMod
                         .SetName($"{name} - Re-sort for {lang} (REF)")
                         .SetDesc("Re-sorts the list to to deco name by the target language.")
                         .SetDefaultLuaName()
                         .SetChanges([
                             new() {
                                 Target = VariousDataTweak.Target.FACILITY_MELD_DECORATION_DATA,
                                 Action = writer => ReSortRef(writer, sortedList)
                             }
                         ])
                         .SetAddonFor(langBaseName)
                         .SetSkipPak(true));
            }
        }
#pragma warning restore CS0162 // Unreachable code detected

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void Cost1Ref(StreamWriter writer) {
        writer.WriteLine("    entry._NeedPoint = 1");
        writer.WriteLine("    entry:call(\"onLoad\")");
    }

    public static void Cost1RefHalf(StreamWriter writer) {
        writer.WriteLine("    entry._NeedPoint = entry._NeedPoint / 2");
        writer.WriteLine("    entry:call(\"onLoad\")");
    }

    public static void Cost1RefQuarter(StreamWriter writer) {
        writer.WriteLine("    entry._NeedPoint = entry._NeedPoint / 4");
        writer.WriteLine("    entry:call(\"onLoad\")");
    }

    public static void UnlockEarlyRef(StreamWriter writer) {
        writer.WriteLine($"    entry._StoryPackage = {((int) App_StoryPackageFlag_TYPE_Fixed.STORY_000488)}");
        writer.WriteLine("    entry:call(\"onLoad\")");
    }

    public static void ReSortRef(StreamWriter writer, List<App_EquipDef_ACCESSORY_ID_Fixed> sortedList) {
        for (var i = 0; i < sortedList.Count; i++) {
            writer.WriteLine($"    if entry._AccessoryId == {((int) sortedList[i])} then");
            writer.WriteLine($"        entry._Index = {i}");
            writer.WriteLine("        entry:call(\"onLoad\")");
            writer.WriteLine($"    end");
        }
    }

    private class MeldingCost {
        public int                             cost;
        public App_StoryPackageFlag_TYPE_Fixed storyFlag;
    }
}