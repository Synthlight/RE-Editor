using System;
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

[UsedImplicitly]
public class SortedTitles : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name    = "Titles Sorted by Name";
        const string desc    = "Sorts all the guid card titles by their name.";
        const string version = "1.5.1";
        var          image   = $@"{PathHelper.MODS_PATH}\{name.ToSafeName()}\Untitled.png";

        var langs = Enum.GetValues<Global.LangIndex>();

        var mods = new List<INexusMod>();
        mods.AddRange(langs.Select(lang => {
                               return new NexusMod {
                                   Name         = $"{name} ({lang})",
                                   NameAsBundle = name,
                                   Desc         = desc,
                                   Image        = image,
                                   Files        = [PathHelper.GUILD_CARD_TITLE_DATA],
                                   Action       = data => SortTitles(data, lang),
                                   Version      = version
                               };
                           })
                           .Cast<INexusMod>());

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void SortTitles(IEnumerable<RszObject> rszObjectData, Global.LangIndex lang) {
        var currentLang = Global.locale;
        Global.settings.locale = lang;
        var titles = rszObjectData.OfType<Snow_data_AchievementUserData_Param>().Where(gem => gem.SortID > 0).ToList();
        var titlesInOrder = from title in titles
                            orderby title.RealName
                            select title;
        var sortId = 10000;
        foreach (var title in titlesInOrder) {
            title.SortID = sortId++;
        }
        Global.settings.locale = currentLang;
    }
}