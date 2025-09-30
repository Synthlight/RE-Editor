using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

/// <summary>
/// This sort of works, and sort of doesn't.
/// It *does* actually sort the titles, as sorting seems to be based on the order they appear in the user file.
/// The problem is, the name plate doesn't actually show any text. The text selections work, and it's all correct when you save/quit/remove mod, and look.
/// It just doesn't like to show text when the mod is active and I don't know why.
/// </summary>
[UsedImplicitly]
public class SortedTitles : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Sorted Titles";
        const string description = "Resorts titles by name.";
        const string version     = "1.0";

        var baseMod = new NexusMod {
            //NameAsBundle = name,
            Version = version,
            Desc    = description,
            //Image = $@"{PathHelper.MODS_PATH}\{name}\Pic.png"
        };

        List<INexusMod> mods = [
            new NexusMod {
                Name          = name,
                Version       = version,
                Desc          = "Activating this entry does nothing, it exists solely to create the submenu.",
                Files         = [],
                SkipPak       = true,
                AlwaysInclude = true
            }
        ];

        foreach (var lang in Global.LANGUAGES) {
            var langGroupName = $"Sorting Language- {lang} (Titles)";

            mods.AddRange(new List<INexusMod> {
                new NexusMod {
                    Name          = langGroupName,
                    AddonFor      = name,
                    Version       = version,
                    Desc          = "Activating this entry does nothing, it exists solely to create the second submenu.",
                    Files         = [],
                    SkipPak       = true,
                    AlwaysInclude = true
                }
            });

            mods.AddRange(new List<INexusMod> {
                baseMod
                    .SetName($"Titles Sorted by Name (PAK, {lang})")
                    .SetAddonFor(langGroupName)
                    .SetFiles([PathHelper.TITLE_WORD_DATA_PATH])
                    .SetAction(SortTitles),
                baseMod
                    .SetName($"Conjunctions Sorted by Name (PAK, {lang})")
                    .SetAddonFor(langGroupName)
                    .SetFiles([PathHelper.TITLE_CONJUNCTION_DATA_PATH])
                    .SetAction(SortTitles)
            });
        }

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void SortTitles(List<RszObject> rszObjectData) {
        var root      = rszObjectData.OfType<App_user_data_TitleData>().First();
        var entries   = root.Values.Cast<App_user_data_TitleData_cData>().ToList();
        var noneEntry = entries[0];
        entries.RemoveAt(0);
        var inOrder = entries.OrderBy(data => data.Name_).ToList();

        /*
        for (var i = 0; i < inOrder.Count; i++) {
            var entry = (App_user_data_TitleData_cData) inOrder[i];
            if (entry.TitleId_Unwrapped is App_HunterProfileDef_TITLE_ID_Fixed.TITLE_0000 or App_HunterProfileDef_TITLE_ID_Fixed.CONJUNCTION_0000) continue;
            entry._Index = i + 100;
        }
        inOrder = inOrder.OrderBy(data => ((App_user_data_TitleData_cData) data)._Index).ToList();
        */

        root.Values = [noneEntry];
        foreach (var entry in inOrder) {
            root.Values.Add(entry);
        }
        //root.Values = new(inOrder.Cast<Ace_user_data_ExcelUserData_cData>());
    }
}