using System.Windows;
using RE_Editor.Obsolete_Detector.Data;

namespace RE_Editor.Obsolete_Detector;

public partial class App {
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        ObsoleteData.Init();
    }
}