#nullable enable
using System.Collections.Generic;
using System.Linq;
using RE_Editor.Common.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;

namespace RE_Editor.Models;

public class NpcVisualData {
    public readonly string?                        name;
    public readonly List<App_NpcDef_ID_Fixed>      ids;
    public readonly Dictionary<string, ReDataFile> visualSettingsData;
    public readonly string?                        rootVisualFile;
    public readonly App_CharacterDef_GENDER        gender;
    public readonly Species                        species;
    public readonly bool                           adult; // Doesn't include children, giants, etc.

    public NpcVisualData(string? name, List<App_NpcDef_ID_Fixed> ids, Dictionary<string, ReDataFile> visualSettingsData, string? rootVisualFile) {
        this.name               = name;
        this.ids                = ids;
        this.visualSettingsData = visualSettingsData;
        this.rootVisualFile     = rootVisualFile;

        var data           = visualSettingsData.Values.First();
        var visualSettings = data.rsz.GetEntryObject<App_user_data_NpcVisualSetting>();

        species = GetSpecies(visualSettings.ParamPackOwCategory);
        gender  = visualSettings.Gender;
        adult   = IsAdult(visualSettings.ParamPackOwCategory);
    }

    private static Species GetSpecies(App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed data) {
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (data) {
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST101:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST103:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST105:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_BC:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST101:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST103:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST105:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_BC:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_CLD_ST101:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_CLD_ST103:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_OLD_ST101:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_OLD_ST103:
                return Species.HUMAN;
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_MALE:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_FEMALE:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_SML:
            case App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_HGE:
                return Species.WYVERIAN;
            default:
                return Species.OTHER;
        }
    }

    private static bool IsAdult(App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed data) {
        return data is App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST101
            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST103
            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_ST105
            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_MALE_BC
            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST101
            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST103
            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_ST105
            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.HUM_ADL_FEMALE_BC
            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_MALE
            or App_NpcDef_PARAM_PACK_OW_CATEGORY_Fixed.RYU_NML_FEMALE;
    }

    public bool IsAllowed() {
        return species is Species.HUMAN or Species.WYVERIAN
               && gender is App_CharacterDef_GENDER.MALE or App_CharacterDef_GENDER.FEMALE
               && adult;
    }

    public enum Species {
        HUMAN,
        WYVERIAN,
        OTHER
    }
}