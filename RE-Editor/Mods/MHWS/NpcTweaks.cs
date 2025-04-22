using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    private const string MASTER_NPC_EQUIP_PATH          = @"\natives\STM\GameDesign\NPC\Data\Master_NpcHunterEquipData.user.3";
    private const string MASTER_NPC_EQUIP_PATH_INTERNAL = @"GameDesign/NPC/Data/Master_NpcHunterEquipData.user";

    [UsedImplicitly]
    public static void Make() {
        const string name        = "NPC Tweaks";
        const string description = "NPC tweaks.";
        const string version     = "1.0";

        const string coreName = $"{name} - Core";

        var baseMod = new NexusMod {
            Version     = version,
            Requirement = coreName,
            Desc        = description
        };

        var mods = new List<NexusMod> {
            baseMod
                .SetName(coreName)
                .SetFiles([])
                .SetAddonFor(null)
                .SetRequirement(null)
                .SetAdditionalFiles(new() {{MASTER_NPC_EQUIP_PATH, CreateCleanHunterEquipFile()}}),
            baseMod
                .SetName("Female NPCs (Human, Innerwear 4)")
                .SetAddonFor(coreName)
                .SetFiles(GetVisualSettingFiles())
                .SetFilteredAction(list => ModStuff(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.FEMALE
                                                                            && visualSettings.ParamPackOwCategory is App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_BC
                                                                                or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST101
                                                                                or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST103
                                                                                or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST105)),
            baseMod
                .SetName("Female NPCs (Ryu, Innerwear 4)")
                .SetAddonFor(coreName)
                .SetFiles(GetVisualSettingFiles())
                .SetFilteredAction(list => ModStuff(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.FEMALE
                                                                            && visualSettings.ParamPackOwCategory == App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_FEMALE)),
            baseMod
                .SetName("Male NPCs (Human, Innerwear 4)")
                .SetAddonFor(coreName)
                .SetFiles(GetVisualSettingFiles())
                .SetFilteredAction(list => ModStuff(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.MALE
                                                                            && visualSettings.ParamPackOwCategory is App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_BC
                                                                                or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST101
                                                                                or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST103
                                                                                or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST105)),
            baseMod
                .SetName("Male NPCs (Ryu, Innerwear 4)")
                .SetAddonFor(coreName)
                .SetFiles(GetVisualSettingFiles())
                .SetFilteredAction(list => ModStuff(list, visualSettings => visualSettings.Gender == App_CharacterDef_GENDER.MALE
                                                                            && visualSettings.ParamPackOwCategory == App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_MALE))
        };

        const string byNameGroup = "Individually, by Name";
        mods.Add(new() {
            Name     = byNameGroup,
            AddonFor = coreName,
            Version  = version,
            Desc     = "Activating this entry does nothing, it exists solely to create the submenu.",
            Files    = [],
            SkipPak  = true
        });

        List<App_NpcDef_ID_Fixed> used = [];
        foreach (var (npcName, npcIds) in from entry in DataHelper.NPC_NAME_LOOKUP_BY_ENUM_VALUE[Global.LangIndex.eng]
                                          where entry.Value != "#Rejected#" && entry.Value != "{0}"
                                          orderby entry.Value
                                          let fixedId = (App_NpcDef_ID_Fixed) entry.Key
                                          group fixedId by entry.Value
                                          into g
                                          select new KeyValuePair<string, List<App_NpcDef_ID_Fixed>>(g.Key, g.ToList())) {
            var idNames = string.Join(", ", from id in npcIds
                                            let idName = Enum.GetName(id)
                                            select idName);
            var files = (from id in npcIds
                         let idName = Enum.GetName(id)
                         select GetVisualSettingFiles(idName)).SelectMany(e => e)
                                                              .ToList();
            if (files.Count == 0) {
                Console.WriteLine($"Warning: Unable to retrieve files for: {npcName}, {idNames}");
                continue;
            }
            mods.Add(baseMod
                     .SetName($"{npcName}: ({idNames})")
                     .SetAddonFor(byNameGroup)
                     .SetFiles(files)
                     .SetFilteredAction(list => ModStuff(list, _ => true)));
            used.AddRange(npcIds);
        }

        var unnamed = Enum.GetValues<App_NpcDef_ID_Fixed>().ToList();
        unnamed.Remove(App_NpcDef_ID_Fixed.INVALID);
        unnamed.Remove(App_NpcDef_ID_Fixed.MAX);
        unnamed.RemoveAll(used.Contains);

        mods.Add(baseMod
                 .SetName("_Unnamed NPCs: (All The Rest)")
                 .SetAddonFor(byNameGroup)
                 .SetFiles((from id in unnamed
                            let idName = Enum.GetName(id)
                            let npcFiles = GetVisualSettingFiles(idName)
                            where npcFiles.Any()
                            select npcFiles).SelectMany(e => e))
                 .SetFilteredAction(list => ModStuff(list, visualSettings => visualSettings.Gender is App_CharacterDef_GENDER.FEMALE or App_CharacterDef_GENDER.MALE
                                                                             && visualSettings.ParamPackOwCategory is App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_BC
                                                                                 or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST101
                                                                                 or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST103
                                                                                 or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST105
                                                                                 or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_FEMALE
                                                                                 or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_BC
                                                                                 or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST101
                                                                                 or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST103
                                                                                 or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST105
                                                                                 or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_MALE)));

        ModMaker.WriteMods(mods, name, copyLooseToFluffy: true);
    }

    private static ReDataFile CreateCleanHunterEquipFile() {
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
                parts.ArmorSubID    = 101; // Variants: 0, 1, 100, 101.
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

        newHunterEquip.rsz.objectData.Add(hunterEquipData);
        newHunterEquip.rsz.objectEntryPoints.Add(1);

        return newHunterEquip;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Float4 NewColor() {
        return new[] {0f, 0f, 0f, 1f};
    }

    private static bool ModStuff(IList<RszObject> rszObjectData, Func<App_user_data_NpcVisualSetting, bool> predicate) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_NpcVisualSetting visualSettings:
                    if (!predicate(visualSettings)) return false;

                    foreach (var part in visualSettings.ModelData[0].ModelInfo[0].PartsList) {
                        part.IsEnabled = false; // Keeps the prefab parts off.
                    }
                    if (visualSettings.HunterEquipData.Count == 0) visualSettings.HunterEquipData.Add(new(App_user_data_NpcHunterEquipData.HASH, visualSettings.rsz));
                    visualSettings.HunterEquipData[0].Value = MASTER_NPC_EQUIP_PATH_INTERNAL;
                    return true;
            }
        }
        return false;
    }

    public static IEnumerable<string> GetVisualSettingFiles(string npcName = null) {
        if (npcName != null) {
            return from file in Directory.EnumerateFiles($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\NPC\Character", $"*{npcName}*_VisualSetting*.user.3", SearchOption.AllDirectories)
                   where File.Exists(file)
                   select file.Replace(PathHelper.CHUNK_PATH, "");
        }
        return from file in Directory.EnumerateFiles($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\NPC\Character", "*_VisualSetting*.user.3", SearchOption.AllDirectories)
               where File.Exists(file)
               select file.Replace(PathHelper.CHUNK_PATH, "");
    }
}