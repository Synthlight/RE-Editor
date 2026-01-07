using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JetBrains.Annotations;
using Microsoft.Win32;
using RE_Editor.Common;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Common.Models.List_Wrappers;
using RE_Editor.Common.PakModels.Hashing;
using RE_Editor.Common.Util;
using RE_Editor.Controls;
using RE_Editor.Models;
using RE_Editor.Util;

namespace RE_Editor.Windows;

public partial class MainWindow {
// @formatter:off (Because it screws up the alignment of inactive sections.)
 // ReSharper disable UnusedMember.Local
#if DEBUG
    private const bool ENABLE_CHEAT_BUTTONS = true;
#else
    private const bool ENABLE_CHEAT_BUTTONS = false;
    public const  bool SHOW_RAW_BYTES       = false;
#endif
 // ReSharper restore UnusedMember.Local
// @formatter:on

#if DD2
    private const string TITLE = "DD2 Editor";
#elif DRDR
    private const string TITLE = "DRDR Editor";
#elif MHR
    private const string TITLE = "MHR Editor";
#elif MHWS
    private const string TITLE = "MHWS Editor";
#elif PRAGMATA
    private const string TITLE = "Pragmata Editor";
#elif RE2
    private const string TITLE = "RE2 Editor";
#elif RE3
    private const string TITLE = "RE3 Editor";
#elif RE4
    private const string TITLE = "RE4 Editor";
#elif RE8
    private const string TITLE = "RE8 Editor";
#endif

    [CanBeNull] private CancellationTokenSource savedTimer;
    [CanBeNull] public  ReDataFile              file;
    public static       string                  targetFile { get; private set; }
    public readonly     string                  filter            = $"RE Data Files|{string.Join(";", Global.FILE_TYPES)}";
    public readonly     List<MethodInfo>        allMakeModMethods = [];

    public MainWindow() {
        var args = Environment.GetCommandLineArgs();

        InitializeComponent();

        Title = TITLE;

        Width  = SystemParameters.MaximizedPrimaryScreenWidth * 0.8;
        Height = SystemParameters.MaximizedPrimaryScreenHeight * 0.5;

        SetupKeybind(new KeyGesture(Key.S, ModifierKeys.Control), (_,                      _) => Save());
        SetupKeybind(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift), (_, _) => Save(true));

        var visibility = File.Exists($@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\enable_cheats") ? Visibility.Visible : Visibility.Collapsed;
        btn_make_mods.Visibility = visibility;
        btn_test.Visibility      = visibility;

        foreach (var modType in NexusModExtensions.GetAllModTypes()) {
            var button = new Button {
                Content = $"Create \"{modType.Name}\" Mod"
            };
            var make = modType.GetMethod("Make", BindingFlags.Static | BindingFlags.Public);
            allMakeModMethods.Add(make);
            button.Click += (_, _) => { make!.Invoke(null, [this]); };
            panel_mods.Children.Add(button);
        }

#if MHR
        btn_wiki_dump.Visibility = visibility;
        btn_all_cheats.Visibility = visibility;
#endif

        UpdateCheck.Run(this);

        TryLoad(args);
    }

    // ReSharper disable once AsyncVoidMethod
    private async void TryLoad(IReadOnlyCollection<string> args) {
        if (args.Count >= 2) {
            var filePath = args.Last();
            if (filePath.StartsWith("-")) return;

            // Tiny delay so the UI is visible to the user before we load.
            await Task.Delay(10);
            Load(filePath);
        }
    }

