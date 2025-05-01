#nullable enable
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
public class NpcTweaks : IMod {
    private const string MASTER_NPC_EQUIP_PATH_FEMALE          = @"natives\STM\GameDesign\NPC\Data\Master_Female_NpcHunterEquipData.user.3";
    private const string MASTER_NPC_EQUIP_PATH_MALE            = @"natives\STM\GameDesign\NPC\Data\Master_Male_NpcHunterEquipData.user.3";
    private const string MASTER_NPC_EQUIP_PATH_INTERNAL_FEMALE = @"GameDesign/NPC/Data/Master_Female_NpcHunterEquipData.user";
    private const string MASTER_NPC_EQUIP_PATH_INTERNAL_MALE   = @"GameDesign/NPC/Data/Master_Male_NpcHunterEquipData.user";
    public const  string PLACEHOLDER_ENTRY_TEXT                = "Activating this entry does nothing, it exists solely to create the submenu.";

    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "NPC Outfit Tweaks";
        const string description = "NPC outfit tweaks - Makes all NPCs wear a given outfit.";
        const string version     = "1.2";

        const string coreName = "Core (MUST ACTIVATE ONE FOR EACH GENDER USED!)";

        var npcData = new NpcTweaksData();

        var baseMod = new NexusMod {
            Version = version,
            Desc    = description
        };

        var mods = new List<NexusMod> {
            new() {
                Name          = name,
                Version       = version,
                Desc          = PLACEHOLDER_ENTRY_TEXT,
                Files         = [],
                SkipPak       = true,
                AlwaysInclude = true,
                Image         = $@"{PathHelper.MODS_PATH}\{name}\1.png"
            },
            new() {
                Name          = coreName,
                AddonFor      = name,
                Version       = version,
                Desc          = PLACEHOLDER_ENTRY_TEXT,
                Files         = [],
                SkipPak       = true,
                AlwaysInclude = true
            }
        };

        foreach (var innerwearOption in Enum.GetValues<NpcTweaksData.InnerwearOption>()) {
            mods.Add(baseMod.SetName($"Core (Female): Innerwear {(int) innerwearOption}                      ,")
                            .SetFiles([])
                            .SetAddonFor(coreName)
                            .SetAdditionalFiles(new() {{MASTER_NPC_EQUIP_PATH_FEMALE, NpcTweaksData.CreateCleanHunterEquipFile(innerwearOption, true)}}));
            mods.Add(baseMod.SetName($"Core (Female): Innerwear {(int) innerwearOption} (No Slinger)")
                            .SetFiles([])
                            .SetAddonFor(coreName)
                            .SetAdditionalFiles(new() {{MASTER_NPC_EQUIP_PATH_FEMALE, NpcTweaksData.CreateCleanHunterEquipFile(innerwearOption, false)}}));
            mods.Add(baseMod.SetName($"Core (Male): Innerwear {(int) innerwearOption}                      ,")
                            .SetFiles([])
                            .SetAddonFor(coreName)
                            .SetAdditionalFiles(new() {{MASTER_NPC_EQUIP_PATH_MALE, NpcTweaksData.CreateCleanHunterEquipFile(innerwearOption, true)}}));
            mods.Add(baseMod.SetName($"Core (Male): Innerwear {(int) innerwearOption} (No Slinger)")
                            .SetFiles([])
                            .SetAddonFor(coreName)
                            .SetAdditionalFiles(new() {{MASTER_NPC_EQUIP_PATH_MALE, NpcTweaksData.CreateCleanHunterEquipFile(innerwearOption, false)}}));
        }

        // ReSharper disable MergeIntoPattern
        mods.AddRange([
            baseMod
                .SetName("Female NPCs (Human, Excluding Skin Issues)")
                .SetAddonFor(name)
                .SetFiles(npcData.allExcludingSkinIssues)
                .SetFilteredAction(list => ChangeVisualSettings(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.FEMALE
                                                                                        && visualSettings.ParamPackOwCategory is App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_BC
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST101
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST103
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST105)),
            baseMod
                .SetName("Female NPCs (Human, All)")
                .SetAddonFor(name)
                .SetFiles(npcData.visualSettingsFiles)
                .SetFilteredAction(list => ChangeVisualSettings(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.FEMALE
                                                                                        && visualSettings.ParamPackOwCategory is App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_BC
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST101
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST103
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST105)),
            baseMod
                .SetName("Female NPCs (Wyverian)")
                .SetAddonFor(name)
                .SetFiles(npcData.visualSettingsFiles)
                .SetFilteredAction(list => ChangeVisualSettings(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.FEMALE
                                                                                        && visualSettings.ParamPackOwCategory == App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_FEMALE)),
            baseMod
                .SetName("Male NPCs (Human, Excluding Skin Issues)")
                .SetAddonFor(name)
                .SetFiles(npcData.allExcludingSkinIssues)
                .SetFilteredAction(list => ChangeVisualSettings(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.MALE
                                                                                        && visualSettings.ParamPackOwCategory is App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_BC
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST101
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST103
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST105)),
            baseMod
                .SetName("Male NPCs (Human, All)")
                .SetAddonFor(name)
                .SetFiles(npcData.visualSettingsFiles)
                .SetFilteredAction(list => ChangeVisualSettings(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.MALE
                                                                                        && visualSettings.ParamPackOwCategory is App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_BC
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST101
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST103
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST105)),
            baseMod
                .SetName("Male NPCs (Wyverian)")
                .SetAddonFor(name)
                .SetFiles(npcData.visualSettingsFiles)
                .SetFilteredAction(list => ChangeVisualSettings(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.MALE
                                                                                        && visualSettings.ParamPackOwCategory == App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_MALE))
        ]);
        // ReSharper restore MergeIntoPattern

