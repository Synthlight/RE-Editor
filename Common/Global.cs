using System.Diagnostics.CodeAnalysis;
using System.Reflection;

#if MHR
using System.Windows.Markup;

[assembly: XmlnsDefinition("MHR", "RE_Editor.Common")]
#endif

namespace RE_Editor.Common;

public static class Global {
    public const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public static bool      showIdBeforeName = true;
    public static LangIndex locale           = LangIndex.eng;

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

#if MHR
    public static readonly List<string> WEAPON_TYPES = ["Bow", "ChargeAxe", "DualBlades", "GreatSword", "GunLance", "Hammer", "HeavyBowgun", "Horn", "InsectGlaive", "Lance", "LightBowgun", "LongSword", "ShortSword", "SlashAxe"];
#elif MHWS
    public static readonly List<string> WEAPON_TYPES = ["Bow", "ChargeAxe", "GunLance", "Hammer", "HeavyBowgun", "Lance", "LightBowgun", "LongSword", "Rod", "ShortSword", "SlashAxe", "Tachi", "TwinSword", "Whistle"];
#elif RE4
    public static readonly List<string> VARIANTS = ["CH", "MC", "AO"];
    public static readonly List<string> FOLDERS  = ["_Chainsaw", "_Mercenaries", "_AnotherOrder"];
    public static          string       variant  = "";
#endif

#if DD2
    public const string MSG_VERSION = "22";
#elif DRDR
    public const string MSG_VERSION = "23";
#elif MHR
    public const string MSG_VERSION = "539100710";
#elif MHWS
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
#elif RE2
    public const string USER_VERSION = "2";
#elif RE3
    public const string USER_VERSION = "2";
#elif RE4
    public const string USER_VERSION = "2";
#elif RE8
    public const string USER_VERSION = "2";
#endif
}