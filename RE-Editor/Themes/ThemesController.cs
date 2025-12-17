using System;
using System.Collections.Generic;
using System.Windows;
using RE_Editor.Common.Models;

namespace RE_Editor.Themes;

public static class ThemesController {
    public static readonly Dictionary<ThemeType, string> THEME_MAP = new() {
        {ThemeType.NONE, "None"},
        {ThemeType.DARK, "Dark (EXPERIMENTAL!!!!)"},
    };

    public static readonly Dictionary<ThemeType, List<ResourceDictionary>> THEMES = new() {
        {ThemeType.NONE, []}, {
            ThemeType.DARK, [
                new() {Source = new("Themes/ColourDictionaries/DeepDark.xaml", UriKind.Relative)},
                new() {Source = new("Themes/ControlColours.xaml", UriKind.Relative)},
                new() {Source = new("Themes/Controls.xaml", UriKind.Relative)},
                new() {Source = new("Themes/CustomControls.xaml", UriKind.Relative)},
            ]
        }
    };

    public static ThemeType CurrentTheme { get; set; }

    public static void SetTheme(ThemeType theme) {
        CurrentTheme = theme;
        Application.Current.Resources.MergedDictionaries.Clear();
        foreach (var resourceDictionary in THEMES[theme]) {
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }
    }
}