    private void Load(string filePath = null) {
        try {
            var target = filePath ?? GetOpenTarget();
            if (string.IsNullOrEmpty(target)) return;

            var supportedSearchTarget = target.ToLower().Replace('/', '\\');
            var nativesIndex          = supportedSearchTarget.IndexOf(@"natives\stm", StringComparison.Ordinal);
            if (nativesIndex > 0
                && DataHelper.SUPPORTED_FILES.Length > 0
                && !DataHelper.SUPPORTED_FILES.Contains(supportedSearchTarget[nativesIndex..])) {
                var result = MessageBox.Show("This file has not passed write tests. It may or may not even open.\n" +
                                             "This also means you WILL BE UNABLE TO SAVE ANY CHANGES MADE TO IT.\n\nTry opening the file anyway?", "File not supported.", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result is MessageBoxResult.No or MessageBoxResult.Cancel) return;
            }

            if (DataHelper.OBSOLETE_BY_HASH.Count > 0) {
                var bytes = File.ReadAllBytes(target);
                var hash  = PakFileHash.GetChecksum(bytes);
                if (DataHelper.OBSOLETE_BY_HASH.TryGetValue(hash, out var fileInfo) // If found, it's an outdated file, since good hashes aren't stored.
                    // ReSharper disable once CommentTypo
                    /*
                    MHWS:
                    `Em0022_00_Param_Legendary.user.3` used to have a hash of `523C61F18F1C79D131CA09E0715F54D174EB32755C4A2BED1B4AAD4BC52FB530`.
                    This was updated to `840CDF9E5D3C9DF69D5F44418CBDEB8A380A1C6A1CAD0D9027E755F3FE6933AD` in PAK 10.
                    `Em0002_00_Param_Legendary.user.3` still has the old hash, and is incorrectly matched as obsolete.
                    */
                    && !(target.Contains("em0002_00_param_legendary.user.3", StringComparison.InvariantCultureIgnoreCase) && hash == "523C61F18F1C79D131CA09E0715F54D174EB32755C4A2BED1B4AAD4BC52FB530")) {
                    var result = MessageBox.Show($"This file is outdated. It comes from PAK {fileInfo.sourcePak}, and the latest version of it is in {fileInfo.knownGoodPak}.\n" +
                                                 "Reading an outdated file is unsupported, and might fail.\n" +
                                                 "It will probably also just cause the game to blackscreen on startup. I STRONGLY recommend against using it.\n\n" +
                                                 "Try opening the file anyway?", "File is outdated.", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result is MessageBoxResult.No or MessageBoxResult.Cancel) return;
                }
            }

            targetFile = target;
            Title      = Path.GetFileName(targetFile);

            ClearGrids(main_grid);

            GC.Collect();

#if RE4
            // ReSharper disable StringLiteralTypo
            if (targetFile.ToLower().Contains("_mercenaries")) {
                Global.variant = "MC";
            } else if (targetFile.ToLower().Contains("_anotherorder")) {
                Global.variant = "AO";
            } else {
                Global.variant = "CH";
            }
            // ReSharper restore StringLiteralTypo
#endif

            file = ReDataFile.Read(target);

            var rszObjectData = file?.rsz.objectData;
            if (rszObjectData == null || rszObjectData.Count == 0) throw new("Error loading data; rszObjectData is null/empty.\n\nPlease report the path/name of the file you are trying to load.");
            var rszObjectInfo = file?.rsz.objectEntryPoints;
            if (rszObjectInfo == null || rszObjectInfo.Count == 0) throw new("Error loading data; rszObjectInfo is null/empty.\n\nPlease report the path/name of the file you are trying to load.");

            var entryObject     = file.rsz.GetEntryObject();
            var dataGridControl = CreateDataGridControl(this, entryObject);
            AddMainDataGrid(dataGridControl);

#if MHR
            btn_sort_gems_by_skill_name.Visibility = target.Contains("DecorationsBaseData.user.2") || target.Contains("HyakuryuDecoBaseData.user.2") ? Visibility.Visible : Visibility.Collapsed;
#endif
        } catch (FileNotSupported) {
            MessageBox.Show("It's using a struct or type the editor doesn't support yet.\n" +
                            "You can comment on the nexus page (if there is one) or on Discord about the file (and give the full path),\n" +
                            "but it may take a while.\n" +
                            "Use RE_RSZ as a good alternative.", "File not supported.", MessageBoxButton.OK, MessageBoxImage.Error);
        } catch (Exception e) when (!Debugger.IsAttached) {
            ShowError(e, "Load Error");
        }
    }

    public static IDataGrid CreateDataGridControl([CanBeNull] UIElement control, RszObject rszObj) {
        /*
         * Gets the items & typeName to use as the root entry in the dataGrid.
         * For param types, this is the list of params. (A shortcut we make.)
         * For the rest, it's the entry point & type.
         */
        var structInfo = rszObj.structInfo;
        if (structInfo.fields is {Count: 1} && structInfo.fields[0].array && structInfo.fields[0].type == "Object") {
            var entryType = rszObj.GetType();
            var entryProp = entryType.GetProperty(structInfo.fields[0].name.ToConvertedFieldName()!)!;
            var dataGrid  = CreateDataGridFromProperty(control, rszObj, entryProp);
            return dataGrid;
        } else {
            var structGrid = InstanceStructGridOfType(rszObj);
            Debug.WriteLine($"Loading type: {rszObj.GetType().Name}");
            return structGrid;
        }
    }

