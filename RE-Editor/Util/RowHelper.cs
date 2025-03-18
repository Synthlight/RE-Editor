using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Common.Models.List_Wrappers;
using RE_Editor.Controls;

namespace RE_Editor.Util;

public class RowHelper<T> where T : notnull {
    private readonly AutoDataGridGeneric<T> dataGrid;
    private readonly RszObject              rszObj;
    private readonly PropertyInfo           prop;

    private RowHelper([CanBeNull] UIElement control, AutoDataGridGeneric<T> dataGrid, RszObject rszObj, PropertyInfo prop) {
        this.dataGrid = dataGrid;
        this.rszObj   = rszObj;
        this.prop     = prop;

        if (control == null) return;

        Utils.SetupKeybind(control, new KeyGesture(Key.I, ModifierKeys.Control), HandleAddRow);
        Utils.SetupKeybind(control, new KeyGesture(Key.R, ModifierKeys.Control), HandleRemoveRow);
    }

    public static void AddKeybinds(UIElement control, AutoDataGridGeneric<T> dataGrid, RszObject rszObj, PropertyInfo prop) {
        _ = new RowHelper<T>(control, dataGrid, rszObj, prop);
    }

    private void HandleAddRow() {
        try {
            var newItem         = CreateInstance();
            var observableItems = dataGrid.Items;
            var originalItems   = prop.GetGetMethod()!.Invoke(rszObj, null)!;

            if (observableItems != originalItems) {
                var listType       = prop.PropertyType.GenericTypeArguments[0];
                var collectionType = typeof(Collection<>).MakeGenericType(listType);
                var addMethod      = collectionType.GetMethod(nameof(Collection<T>.Add), Global.FLAGS)!; // TODO: Remove the `T` generic param once C# 14 come out.
                try {
                    addMethod.Invoke(originalItems, [Convert.ChangeType(newItem, listType)]);
                } catch (InvalidCastException e) {
                    if (e.Message == "Object must implement IConvertible.") {
                        // Seems to happen in cases where it's a reference type, and we're casting to our parent.
                        // e.g. Casting from `App_user_data_ItemShopData_cData` -> `Ace_user_data_ExcelUserData_cData`.
                        // So just try it as-is.
                        addMethod.Invoke(originalItems, [newItem]);
                    } else {
                        throw;
                    }
                }
            }
            observableItems.Add(newItem);
            dataGrid.ScrollIntoView(newItem);
        } catch (Exception e) when (!Debugger.IsAttached) {
            MessageBox.Show($"Error adding a new row: {e}", "Error Adding Rows", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void HandleRemoveRow() {
        try {
            var observableItems = dataGrid.Items;
            var originalItems   = prop.GetGetMethod()!.Invoke(rszObj, null)!;
            var selectedItems   = dataGrid.SelectedItems.Cast<T>().ToList();

            // Because what `SelectedItems` returns can differ depending on *how* things were selected.
            foreach (var cellItem in dataGrid.SelectedCells.Select(cell => cell.Item)) {
                if (selectedItems.Contains((T) cellItem)) continue;
                selectedItems.Add((T) cellItem);
            }

            foreach (var selectedItem in selectedItems) {
                if (observableItems != originalItems) {
                    var listType       = prop.PropertyType.GenericTypeArguments[0];
                    var collectionType = typeof(Collection<>).MakeGenericType(listType);
                    var removeMethod   = collectionType.GetMethod(nameof(Collection<T>.Remove), Global.FLAGS)!;
                    try {
                        removeMethod.Invoke(originalItems, [Convert.ChangeType(selectedItem, listType)]);
                    } catch (InvalidCastException e) {
                        if (e.Message == "Object must implement IConvertible.") {
                            // Seems to happen in cases where it's a reference type, and we're casting to our parent.
                            // e.g. Casting from `App_user_data_ItemShopData_cData` -> `Ace_user_data_ExcelUserData_cData`.
                            // So just try it as-is.
                            removeMethod.Invoke(originalItems, [selectedItem]);
                        } else {
                            throw;
                        }
                    }
                }
                observableItems.Remove(selectedItem);
            }
        } catch (Exception e) when (!Debugger.IsAttached) {
            MessageBox.Show($"Error adding a new row: {e}", "Error Adding Rows", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private T CreateInstance() {
        if (typeof(T).GetNameWithoutGenericArity() == typeof(GenericWrapper<>).GetNameWithoutGenericArity()) {
            // Here, `T` is `GenericWrapper<*>`.
            var    innerType          = typeof(T).GenericTypeArguments[0];
            var    genericWrapperType = typeof(GenericWrapper<>).MakeGenericType(innerType);
            object itemValue;
            if (innerType.IsValueType) {
                itemValue = Convert.ChangeType(0, innerType);
            } else if (innerType == typeof(string)) {
                itemValue = "";
            } else {
                // Should never happen.
                throw new NotImplementedException($"Unknown value for type, not sure how to instance it: {innerType}");
            }
            return (T) Activator.CreateInstance(genericWrapperType, [-1, itemValue])!;
        } else if (typeof(T).IsAssignableTo(typeof(RszObject))) {
            var createMethod = typeof(T).GetMethod("Create", BindingFlags.Public | BindingFlags.Static, [typeof(RSZ)])!;
            return (T) createMethod.Invoke(null, [rszObj.rsz])!;
        } else {
            throw new NotImplementedException($"Unable to determine how to instance `{typeof(T)}` to add a row.");
        }
    }
}