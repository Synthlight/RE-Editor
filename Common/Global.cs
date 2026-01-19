using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RE_Editor.Common.Models;

#if MHR
using System.Windows.Markup;

[assembly: XmlnsDefinition("MHR", "RE_Editor.Common")]
#endif

namespace RE_Editor.Common;

public static class Global {
    public const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public static Settings  settings = new();
    public static LangIndex locale                => settings.locale;
    public static bool      showIdBeforeName      => settings.showIdBeforeName;
    public static bool      singleClickToEditMode => settings.singleClickToEditMode;
    public static ThemeType theme                 => settings.theme;

    public static readonly string[] FILE_TYPES = [
        $"*.user.{USER_VERSION}"
    ];

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum LangIndex {
        jpn,
        eng,
        fre,
        ita,
        ger,
        spa,
        rus,
        pol,
        ptB = 10,
        kor,
        chT,
        chS,
        ara = 21,
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static readonly Dictionary<LangIndex, string> LANGUAGE_NAME_LOOKUP = new() {
        {LangIndex.ara, "العربية"},
        {LangIndex.chS, "简体中文"},
        {LangIndex.chT, "繁體中文"},
        {LangIndex.eng, "English"},
        {LangIndex.fre, "Français"},
        {LangIndex.ger, "Deutsch"},
        {LangIndex.ita, "Italiano"},
        {LangIndex.jpn, "日本語"},
        {LangIndex.kor, "한국어"},
        {LangIndex.pol, "Polski"},
        {LangIndex.ptB, "Português do Brasil"},
        {LangIndex.rus, "Русский"},
        {LangIndex.spa, "Español"}
    };

    public static readonly List<LangIndex> LANGUAGES = Enum.GetValues(typeof(LangIndex)).Cast<LangIndex>().ToList();

    public static readonly Dictionary<LangIndex, Dictionary<string, string>> TRANSLATION_MAP = new() {
        {
            LangIndex.eng, new() {
                {"Hyakuryu", "Rampage"},
                {"Haykuryu", "Rampage"}, // Yes, it's spelled wrong in a few places. (Like in `PlayerUserDataSkillParameter`.)
                {"Takumi", "Handicraft"},
                {"Hagitori", "Carve"},
                {"Kanpou", "Herbal"},
                {"Sashimi", "Sushifish"},
                {"Kairiki", "Might Pill"},
                {"Nintai", "Adamant Pill"},
                {"KijinDrink", "Demondrug"},
                {"KoukaDrink", "Armorskin"},
                {"KijinPowder", "Demon Powder"},
                {"KoukaPowder", "Hardshell Powder"},
                {"KijinAmmo", "Demon Ammo"},
                {"KoukaAmmo", "Armor Ammo"},
                {"Toishi_KireajiFish", "Whetfish"}, // Match first.
                {"Toishi", "Whetstone"},
                // TODO: Figure out what this is once Wilds is out.
                {"Ryunyu", "Wyvern Milk?"},
                // Yamori // Item Param: Some grill facility result.
                // Ryukil // Skill Param: Dragon something? 'kil' might be kiru which would make it dragon cut?
                // Hunki // Skill Param: Do something with stamina when health falls below the threshold?
                {"BattoWaza", "Punishing Draw"},
                {"BattoPower", "Punishing Draw Power"},
                {"Konshin", "Max Might"},
                {"_G_", "+_"},
            }
        }
    };

// @formatter:off (Because it screws up the alignment of inactive sections.)
#if MHR
    public static readonly List<string> WEAPON_TYPES = ["Bow", "ChargeAxe", "DualBlades", "GreatSword", "GunLance", "Hammer", "HeavyBowgun", "Horn", "InsectGlaive", "Lance", "LightBowgun", "LongSword", "ShortSword", "SlashAxe"];
#elif MHWS
    public static readonly List<string> WEAPON_TYPES = ["Bow", "ChargeAxe", "GunLance", "Hammer", "HeavyBowgun", "Lance", "LightBowgun", "LongSword", "Rod", "ShortSword", "SlashAxe", "Tachi", "TwinSword", "Whistle"];

    public static readonly Dictionary<string, string> WEAPON_TYPE_NAME_TO_TYPE = new() {
        {"Bow", "Bow"},
        {"Charge Blade", "ChargeAxe"},
        {"Dual Blades", "TwinSword"},
        {"Great Sword", "LongSword"},
        {"Gunlance", "GunLance"},
        {"Hammer", "Hammer"},
        {"HBG", "HeavyBowgun"},
        {"Hunting Horn", "Whistle"},
        {"Insect Glaive", "Rod"},
        {"Lance", "Lance"},
        {"LBG", "LightBowgun"},
        {"Long Sword", "Tachi"},
        {"Switch Axe", "SlashAxe"},
        {"Sword & Shield", "ShortSword"},
    };
#elif RE4
    public static readonly List<string> VARIANTS = ["CH", "MC", "AO"];
    public static readonly List<string> FOLDERS  = ["_Chainsaw", "_Mercenaries", "_AnotherOrder"];
    public static          string       variant  = "";
#endif
// @formatter:on

#if DD2
    public const string MSG_VERSION = "22";
#elif DRDR
    public const string MSG_VERSION = "23";
#elif MHR
    public const string MSG_VERSION = "539100710";
#elif MHWS
    public const string MSG_VERSION = "23";
#elif PRAGMATA
    public const string MSG_VERSION = "23";
#elif RE2
    public const string MSG_VERSION = "50397457";
#elif RE3
    public const string MSG_VERSION = "67305745";
#elif RE4
    public const string MSG_VERSION = "22";
#elif RE8
    public const string MSG_VERSION = "33685777";
#endif

#if DD2
    public const string USER_VERSION = "2";
#elif DRDR
    public const string USER_VERSION = "3";
#elif MHR
    public const string USER_VERSION = "2";
#elif MHWS
    public const string USER_VERSION = "3";
#elif PRAGMATA
    public const string USER_VERSION = "3";
#elif RE2
    public const string USER_VERSION = "2";
#elif RE3
    public const string USER_VERSION = "2";
#elif RE4
    public const string USER_VERSION = "2";
#elif RE8
    public const string USER_VERSION = "2";
#endif

#if DD2
    public const string PAK_CREATE_ARGS = "-version 4 1";
#elif DRDR
    public const string PAK_CREATE_ARGS = "";
#elif MHR
    public const string PAK_CREATE_ARGS = "";
#elif MHWS
    public const string PAK_CREATE_ARGS = "-version 4 1";
#elif PRAGMATA
    public const string PAK_CREATE_ARGS = "-version 4 1";
#elif RE2
    public const string PAK_CREATE_ARGS = "";
#elif RE3
    public const string PAK_CREATE_ARGS = "";
#elif RE4
    public const string PAK_CREATE_ARGS = "";
#elif RE8
    public const string PAK_CREATE_ARGS = "";
#endif

#if DD2
    public const string CURRENT_GAME_VERSION = "Update 13 (2025-04-09)";
#elif DRDR
    public const string CURRENT_GAME_VERSION = "Unknown";
#elif MHR
    public const string CURRENT_GAME_VERSION = "Unknown";
#elif MHWS
    public const string CURRENT_GAME_VERSION = "v1.040.02.00";
#elif PRAGMATA
    public const string CURRENT_GAME_VERSION = "Demo v1 (Unknown)";
#elif RE2
    public const string CURRENT_GAME_VERSION = "Unknown";
#elif RE3
    public const string CURRENT_GAME_VERSION = "Unknown";
#elif RE4
    public const string CURRENT_GAME_VERSION = "Unknown";
#elif RE8
    public const string CURRENT_GAME_VERSION = "Unknown";
#endif

// @formatter:off (Because it screws up the alignment of inactive sections.)
#if MHR
    public const string BITSET_NAME       = "Snow_BitSetFlagBase";
    public const string BITSET_FIELD_NAME = "Flag";
#elif MHWS
    public const string BITSET_NAME       = "Ace_Bitset";
    public const string BITSET_FIELD_NAME = "Value";
#else
    public const string BITSET_NAME       = "There is no cow level."; // Something that will always fail when compared to class/field names.
    public const string BITSET_FIELD_NAME = BITSET_NAME;
#endif
// @formatter:on

    static Global() {
        SettingsController.Load();
    }

    public static void Log(string msg) {
        Console.WriteLine(msg);
        Debug.WriteLine(msg);
    }
}