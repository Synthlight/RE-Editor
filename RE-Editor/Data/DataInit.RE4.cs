using System.Collections.Generic;
using RE_Editor.Common;
using RE_Editor.Common.Data;

namespace RE_Editor.Data;

public static partial class DataInit {
    // ReSharper disable once IdentifierTypo
    private static void LoadDicts() {
        DataHelper.ITEM_NAME_LOOKUP          = [];
        DataHelper.WEAPON_NAME_LOOKUP        = [];
        DataHelper.STATUS_EFFECT_NAME_LOOKUP = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(GetAsset("STATUS_EFFECT_NAME_LOOKUP"));

        foreach (var variant in Global.VARIANTS) {
            DataHelper.ITEM_NAME_LOOKUP[variant]   = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(GetAsset($"{variant}_ITEM_NAME_LOOKUP"));
            DataHelper.WEAPON_NAME_LOOKUP[variant] = DataHelper.LoadDict<Global.LangIndex, Dictionary<uint, string>>(GetAsset($"{variant}_WEAPON_NAME_LOOKUP"));
        }

        // Scuffed, but fill in all potentially missing entries with the data from the others without overwriting anything.
        // The game likes to share IDs across the modes and I don't know the rules.
        FillMissing(DataHelper.ITEM_NAME_LOOKUP, ["CH", "AO", "MC"], ["CH", "AO", "MC"]);
        FillMissing(DataHelper.WEAPON_NAME_LOOKUP, ["CH", "AO", "MC"], ["CH", "AO", "MC"]);
    }

    // ReSharper disable once ParameterTypeCanBeEnumerable.Local
    private static void FillMissing(IReadOnlyDictionary<string, Dictionary<Global.LangIndex, Dictionary<uint, string>>> dict, string[] targets, string[] sources) {
        foreach (var source in sources) {
            foreach (var (language, entries) in dict[source]) {
                foreach (var (id, text) in entries) {
                    foreach (var target in targets) {
                        dict[target][language].TryAdd(id, text);
                    }
                }
            }
        }
    }
}