#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;
using RE_Editor.Common.Models;
using RE_Editor.Common.Structs;

namespace RE_Editor.Controls;

public partial class ColoredTextBox : IOnPropertyChanged {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(ColoredTextBox), new(null));

    public Color Color {
        get => (Color) GetValue(ColorProperty);
        set {
            var color = (Color) GetValue(ColorProperty);
            color.RGBA = value.RGBA;
            SetValue(ColorProperty, color);
        }
    }

    public ColoredTextBox() {
        InitializeComponent();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(PropertyChangedEventArgs eventArgs) {
        PropertyChanged?.Invoke(this, eventArgs);
    }

    public void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    public void OnPropertyChanged(IEnumerable<string> propertyName) {
        foreach (var name in propertyName) {
            OnPropertyChanged(name);
        }
    }

    public void OnPropertyChanged(params string[] propertyName) {
        foreach (var name in propertyName) {
            OnPropertyChanged(name);
        }
    }
}