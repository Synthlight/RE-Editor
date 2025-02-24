using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using RE_Editor.Common;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Controls.Models;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Util;
using RE_Editor.Windows;

#if RE4
using RE_Editor.Common.Data;
#elif MHR
using Bitset = RE_Editor.Models.Structs.Snow_BitSetFlagBase;
#elif MHWS
using Bitset = RE_Editor.Models.Structs.Ace_Bitset;
#endif

namespace RE_Editor.Controls;

public interface IStructGrid : IDataGrid {
    void SetItem(object item);
    void Refresh();
}

public interface IStructGrid<T> : IStructGrid where T : RszObject {
    T    Item { get; }
    void SetItem(T item);
}

public abstract partial class StructGrid : IStructGrid {
    protected StructGrid() {
        if (!Utils.IsRunningFromUnitTests) InitializeComponent();
    }

    public abstract void SetItem(object item);
    public abstract void Refresh();
}

/***
 ** A vertical layout of a single struct.
 **/
public class StructGridGeneric<T> : StructGrid, IStructGrid<T> where T : RszObject {
    private T item;
    public T Item {
        get => item;
        set {
            if (Utils.IsRunningFromUnitTests) return;
            ClearContents();
            item = value;
            SetupRows();
        }
    }

    // ReSharper disable once ParameterHidesMember
    public void SetItem(T item) {
        Item = item;
    }

    // ReSharper disable once ParameterHidesMember
    public override void SetItem(object item) {
        Item = (T) item;
    }

    private void ClearContents() {
        if (Item == null) return;

        grid.Children.Clear();
        grid.RowDefinitions.Clear();
    }

    private void SetupRows() {
        if (Item == null) return;

        var properties = typeof(T).GetProperties();
        var rows       = new List<Row>(properties.Length);

// @formatter:off (Because it screws up the alignment of inactive sections.)
#if MHR
        var isBitset = typeof(T).Is(typeof(Bitset));
#elif MHWS
        var isBitset        = typeof(T).Is(typeof(Bitset));
        var maxBitElement   = -1;
        var bitElementCount = 0;
        if (isBitset) {
            var maxElementProp = properties.First(prop => prop.Name == nameof(Bitset.MaxElement));
            maxBitElement = (int) maxElementProp.GetGetMethod()!.Invoke(item, null)!;
        }
#endif
// @formatter:on

        foreach (var propertyInfo in properties) {
            var propertyName = propertyInfo.Name;
            if (properties.Any(prop => prop.Name == $"{propertyName}_button")) continue; // Skip the fields that have buttons so we only show the button fields.
            if (propertyName == "Index") continue;

#if MHWS
            if (isBitset && propertyName is nameof(Bitset.Value) or nameof(Bitset.MaxElement)) continue;
#endif

            var displayName    = ((DisplayNameAttribute) propertyInfo.GetCustomAttribute(typeof(DisplayNameAttribute), true))?.DisplayName;
            var sortOrder      = ((SortOrderAttribute) propertyInfo.GetCustomAttribute(typeof(SortOrderAttribute), true))?.sortOrder ?? 0;
            var isList         = (IsListAttribute) propertyInfo.GetCustomAttribute(typeof(IsListAttribute), true) != null;
            var showAsHex      = (ShowAsHexAttribute) propertyInfo.GetCustomAttribute(typeof(ShowAsHexAttribute), true) != null;
            var genericTypeDef = propertyInfo.PropertyType.IsGenericType ? propertyInfo.PropertyType.GetGenericTypeDefinition() : null;
            //var genericParamType    = propertyInfo.PropertyType.IsGenericType ? propertyInfo.PropertyType.GenericTypeArguments[0] : null;
            //var genericParamTypeDef = genericParamType?.IsGenericType == true ? genericParamType.GetGenericTypeDefinition() : null;

            if (displayName is "") continue;
            displayName ??= propertyName;

            var row = new Row {sortOrder = sortOrder};

            var headerInfo = new HeaderInfo(displayName, propertyName);
            var header     = new TextBlock {Padding = new(4)};

            header.SetBinding(TextBlock.TextProperty, new Binding(nameof(HeaderInfo.OriginalText)) {Source = headerInfo});
            row.name = header;

            if (propertyInfo.PropertyType.IsEnum) {
                var comboBox = new ComboBox();
                comboBox.SetBinding(ItemsControl.ItemsSourceProperty, new Binding {Source            = Enum.GetValues(propertyInfo.PropertyType)});
                comboBox.SetBinding(Selector.SelectedItemProperty, new Binding(propertyName) {Source = Item, ValidatesOnExceptions = true});

                row.value = comboBox;
            } else if (propertyInfo.PropertyType == typeof(bool)) {
                var checkBox = new CheckBox();
                checkBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding(propertyName) {Source = Item, ValidatesOnExceptions = true});

                row.value = checkBox;
            } else if (isList || genericTypeDef?.Is(typeof(ObservableCollection<>)) == true) {
                var button = new Button {Content = "Open"};
                button.Click += (_, _) => OpenGrid(propertyInfo, displayName);

                row.value = button;
            } else {
                var textBox = new TextBox();

                var binding = new Binding(propertyName) {Source = Item, ValidatesOnExceptions = true};
                if (propertyInfo.PropertyType == typeof(DateTime)) {
                    binding.StringFormat = "{0:yyyy-MM-dd}";
                } else if (showAsHex) {
                    binding.StringFormat = "0x{0:X}";
                }
                if (!propertyInfo.CanWrite || propertyName.EndsWith("_button")) {
                    binding.Mode       = BindingMode.OneWay;
                    textBox.IsReadOnly = true;
                }

                if (propertyName.EndsWith("_button")) {
                    textBox.PreviewMouseUp += (_, _) => EditSelectedItemId(propertyName);
                }

                textBox.SetBinding(TextBox.TextProperty, binding);

                row.value = textBox;
            }

            rows.Add(row);

#if MHWS
            if (isBitset) bitElementCount++;
            if (bitElementCount > maxBitElement) break;
#endif
        }

