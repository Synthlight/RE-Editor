using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Common.Structs;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class NpcTweaks : IMod {
    private const string MASTER_NPC_EQUIP_PATH_FEMALE          = @"natives\STM\GameDesign\NPC\Data\Master_Female_NpcHunterEquipData.user.3";
    private const string MASTER_NPC_EQUIP_PATH_MALE            = @"natives\STM\GameDesign\NPC\Data\Master_Male_NpcHunterEquipData.user.3";
    private const string MASTER_NPC_EQUIP_PATH_INTERNAL_FEMALE = @"GameDesign/NPC/Data/Master_Female_NpcHunterEquipData.user";
    private const string MASTER_NPC_EQUIP_PATH_INTERNAL_MALE   = @"GameDesign/NPC/Data/Master_Male_NpcHunterEquipData.user";
    public const  string UNUSED_KEY                            = "{UNUSED}";
    public const  string PLACEHOLDER_ENTRY_TEXT                = "Activating this entry does nothing, it exists solely to create the submenu.";
    private const string INVISIBLE_SLINGER_PREFAB              = "GameDesign/Equip/_Prefab/Armor/Female/069/000/Slinger/ch03_069_0006.pfb"; // Sakuratide slinger which is EFX.

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static readonly List<string> NPCS_WITH_SKIN_ISSUES = [
        "Alma",
        "Erik",
        "Fabius",
        "Nata",
        "Gemma",
        "Olivia",
        "Werner"
    ];

    [UsedImplicitly]
    public static void Make() {
        const string name        = "NPC Outfit Tweaks";
        const string description = "NPC outfit tweaks - Makes all NPCs wear a given outfit.";
        const string version     = "1.1";

        const string coreName = "Core (MUST ACTIVATE ONE FOR EACH GENDER USED!)";

        var npcIdsByName           = GetNpcIdsByName();
        var visualSettingsFiles    = GetAllVisualSettingsFiles();
        var filesByNpcId           = GetAllFilesByNpcId(visualSettingsFiles);
        var allExcludingSkinIssues = GetAllFilesExcludingSkinIssues(filesByNpcId);

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

        foreach (var innerwearOption in Enum.GetValues<InnerwearOption>()) {
            mods.Add(baseMod.SetName($"Core (Female): Innerwear {(int) innerwearOption}                      ,")
                            .SetFiles([])
                            .SetAddonFor(coreName)
                            .SetAdditionalFiles(new() {{MASTER_NPC_EQUIP_PATH_FEMALE, CreateCleanHunterEquipFile(innerwearOption, true)}}));
            mods.Add(baseMod.SetName($"Core (Female): Innerwear {(int) innerwearOption} (No Slinger)")
                            .SetFiles([])
                            .SetAddonFor(coreName)
                            .SetAdditionalFiles(new() {{MASTER_NPC_EQUIP_PATH_FEMALE, CreateCleanHunterEquipFile(innerwearOption, false)}}));
            mods.Add(baseMod.SetName($"Core (Male): Innerwear {(int) innerwearOption}                      ,")
                            .SetFiles([])
                            .SetAddonFor(coreName)
                            .SetAdditionalFiles(new() {{MASTER_NPC_EQUIP_PATH_MALE, CreateCleanHunterEquipFile(innerwearOption, true)}}));
            mods.Add(baseMod.SetName($"Core (Male): Innerwear {(int) innerwearOption} (No Slinger)")
                            .SetFiles([])
                            .SetAddonFor(coreName)
                            .SetAdditionalFiles(new() {{MASTER_NPC_EQUIP_PATH_MALE, CreateCleanHunterEquipFile(innerwearOption, false)}}));
        }

        mods.AddRange([
            baseMod
                .SetName("Female NPCs (Human, Excluding Skin Issues)")
                .SetAddonFor(name)
                .SetFiles(allExcludingSkinIssues)
                .SetFilteredAction(list => ChangeVisualSettings(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.FEMALE
                                                                                        && visualSettings.ParamPackOwCategory is App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_BC
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST101
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST103
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST105)),
            baseMod
                .SetName("Female NPCs (Human, All)")
                .SetAddonFor(name)
                .SetFiles(visualSettingsFiles)
                .SetFilteredAction(list => ChangeVisualSettings(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.FEMALE
                                                                                        && visualSettings.ParamPackOwCategory is App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_BC
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST101
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST103
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST105)),
            baseMod
                .SetName("Female NPCs (Wyverian)")
                .SetAddonFor(name)
                .SetFiles(visualSettingsFiles)
                .SetFilteredAction(list => ChangeVisualSettings(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.FEMALE
                                                                                        && visualSettings.ParamPackOwCategory == App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_FEMALE)),
            baseMod
                .SetName("Male NPCs (Human, Excluding Skin Issues)")
                .SetAddonFor(name)
                .SetFiles(allExcludingSkinIssues)
                .SetFilteredAction(list => ChangeVisualSettings(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.MALE
                                                                                        && visualSettings.ParamPackOwCategory is App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_BC
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST101
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST103
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST105)),
            baseMod
                .SetName("Male NPCs (Human, All)")
                .SetAddonFor(name)
                .SetFiles(visualSettingsFiles)
                .SetFilteredAction(list => ChangeVisualSettings(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.MALE
                                                                                        && visualSettings.ParamPackOwCategory is App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_BC
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST101
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST103
                                                                                            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST105)),
            baseMod
                .SetName("Male NPCs (Wyverian)")
                .SetAddonFor(name)
                .SetFiles(visualSettingsFiles)
                .SetFilteredAction(list => ChangeVisualSettings(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.MALE
                                                                                        && visualSettings.ParamPackOwCategory == App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_MALE))
        ]);

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

        List<App_NpcDef_ID_Fixed> used = [];
        foreach (var (npcName, npcIds) in npcIdsByName) {
            if (npcName == UNUSED_KEY) continue;
            var idNames = string.Join(", ", from id in npcIds
                                            let idName = Enum.GetName(id)
                                            select idName);
            var files = (from id in npcIds
                         where filesByNpcId.ContainsKey(id)
                         from path in filesByNpcId[id]
                         orderby path
                         select path).ToList();
            if (files.Count == 0) {
                Console.WriteLine($"Warning: Unable to retrieve files for: {npcName}, {idNames}");
                continue;
            }
            mods.Add(baseMod
                     .SetName($"{npcName}: ({idNames})")
                     .SetAddonFor(byNameGroup)
                     .SetFiles(files)
                     .SetFilteredAction(list => ChangeVisualSettings(list, IsAllowed)));
            used.AddRange(npcIds);
        }

        var unnamedFiles = (from id in npcIdsByName[UNUSED_KEY]
                            where !used.Contains(id)
                            where filesByNpcId.ContainsKey(id)
                            from path in filesByNpcId[id]
                            orderby path
                            select path).Distinct()
                                        .ToList();

        mods.Add(baseMod
                 .SetName("_Unnamed NPCs: (All The Rest)")
                 .SetAddonFor(byNameGroup)
                 .SetFiles(unnamedFiles)
                 .SetFilteredAction(list => ChangeVisualSettings(list, IsAllowed)));

        ModMaker.WriteMods(mods, name, copyLooseToFluffy: true);
    }

    public static bool IsAllowed(App_user_data_NpcVisualSetting visualSettings) {
        return visualSettings.Gender is App_CharacterDef_GENDER.FEMALE or App_CharacterDef_GENDER.MALE
               && visualSettings.ParamPackOwCategory is App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_BC
                   or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST101
                   or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST103
                   or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST105
                   or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_FEMALE
                   or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_BC
                   or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST101
                   or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST103
                   or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST105
                   or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_MALE;
    }

    public static Dictionary<string, List<App_NpcDef_ID_Fixed>> GetNpcIdsByName() {
        // Build a map of NPCs to their ID, and exclude anything unused. (Which won't account for values that never had a name associated with them.)
        var npcIdsByName = (from entry in DataHelper.NPC_NAME_LOOKUP_BY_ENUM_VALUE[Global.LangIndex.eng]
                            where entry.Value != "#Rejected#" && entry.Value != "{0}"
                            orderby entry.Value
                            let fixedId = (App_NpcDef_ID_Fixed) entry.Key
                            group fixedId by entry.Value
                            into g
                            select new KeyValuePair<string, List<App_NpcDef_ID_Fixed>>(g.Key, g.ToList())).ToDictionary(pair => pair.Key, pair => pair.Value);

        // Now create an unused entry for everything not in the map.
        npcIdsByName[UNUSED_KEY] = (from enumValue in Enum.GetValues<App_NpcDef_ID_Fixed>()
                                    from entry in npcIdsByName
                                    where enumValue != App_NpcDef_ID_Fixed.INVALID && enumValue != App_NpcDef_ID_Fixed.MAX
                                    where !entry.Value.Contains(enumValue)
                                    select enumValue).ToList();

        return npcIdsByName;
    }

    public static List<string> GetAllVisualSettingsFiles() {
        var visualSettingsFiles = (from file in Directory.EnumerateFiles($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\NPC\Character", "*_VisualSetting*.user.3", SearchOption.AllDirectories)
                                   select file.Replace(PathHelper.CHUNK_PATH, "")).ToList();
        return visualSettingsFiles;
    }

    public static Dictionary<App_NpcDef_ID_Fixed, List<string>> GetAllFilesByNpcId(List<string> visualSettingsFiles) {
        var fileNpcIdRegex = new Regex(@"GameDesign\\NPC\\Character\\[^\\]+\\([^\\]+)\\");
        var filesByNpcId = (from file in visualSettingsFiles
                            let match = fileNpcIdRegex.Match(file)
                            where match.Success
                            let npcId = Enum.Parse<App_NpcDef_ID_Fixed>(match.Groups[1].Value)
                            group file by npcId
                            into g
                            orderby g.Key
                            select new KeyValuePair<App_NpcDef_ID_Fixed, List<string>>(g.Key, g.ToList())).ToDictionary(pair => pair.Key, pair => pair.Value);
        return filesByNpcId;
    }

    private static HashSet<string> GetAllFilesExcludingSkinIssues(Dictionary<App_NpcDef_ID_Fixed, List<string>> filesByNpcId) {
        var allExcludingSkinIssues = new HashSet<string>();
        foreach (var (id, files) in filesByNpcId) {
            if (DataHelper.NPC_NAME_LOOKUP_BY_ENUM_VALUE[Global.LangIndex.eng].ContainsKey((int) id)
                && NPCS_WITH_SKIN_ISSUES.Contains(DataHelper.NPC_NAME_LOOKUP_BY_ENUM_VALUE[Global.LangIndex.eng][(int) id])) {
                continue;
            }
            foreach (var file in files) {
                allExcludingSkinIssues.Add(file);
            }
        }
        return allExcludingSkinIssues;
    }

    private static ReDataFile CreateCleanHunterEquipFile(InnerwearOption innerwearOption, bool slingerEnabled) {
        var newHunterEquip = new ReDataFile {
            magic        = ReDataFile.Magic.id_USR,
            resourceInfo = [],
            userDataInfo = [],
            rsz = new() {
                magic             = 5919570,
                version           = 16,
                objectEntryPoints = [],
                instanceInfo      = [],
                userDataInfo      = [],
                objectData        = [],
            },
        };
        var hunterEquipData = App_user_data_NpcHunterEquipData.Create(newHunterEquip.rsz);
        hunterEquipData.IsUnique = false;

        hunterEquipData.WeaponData = [App_user_data_NpcHunterEquipData_cWeaponData.Create(newHunterEquip.rsz)];
        var weaponData = hunterEquipData.WeaponData[0];
        weaponData.WpType                   = [App_WeaponDef_TYPE_Serializable.Create(newHunterEquip.rsz)];
        weaponData.WpType_Unwrapped         = App_WeaponDef_TYPE_Fixed.INVALID;
        weaponData.WeaponID                 = -1;
        weaponData.CustomPartsConditionList = [];

        hunterEquipData.ArmorData = [App_user_data_NpcHunterEquipData_cArmorData.Create(newHunterEquip.rsz)];
        var armorData = hunterEquipData.ArmorData[0];
        armorData.SColor_A                = [App_user_data_NpcHunterEquipData_cArmorData_cColorInfo.Create(newHunterEquip.rsz)];
        armorData.SColor_A[0].IsApply     = false;
        armorData.SColor_A[0].Value       = [NewColor()];
        armorData.SColor_B                = [App_user_data_NpcHunterEquipData_cArmorData_cColorInfo.Create(newHunterEquip.rsz)];
        armorData.SColor_B[0].IsApply     = false;
        armorData.SColor_B[0].Value       = [NewColor()];
        armorData.UColor_Upper            = [App_user_data_NpcHunterEquipData_cArmorData_cColorInfo.Create(newHunterEquip.rsz)];
        armorData.UColor_Upper[0].IsApply = false;
        armorData.UColor_Upper[0].Value   = [NewColor()];
        armorData.UColor_Lower            = [App_user_data_NpcHunterEquipData_cArmorData_cColorInfo.Create(newHunterEquip.rsz)];
        armorData.UColor_Lower[0].IsApply = false;
        armorData.UColor_Lower[0].Value   = [NewColor()];

        armorData.ArmorParts = [];
        foreach (var part in Enum.GetValues<App_ArmorDef_ARMOR_PARTS>()) {
            var parts = App_user_data_NpcHunterEquipData_cArmorData_cArmorInfo.Create(newHunterEquip.rsz);
            if (part == App_ArmorDef_ARMOR_PARTS.HELM) {
                parts.IsInstantiate = false;
                parts.ArmorID       = -1;
                parts.ArmorSubID    = -1;
            } else {
                parts.IsInstantiate = true; // If false, part is invisible.
                parts.ArmorID       = 2; // Innerwear.
                parts.ArmorSubID    = GetInnerwearSubId(innerwearOption); // Variants: 0, 1, 100, 101.
            }
            parts.SColor_A            = [App_user_data_NpcHunterEquipData_cArmorData_cColorInfo.Create(newHunterEquip.rsz)];
            parts.SColor_A[0].IsApply = false;
            parts.SColor_A[0].Value   = [NewColor()];
            parts.SColor_B            = [App_user_data_NpcHunterEquipData_cArmorData_cColorInfo.Create(newHunterEquip.rsz)];
            parts.SColor_B[0].IsApply = false;
            parts.SColor_B[0].Value   = [NewColor()];
            armorData.ArmorParts.Add(parts);
        }

        armorData.OverwriteSlingerPrefab = [Prefab.Create(newHunterEquip.rsz)];

        if (!slingerEnabled) {
            var slinger = armorData.OverwriteSlingerPrefab[0];
            slinger.Enabled = true;
            slinger.Name    = INVISIBLE_SLINGER_PREFAB;
        }

        newHunterEquip.rsz.objectData.Add(hunterEquipData);
        newHunterEquip.rsz.objectEntryPoints.Add(1);

        return newHunterEquip;
    }

    private static int GetInnerwearSubId(InnerwearOption innerwearOption) {
        return innerwearOption switch {
            InnerwearOption.INNERWEAR_1 => 0,
            InnerwearOption.INNERWEAR_2 => 1,
            InnerwearOption.INNERWEAR_3 => 100,
            InnerwearOption.INNERWEAR_4 => 101,
            _ => throw new ArgumentOutOfRangeException(nameof(innerwearOption), innerwearOption, null)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Float4 NewColor() {
        return new[] {0f, 0f, 0f, 1f};
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

    private enum InnerwearOption {
        INNERWEAR_1 = 1,
        INNERWEAR_2 = 2,
        INNERWEAR_3 = 3,
        INNERWEAR_4 = 4
    }
}