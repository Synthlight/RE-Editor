using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RE_Editor.Common.Models;
using RE_Editor.Util;

namespace RE_Editor.Windows {
    public partial class SubStructView {
        public SubStructView(Window window, string name, RszObject rszObj, PropertyInfo sourceProperty) {
            InitializeComponent();

            Title  = name;
            Owner  = window;
            Width  = window.Width;
            Height = window.Height * 0.8d;

            Init(rszObj, sourceProperty);
        }

        private void Init(RszObject rszObj, PropertyInfo sourceProperty) {
            var dataGrid = MainWindow.CreateDataGridFromProperty(this, rszObj, sourceProperty);
            dataGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            dataGrid.VerticalScrollBarVisibility   = ScrollBarVisibility.Auto;
            AddChild(dataGrid);
            Utils.SetupKeybind(this, new KeyGesture(Key.Escape), Close);
        }
    }
}