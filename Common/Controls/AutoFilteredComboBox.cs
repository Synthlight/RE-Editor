using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace RE_Editor.Common.Controls {
    public class AutoFilteredComboBox : ComboBox {
        // https://stackoverflow.com/a/55190176

        private          string           CurrentFilter = string.Empty;
        private          bool             TextBoxFrozen;
        public           TextBox?         EditableTextBox => GetTemplateChild("PART_EditableTextBox") as TextBox;
        private readonly UserChange<bool> IsDropDownOpenUc;

        public static readonly DependencyProperty MIN_COUNT_FOR_FILTER_PROPERTY = DependencyProperty.Register(nameof(MinCountForFilter), typeof(uint), typeof(AutoFilteredComboBox), new(0u));

        public uint MinCountForFilter {
            get => (uint) GetValue(MIN_COUNT_FOR_FILTER_PROPERTY);
            set => SetValue(MIN_COUNT_FOR_FILTER_PROPERTY, value);
        }

        public AutoFilteredComboBox() {
            IsDropDownOpenUc =  new(v => IsDropDownOpen = v);
            DropDownOpened   += FilteredComboBox_DropDownOpened;

            Loaded += (_, _) => {
                if (EditableTextBox != null) {
                    EditableTextBox.TrackUserChange().UserTextChanged += FilteredComboBox_UserTextChange;
                }
            };
        }

        public void ClearFilter() {
            if (string.IsNullOrEmpty(CurrentFilter)) return;
            CurrentFilter = "";
            CollectionViewSource.GetDefaultView(ItemsSource).Refresh();
        }

        private void FilteredComboBox_DropDownOpened(object? sender, EventArgs e) {
            // If user opens the dropdown show all items.
            if (IsDropDownOpenUc.IsUserChange) ClearFilter();
        }

        private void FilteredComboBox_UserTextChange(object? sender, EventArgs e) {
            if (TextBoxFrozen || EditableTextBox == null) return;
            var tb = EditableTextBox;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (tb.SelectionStart + tb.SelectionLength == tb.Text.Length) {
                CurrentFilter = tb.Text[..tb.SelectionStart].ToLower();
            } else {
                CurrentFilter = tb.Text.ToLower();
            }

            RefreshFilter();
        }

        protected override void OnItemsSourceChanged(IEnumerable? oldValue, IEnumerable? newValue) {
            if (newValue != null) {
                var view = CollectionViewSource.GetDefaultView(newValue);
                view.Filter += FilterItem;
            }

            if (oldValue != null) {
                var view                      = CollectionViewSource.GetDefaultView(oldValue);
                if (view != null) view.Filter -= FilterItem;
            }

            base.OnItemsSourceChanged(oldValue, newValue);
        }

        private void RefreshFilter() {
            if (ItemsSource == null) return;
            if (Text.Length < MinCountForFilter) return;

            var view = CollectionViewSource.GetDefaultView(ItemsSource);
            FreezeTextBoxState(() => {
                var isDropDownOpen = IsDropDownOpen;
                // Always hide because showing it enables the user to pick with up and down keys, otherwise it's not working because of the glitch in view.Refresh().
                IsDropDownOpenUc.Set(false);
                view.Refresh();

                if (!string.IsNullOrEmpty(CurrentFilter) || isDropDownOpen) IsDropDownOpenUc.Set(true);

                if (SelectedItem == null) {
                    foreach (var itm in ItemsSource) {
                        if (itm.ToString() == Text) {
                            SelectedItem = itm;
                            break;
                        }
                    }
                }
            });
        }

        private void FreezeTextBoxState(Action action) {
            if (EditableTextBox == null) return;
            TextBoxFrozen = true;
            var tb       = EditableTextBox;
            var text     = Text;
            var selStart = tb.SelectionStart;
            var selLen   = tb.SelectionLength;
            action();
            Text               = text;
            tb.SelectionStart  = selStart;
            tb.SelectionLength = selLen;
            TextBoxFrozen      = false;
        }

        private bool FilterItem(object? value) {
            if (value == null) return false;
            if (CurrentFilter.Length == 0) return true;

            return value.ToString()?.ToLower().Contains(CurrentFilter) ?? false;
        }
    }

    public class TextBoxBaseUserChangeTracker {
        private bool IsTextInput { get; set; }

        public           TextBoxBase TextBox { get; set; }
        private readonly List<Key>   PressedKeys = [];
        public event EventHandler?   UserTextChanged;

        public TextBoxBaseUserChangeTracker(TextBoxBase textBox) {
            TextBox = textBox;
            var lastText = TextBox.ToString();

            textBox.PreviewTextInput += (_, _) => { IsTextInput = true; };

            textBox.TextChanged += (_, e) => {
                var isUserChange = PressedKeys.Count > 0 || IsTextInput || lastText == TextBox.ToString();
                IsTextInput = false;
                lastText    = TextBox.ToString();
                if (isUserChange) UserTextChanged?.Invoke(this, e);
            };

            textBox.PreviewKeyDown += (_, e) => {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (e.Key) {
                    case Key.Back:
                    case Key.Space:
                    case Key.Delete:
                        if (!PressedKeys.Contains(e.Key)) PressedKeys.Add(e.Key);
                        break;
                }
            };

            textBox.PreviewKeyUp += (_, e) => {
                if (PressedKeys.Contains(e.Key)) PressedKeys.Remove(e.Key);
            };

            textBox.LostFocus += (_, _) => {
                PressedKeys.Clear();
                IsTextInput = false;
            };
        }
    }

    public static class ExtensionMethods {
        public static TextBoxBaseUserChangeTracker TrackUserChange(this TextBoxBase textBox) {
            return new(textBox);
        }
    }

    public class UserChange<T>(Action<T> action) {
        public bool IsUserChange { get; private set; } = true;

        public void Set(T val) {
            try {
                IsUserChange = false;
                action(val);
            } finally {
                IsUserChange = true;
            }
        }
    }
}