    public static IDataGrid CreateDataGridFromProperty([CanBeNull] UIElement control, RszObject sourceObj, PropertyInfo propertyInfo) {
        var items         = propertyInfo.GetGetMethod()!.Invoke(sourceObj, null)!;
        var entryListType = propertyInfo.PropertyType.GenericTypeArguments[0];
        var isList        = (IsListAttribute) propertyInfo.GetCustomAttribute(typeof(IsListAttribute), true) != null;

        if (entryListType.Is(typeof(ITuple))) {
            throw new NotImplementedException("Tuples are not supported yet."); // TODO: Add tuple support to the UI.
        }

        if (!isList) {
            var observableType = typeof(ObservableCollection<>).MakeGenericType(entryListType);
            var count          = (int) observableType.GetProperty(nameof(ObservableCollection<int>.Count), Global.FLAGS)!.GetGetMethod()!.Invoke(items, null)!; // TODO: Remove the `int` generic param once C# 14 come out.

            switch (count) {
                case 0: throw new NoNullAllowedException("Object is null, nothing to open.\n(This isn't really an error, just framed as one.)");
                case > 1: throw new("A non-list type somehow has more than one item in the collection.");
            }

            var rszObj     = ((dynamic) items)[0];
            var structGrid = InstanceStructGridOfType(rszObj);
            Debug.WriteLine($"Loading type: {rszObj.GetType().Name}");
            return structGrid;
        }

        /*
         * If there is only one type, create a list with only that type for the grid so it can correctly plot columns.
         * If there's more than one type, pass the root item and populate the UI so the user can switch target types.
         * If it's empty, just make a grid with the generic type.
         */

        IAutoDataGrid dataGrid;

        var itemTypes = Utils.GetTypesInList((IEnumerable) items!);
        if (itemTypes.Count == 1) {
            dataGrid = InstanceAutoDataGridOfType(control, sourceObj, items, itemTypes[0], propertyInfo);
            Debug.WriteLine($"Loading type: {itemTypes[0].Name}");
        } else if (itemTypes.Count == 0) {
            dataGrid = InstanceAutoDataGridOfType(control, sourceObj, items, entryListType, propertyInfo);
            Debug.WriteLine($"Loading type: {entryListType.Name}");
        } else {
            // TODO: Add a UI so the user can select the target type for the grid.
            // There might also be nothing in the list at all.
            throw new NotImplementedException("Generic lists with multiple concrete types are not supported yet.");
        }

        return dataGrid;
    }

    private static IAutoDataGrid InstanceAutoDataGridOfType([CanBeNull] UIElement control, RszObject rszObj, object items, Type propertyListType, PropertyInfo propertyInfo) {
        var ofType = typeof(Enumerable).GetMethod(nameof(Enumerable.OfType))!.MakeGenericMethod(propertyListType);
        var list   = ofType.Invoke(null, [items]);
        var newObs = Utils.GetGenericConstructor(typeof(ObservableCollection<>), [typeof(IEnumerable<>)], propertyListType)!;
        items = newObs.Invoke([list]);

        var makeGridMethod = typeof(MainWindow).GetMethod(nameof(MakeAutoDataGrid), Global.FLAGS)!.MakeGenericMethod(propertyListType);
        var dataGrid       = (AutoDataGrid) makeGridMethod.Invoke(null, [control, items, rszObj, propertyInfo]);
        return dataGrid;
    }

    private static IStructGrid InstanceStructGridOfType(RszObject rszObj) {
        var type           = rszObj.GetType();
        var makeGridMethod = typeof(MainWindow).GetMethod(nameof(MakeStructGrid), Global.FLAGS)!.MakeGenericMethod(type);
        var structGrid     = (StructGrid) makeGridMethod.Invoke(null, [Convert.ChangeType(rszObj, type)]);
        return structGrid;
    }

    /// <summary>
    /// Creates a data-grid of the given items, and adds keybindings to it for adding/removing rows.
    /// The type given as `T` is used to generate the columns, instance new rows, etc.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="control">What to bind keybindings to.</param>
    /// <param name="items">The items to show in the grid.</param>
    /// <param name="rszObj">Object containing the item list.
    /// Required as the item list might be a generic list; something that is not actually part of the saved data and thus added/removed items would be lost.</param>
    /// <param name="prop">The property that the original list is pulled from, that we will be adding/removing items from.</param>
    /// <returns></returns>
    private static AutoDataGridGeneric<T> MakeAutoDataGrid<T>(UIElement control, ObservableCollection<T> items, RszObject rszObj, PropertyInfo prop) where T : notnull {
        var dataGrid = new AutoDataGridGeneric<T>();
        dataGrid.SetItems(items);
        if (typeof(T).Is(typeof(RszObject)) || typeof(T).IsGeneric(typeof(GenericWrapper<>))) {
            RowHelper<T>.AddKeybinds(control, dataGrid, rszObj, prop);
        }
        return dataGrid;
    }

    private static StructGridGeneric<T> MakeStructGrid<T>(T item) {
        var dataGrid = new StructGridGeneric<T>();
        dataGrid.SetItem(item);
        // No need to bring in the RowHelper here, can't add rows to a vertical struct view.
        return dataGrid;
    }

    public void AddMainDataGrid(IDataGrid dataGrid) {
        dataGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        dataGrid.VerticalScrollBarVisibility   = ScrollBarVisibility.Auto;

        AddMainControl((UIElement) dataGrid);
    }

