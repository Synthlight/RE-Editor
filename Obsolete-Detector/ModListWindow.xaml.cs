using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RE_Editor.Common.Models;
using RE_Editor.Obsolete_Detector.Models;

namespace RE_Editor.Obsolete_Detector;

public partial class ModListWindow {
    private readonly string                            targetDir;
    public           ObservableCollection<ObsoleteMod> obsoleteMods { get; } = [];

    public ModListWindow(string targetDir, IEnumerable<ObsoleteFileData> obsoleteFileList) {
        this.targetDir = targetDir;
        foreach (var fileData in obsoleteFileList) {
            obsoleteMods.Add(new(fileData, fileData.pak ?? "N/A", fileData.path, fileData.GetReasonText(), fileData.obsoletedBy, fileData.modName ?? "") {
                ToDelete = fileData.reason != Reason.LENGTH && fileData.modName == null
            });
        }

        InitializeComponent();
        main_text.Text = "Potentially obsolete files detected. Files with 'length' matches are not always obsolete, e.g. shop lists that add data will always have a different size.\n" +
                         "Select files to be deleted:\n" +
                         "(Double click row, or single click checkbox cell.)\n" +
                         "\n" +
                         "If FMMs dir is set, and a source mod is found, delete will be unchecked by default. You're better off turning off the mod than directly deleting the files.";
        Width = SystemParameters.MaximizedPrimaryScreenWidth * 0.8;
    }

    private void Delete_OnClick(object sender, RoutedEventArgs e) {
        try {
            foreach (var mod in obsoleteMods) {
                if (mod.ToDelete) {
                    var basePath = $"{targetDir}/{mod.fileData.pak ?? mod.fileData.path}";
                    File.Delete(basePath);
                }
            }

            MessageBox.Show("Checked files successfully deleted.\r\nIf you still have a blackscreen after,\r\ntest without all mods and then just REFramework.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        } catch (Exception err) when (!Debugger.IsAttached) {
            MainWindow.ShowError(err);
        }
    }

    private void Rename_OnClick(object sender, RoutedEventArgs e) {
        try {
            foreach (var mod in obsoleteMods) {
                if (mod.ToDelete) {
                    var basePath = $"{targetDir}/{mod.fileData.pak ?? mod.fileData.path}";
                    File.Move(basePath, $"{basePath}.old");
                }
            }

            MessageBox.Show("Checked files successfully renamed to {file}.old.\r\nIf you still have a blackscreen after,\r\ntest without all mods and then just REFramework.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        } catch (Exception err) when (!Debugger.IsAttached) {
            MainWindow.ShowError(err);
        }
    }

    private void RowDoubleClick(object sender, MouseButtonEventArgs e) {
        var mod = (ObsoleteMod) ((DataGridRow) e.Source).DataContext;
        mod.ToDelete = !mod.ToDelete;
    }

    private void CellClick(object sender, MouseButtonEventArgs e) {
        if (e.Source is DataGridCell cell && cell.Column.SortMemberPath == nameof(ObsoleteMod.ToDelete)) {
            var mod = (ObsoleteMod) cell.DataContext;
            mod.ToDelete = !mod.ToDelete;
        }
    }
}

public class ObsoleteMod(ObsoleteFileData fileData, string inPak, string path, string reason, string obsoletedBy, string modName) : OnPropertyChangedBase {
    public readonly ObsoleteFileData fileData = fileData;
    public          string           InPak       { get; } = inPak;
    public          string           Path        { get; } = path;
    public          string           Reason      { get; } = reason;
    public          string           ObsoletedBy { get; } = obsoletedBy;
    public          string           ModName     { get; } = modName;
    public          bool             ToDelete    { get; set; }
}