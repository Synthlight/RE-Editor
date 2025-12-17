using System.Linq;
using System.Windows;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Controls;
using RE_Editor.Themes;

namespace RE_Editor.Windows {
    public partial class SettingsWindow {
        public Global.LangIndex Locale                { get; set; } = Global.locale;
        public bool             ShowIdBeforeName      { get; set; } = Global.showIdBeforeName;
        public bool             SingleClickToEditMode { get; set; } = Global.singleClickToEditMode;
        public ThemeType        Theme                 { get; set; } = Global.theme;

        public SettingsWindow() {
            Owner = Application.Current.MainWindow;

            InitializeComponent();

            cbx_localization.ItemsSource = Global.LANGUAGE_NAME_LOOKUP;
            cbx_theme.ItemsSource        = ThemesController.THEME_MAP;
        }

        private void BtnSaveClick(object sender, RoutedEventArgs e) {
            var refreshButtons = false;
            if (Global.settings.locale != Locale) {
                refreshButtons         = true;
                Global.settings.locale = Locale;
            }
            if (Global.settings.showIdBeforeName != ShowIdBeforeName) {
                refreshButtons                   = true;
                Global.settings.showIdBeforeName = ShowIdBeforeName;
            }
            Global.settings.singleClickToEditMode = SingleClickToEditMode;
            if (Global.settings.theme != Theme) {
                Global.settings.theme = Theme;
                ThemesController.SetTheme(Global.settings.theme);
            }

            if (refreshButtons) {
                var mainWindow = Application.Current.Windows.OfType<MainWindow>().First();
                foreach (var grid in mainWindow.GetGrids().OfType<AutoDataGrid>()) {
                    grid.RefreshHeaderText();
                }
                if (mainWindow.file != null) {
                    foreach (var item in mainWindow.file.rsz.objectData) {
                        if (item is OnPropertyChangedBase io) {
                            foreach (var propertyInfo in io.GetType().GetProperties()) {
                                if (propertyInfo.Name == "Name"
                                    || propertyInfo.Name.EndsWith("_button")) {
                                    io.OnPropertyChanged(propertyInfo.Name);
                                }
                            }
                        }
                    }
                }
            }

            SettingsController.Save();
            Close();
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}