        var rowIndex     = 0;
        var shadeThisRow = false;

        foreach (var row in rows.OrderBy(row => row.sortOrder)) {
            grid.RowDefinitions.Add(new() {Height = GridLength.Auto});

            Grid.SetRow(row.name, rowIndex);
            Grid.SetColumn(row.name, 0);
            grid.Children.Add(row.name);
            AddBorder(rowIndex, 0);

            Grid.SetRow(row.value, rowIndex);
            Grid.SetColumn(row.value, 1);
            grid.Children.Add(row.value);
            AddBorder(rowIndex, 1);

            if (shadeThisRow) {
                row.name.Background = AutoDataGrid.ALT_ROW_BRUSH;
                row.value.SetValue(BackgroundProperty, AutoDataGrid.ALT_ROW_BRUSH);
            }
            shadeThisRow = !shadeThisRow;

            rowIndex++;
        }
    }

    private void EditSelectedItemId(string propertyName) {
        var property            = typeof(T).GetProperty(propertyName.Replace("_button", ""), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)!;
        var propertyType        = property.PropertyType;
        var value               = property.GetValue(Item);
        var dataSourceType      = ((DataSourceAttribute) property.GetCustomAttribute(typeof(DataSourceAttribute), true))?.dataType;
        var showAsHex           = (ButtonIdAsHexAttribute) property.GetCustomAttribute(typeof(ButtonIdAsHexAttribute), true) != null;
        var negativeOneForEmpty = (NegativeOneForEmptyAttribute) property.GetCustomAttribute(typeof(NegativeOneForEmptyAttribute), true) != null;
        var dataSource          = Utils.GetDataSourceType(dataSourceType);

        if (negativeOneForEmpty) {
            var newData = new Dictionary<int, string> {[-1] = "<None>"};
#if RE4
            foreach (var (id, name) in DataHelper.ITEM_NAME_LOOKUP[Global.variant][Global.locale]) {
                newData[(int) id] = name;
            }
#endif
            dataSource = newData;
        }

        var getNewItemId = new GetNewItemId(value, dataSource, showAsHex);
        getNewItemId.ShowDialog();

        if (!getNewItemId.Cancelled) {
            property.SetValue(Item, propertyType.IsEnum ? Enum.ToObject(propertyType, getNewItemId.CurrentItem) : Convert.ChangeType(getNewItemId.CurrentItem, propertyType));
            //Item.OnPropertyChanged(propertyName);
        }
    }

    private void OpenGrid(PropertyInfo propertyInfo, string displayName) {
        try {
            var subStructView = new SubStructView(Window.GetWindow(this), displayName, item, propertyInfo);
            subStructView.ShowDialog();
        } catch (NoNullAllowedException err) when (Debugger.IsAttached) {
            MessageBox.Show(err.Message, "Error Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
        } catch (Exception err) when (!Debugger.IsAttached) {
            MainWindow.ShowError(err, "Error Occurred");
        }
    }

    private void AddBorder(int row, int col) {
        const float borderWidth = 0.5f;
        var border = new Border {
            BorderThickness = new(borderWidth, row == 0 ? borderWidth : 0, col == 1 ? borderWidth : 0, borderWidth),
            BorderBrush     = Brushes.Black
        };
        Grid.SetRow(border, row);
        Grid.SetColumn(border, col);
        grid.Children.Add(border);
    }

    public override void Refresh() {
    }

    private class Row {
        public int              sortOrder;
        public TextBlock        name;
        public FrameworkElement value;
    }
}