        // Build the whole list of individual options.
        const string byNameGroup = "Individually, by Name";
        mods.Add(new() {
            Name          = byNameGroup,
            AddonFor      = name,
            Version       = version,
            Desc          = PLACEHOLDER_ENTRY_TEXT,
            Files         = [],
            SkipPak       = true,
            AlwaysInclude = true
        });

        foreach (var (npcName, visualData) in npcData.npcDataByName) {
            if (!visualData.IsAllowed()) continue;
            var idNames = string.Join(", ", from id in visualData.ids
                                            let idName = Enum.GetName(id)
                                            select idName);
            List<string> files = [];
            foreach (var (file, data) in visualData.visualSettingsData) {
                var visualSettings = data.rsz.GetEntryObject<App_user_data_NpcVisualSetting>();
                if (NpcTweaksData.IsAllowed(visualSettings)) {
                    files.Add(file);
                }
            }
            if (files.Count == 0) {
                Console.WriteLine($"Warning: Unable to retrieve files for: {npcName}, {idNames}");
                continue;
            }
            mods.Add(baseMod
                     .SetName($"{npcName}: ({idNames})")
                     .SetAddonFor(byNameGroup)
                     .SetFiles(files)
                     .SetFilteredAction(list => ChangeVisualSettings(list, NpcTweaksData.IsAllowed))); // Filtered again, yes, but it should always return true.
        }

        List<string> unnamedFiles = [];
        foreach (var (file, data) in npcData.unnamedData.visualSettingsData) {
            var visualSettings = data.rsz.GetEntryObject<App_user_data_NpcVisualSetting>();
            if (NpcTweaksData.IsAllowed(visualSettings)) {
                unnamedFiles.Add(file);
            }
        }

        mods.Add(baseMod
                 .SetName("_Unnamed NPCs: (All The Rest)")
                 .SetAddonFor(byNameGroup)
                 .SetFiles(unnamedFiles)
                 .SetFilteredAction(list => ChangeVisualSettings(list, NpcTweaksData.IsAllowed))); // Filtered again, yes, but it should always return true.

        // The optional file will still use this mod as a base, so include this here.
        mods.Add(new() {
            Name          = NpcOverNpc.NAME,
            AddonFor      = "NPC Outfit Tweaks",
            Version       = version,
            Desc          = PLACEHOLDER_ENTRY_TEXT,
            Files         = [],
            SkipPak       = true,
            AlwaysInclude = true
        });

        // Include just the few most care about.
        var npcOverNpcMods = NpcOverNpc.CreateNpcOverNpcModsByDest(version, baseMod, npcData, whitelist: NpcOverNpc.NPC_OVERRIDES_TO_MOVE_TO_MAIN);
        mods.AddRange(npcOverNpcMods);

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true, workingDir: "Q:");

        var bySourceNpcMods = NpcOverNpc.CreateNpcOverNpcModsBySource(version, baseMod, npcData, whitelist: [
            "Felicita"
        ]);

        ModMaker.WriteMods(mainWindow, bySourceNpcMods, $"{name} - By Source", copyLooseToFluffy: true, workingDir: "Q:");
    }

    public static bool ChangeVisualSettings(IList<RszObject> rszObjectData, Func<App_user_data_NpcVisualSetting, bool> predicate) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_NpcVisualSetting visualSettings:
                    if (!predicate(visualSettings)) return false;
                    var isFemale = visualSettings.Gender == App_CharacterDef_GENDER.FEMALE;

                    foreach (var part in visualSettings.ModelData[0].ModelInfo[0].PartsList) {
                        part.IsEnabled = false; // Keeps the prefab parts off.
                    }
                    if (visualSettings.HunterEquipData.Count == 0) visualSettings.HunterEquipData.Add(new(App_user_data_NpcHunterEquipData.HASH, visualSettings.rsz));
                    visualSettings.HunterEquipData[0].Value = isFemale ? MASTER_NPC_EQUIP_PATH_INTERNAL_FEMALE : MASTER_NPC_EQUIP_PATH_INTERNAL_MALE;

                    /*
                    if (visualSettings.CategoryData.Count > 0) {
                        switch (visualSettings.CategoryData[0]) {
                            case App_cNpcCategoryData_BaseCampHuman:
                            case App_cNpcCategoryData_DesertHuman:
                            case App_cNpcCategoryData_OilHuman:
                            case App_cNpcCategoryData_UniqueHuman:
                                visualSettings.CategoryData = [App_cNpcCategoryData_CoreHuman.Create(visualSettings.rsz)];
                                break;
                        }
                    }

                    visualSettings.ModelData[0].UniqueInfo[0].Prefab[0].Name = null;
                    visualSettings.ModelData[0].UniqueInfo[0].UniqueVisual   = App_NpcDef_UNIQUE_VISUAL_Fixed.INVALID;
                    */

                    return true;
            }
        }
        return false;
    }
}