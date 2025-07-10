#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Common.Structs;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;

namespace RE_Editor.Models;

public class NpcTweaksData {
    private const string INVISIBLE_SLINGER_PREFAB = "GameDesign/Equip/_Prefab/Armor/Female/069/000/Slinger/ch03_069_0006.pfb"; // Sakuratide slinger which is EFX.

    private static readonly Regex FILE_NPC_ID_REGEX = new(@"GameDesign\\NPC\\Character\\[^\\]+\\([^\\]+)\\");

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static readonly List<string> NPCS_WITH_SKIN_ISSUES = [
        "Alma",
        "Erik",
        "Fabius",
        "Nata",
        "Gemma",
        "Olivia",
        "Werner",
        "Y'sai",
        "Zatoh"
    ];

    public readonly HashSet<string>                                visualSettingsFiles    = [];
    public readonly HashSet<string>                                allExcludingSkinIssues = [];
    public readonly Dictionary<string, List<App_NpcDef_ID_Fixed>>  npcIdsByName           = [];
    public readonly Dictionary<App_NpcDef_ID_Fixed, string?>       nameByNpcId            = [];
    public readonly List<App_NpcDef_ID_Fixed>                      npcIdsWithoutAName     = [];
    public readonly Dictionary<App_NpcDef_ID_Fixed, List<string>>  filesByNpcId           = [];
    public readonly Dictionary<string, ReDataFile>                 fileData               = [];
    public readonly Dictionary<App_NpcDef_ID_Fixed, string>        rootVisualFileByNpcId  = [];
    public readonly Dictionary<string, NpcVisualData>              npcDataByName          = [];
    public readonly Dictionary<App_NpcDef_ID_Fixed, NpcVisualData> npcDataByNpcId         = [];
    public readonly Dictionary<App_NpcDef_ID_Fixed, NpcVisualData> npcDataByUnknownNpcId  = [];
    public readonly NpcVisualData                                  unnamedData;

    public NpcTweaksData() {
        // Build a map of NPCs to their ID, and list all unused IDs.
        foreach (var id in Enum.GetValues<App_NpcDef_ID_Fixed>()) {
            var name = DataHelper.NPC_NAME_LOOKUP_BY_ENUM_VALUE[Global.LangIndex.eng]!.TryGet<int, string?>((int) id, null);

            nameByNpcId[id] = name;

            if (name is null or "#Rejected#" or "{0}") {
                npcIdsWithoutAName.Add(id);
            } else {
                if (!npcIdsByName.ContainsKey(name)) npcIdsByName[name] = [];
                npcIdsByName[name].Add(id);
            }
        }

        foreach (var fullPath in Directory.EnumerateFiles($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\NPC\Character", "*_VisualSetting*.user.3", SearchOption.AllDirectories)) {
            var file  = fullPath.Replace(PathHelper.CHUNK_PATH, "");
            var match = FILE_NPC_ID_REGEX.Match(file);
            if (!match.Success) continue;
            var idName = match.Groups[1].Value;
            var npcId  = Enum.Parse<App_NpcDef_ID_Fixed>(idName);

            var data = ReDataFile.Read(@$"{PathHelper.CHUNK_PATH}\{file}");
            if (!data.rsz.TryGetEntryObject<App_user_data_NpcVisualSetting>(out _)) continue;
            fileData[file] = data;

            visualSettingsFiles.Add(file);

            if (!filesByNpcId.ContainsKey(npcId)) filesByNpcId[npcId] = [];
            filesByNpcId[npcId].Add(file);

            var name = nameByNpcId[npcId];
            if (name == null || !NPCS_WITH_SKIN_ISSUES.Contains(name)) {
                allExcludingSkinIssues.Add(file);
            }

            if (file.EndsWith($"{idName}_VisualSetting.user.3")) {
                rootVisualFileByNpcId[npcId] = file;
            }
        }

        foreach (var (name, ids) in npcIdsByName) {
            var npcFiles    = GetNpcFilesForIds(ids);
            var npcRootFile = GetNpcRootFile(ids);

            var data = new NpcVisualData(name, ids, npcFiles, npcRootFile);
            npcDataByName[name] = data;
            foreach (var id in ids) {
                npcDataByNpcId[id] = data;
            }
        }

        foreach (var id in npcIdsWithoutAName) {
            var ids         = new List<App_NpcDef_ID_Fixed> {id};
            var npcFiles    = GetNpcFilesForIds(ids);
            var npcRootFile = GetNpcRootFile(ids);
            if (npcFiles.Count == 0) continue;
            var data = new NpcVisualData(null, ids, npcFiles, npcRootFile);
            npcDataByUnknownNpcId[id] = data;
        }

        var unnamedNpcFiles    = GetNpcFilesForIds(npcIdsWithoutAName);
        var unnamedNpcRootFile = GetNpcRootFile(npcIdsWithoutAName);
        unnamedData = new(null, npcIdsWithoutAName, unnamedNpcFiles, unnamedNpcRootFile);
    }

    private Dictionary<string, ReDataFile> GetNpcFilesForIds(List<App_NpcDef_ID_Fixed> ids) {
        Dictionary<string, ReDataFile> npcFiles = [];
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var id in ids) {
            if (filesByNpcId.TryGetValue(id, out var files)) {
                foreach (var file in files) {
                    npcFiles[file] = fileData[file];
                }
            }
        }
        return npcFiles;
    }

    private string? GetNpcRootFile(List<App_NpcDef_ID_Fixed> ids) {
        List<string> npcFiles = [];
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var id in ids) {
            if (rootVisualFileByNpcId.TryGetValue(id, out var rootVisualFile)) {
                npcFiles.Add(rootVisualFile);
            }
        }
        return npcFiles.Count > 0 ? npcFiles[0] : null;
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

    public static ReDataFile CreateCleanHunterEquipFile(InnerwearOption innerwearOption, bool slingerEnabled) {
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

    public enum InnerwearOption {
        INNERWEAR_1 = 1,
        INNERWEAR_2 = 2,
        INNERWEAR_3 = 3,
        INNERWEAR_4 = 4
    }
}