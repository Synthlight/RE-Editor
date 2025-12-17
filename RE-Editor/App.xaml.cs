using System.Windows;
using RE_Editor.Common;
using RE_Editor.Data;
using RE_Editor.Themes;

namespace RE_Editor;

public partial class App {
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        DataInit.Init();
        ThemesController.SetTheme(Global.theme);
    }
}