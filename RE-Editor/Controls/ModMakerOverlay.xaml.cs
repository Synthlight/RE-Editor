#nullable enable
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using RE_Editor.Common.Models;

namespace RE_Editor.Controls {
    public partial class ModMakerOverlay {
        public ObservableCollection<ProgressInfo> Stuff { get; } = [];

        public ModMakerOverlay() {
            InitializeComponent();
            Stuff.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            Visibility = Stuff.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class ProgressInfo : OnPropertyChangedBase {
        public string  Text          { get; init; } = "";
        public int     ProgressValue { get; set; }
        public int     ProgressMax   { get; set; }
        public string? Context       { get; set; }
    }
}