    private string GetOpenTarget() {
        var ofdResult = new OpenFileDialog {
            Filter           = filter,
            Multiselect      = false,
            InitialDirectory = targetFile == null ? string.Empty : Path.GetDirectoryName(targetFile) ?? string.Empty
        };
        ofdResult.ShowDialog();

        return ofdResult.FileName;
    }

    // ReSharper disable once AsyncVoidMethod
    private async void Save(bool saveAs = false) {
        if (file == null || targetFile == null) return;

        try {
            if (saveAs) {
                var target = GetSaveTarget();
                if (string.IsNullOrEmpty(target)) return;
                targetFile = target;
                Title      = Path.GetFileName(targetFile);
                file!.Write(targetFile!);
            } else {
                file!.Write(targetFile);
            }

            await ShowChangesSaved(true);
        } catch (Exception e) when (!Debugger.IsAttached) {
            ShowError(e, "Save Error");
        }
    }

    private static string GetSaveTarget() {
        var sfdResult = new SaveFileDialog {
            FileName         = $"{Path.GetFileName(targetFile)}",
            InitialDirectory = targetFile == null ? string.Empty : Path.GetDirectoryName(targetFile) ?? string.Empty,
            AddExtension     = false
        };
        return sfdResult.ShowDialog() == true ? sfdResult.FileName : null;
    }

    private async Task ShowChangesSaved(bool changesSaved) {
        savedTimer?.Cancel();
        savedTimer                = new();
        lbl_saved.Visibility      = changesSaved.VisibleIfTrue();
        lbl_no_changes.Visibility = changesSaved ? Visibility.Collapsed : Visibility.Visible;
        try {
            await Task.Delay(3000, savedTimer.Token);
            lbl_saved.Visibility = lbl_no_changes.Visibility = Visibility.Hidden;
        } catch (TaskCanceledException) {
        }
    }

    private void AddMainControl(UIElement uiElement) {
        main_grid.AddControl(uiElement);

        Grid.SetRow(uiElement, 1);
        Grid.SetColumn(uiElement, 0);
        Grid.SetColumnSpan(uiElement, 3);

        main_grid.UpdateLayout();
    }

    public void ClearGrids(Panel panel) {
        var grids = GetGrids().ToList();

        // Remove them.
        foreach (var grid in grids) {
            switch (grid) {
                case AutoDataGrid dataGrid:
                    dataGrid.SetItems(null);
                    break;
                case StructGrid structGrid:
                    structGrid.SetItem(null);
                    break;
            }
            panel.Children.Remove(grid);
        }

        // Cleanup if needed.
        if (grids.Count > 0) {
            panel.UpdateLayout();
        }
    }

    public IEnumerable<UIElement> GetGrids() {
        foreach (UIElement child in main_grid.Children) {
            switch (child) {
                case AutoDataGrid:
                case StructGrid:
                    yield return child;
                    break;
            }
        }
    }

    public static void ShowError(Exception err, string title) {
        var errMsg = "Error occurred. Press Ctrl+C to copy the contents of this window and report to the developer.\r\n\r\n";

        if (!string.IsNullOrEmpty(targetFile)) {
            errMsg += $"Target File: {targetFile}\r\n\r\n";
        }

        MessageBox.Show(errMsg + err, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void SetupKeybind(InputGesture keyGesture, ExecutedRoutedEventHandler onPress) {
        var changeItemValues = new RoutedCommand();
        var ib               = new InputBinding(changeItemValues, keyGesture);
        InputBindings.Add(ib);
        // Bind handler.
        var cb = new CommandBinding(changeItemValues);
        cb.Executed += onPress;
        CommandBindings.Add(cb);
    }

    private void OnDragDrop(object sender, DragEventArgs e) {
        if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            if (files is null) return;
            if (files.Length > 0) {
                Load(files[0]);
            }
        }
    }

    public void ShowThreadProgress(ThreadHandler threadHandler, string text) {
        var info = new ProgressInfo {
            Text          = text,
            ProgressValue = threadHandler.DoneCount,
            ProgressMax   = threadHandler.DoCount
        };
        ThreadHandler.OnUpdateHandler onUpdateHandler = null;
        onUpdateHandler = (doCount, doneCount, threadName) => {
            Utils.RunOnUiThread(() => {
                info.ProgressValue = doneCount;
                info.ProgressMax   = doCount;
                info.Context       = threadName;
            });
            if (doCount == doneCount) {
                threadHandler.OnUpdate -= onUpdateHandler;
                Utils.RunOnUiThread(() => { mod_maker_overlay.Stuff.Remove(info); });
            }
        };
        threadHandler.OnUpdate += onUpdateHandler;
        if (info.ProgressMax > 0) { // Just in-case it finished before we added it to the UI.
            mod_maker_overlay.Stuff.Add(info);
        }
        new Thread(threadHandler.WaitAll).Start();
    }
}