using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using RE_Editor.Common;

namespace RE_Editor;

public static class Extensions {
    public static void AddControl(this Grid grid, UIElement control) {
        var rowDefinition = new RowDefinition {Height = GridLength.Auto};
        grid.RowDefinitions.Add(rowDefinition);
        Grid.SetRow(control, grid.RowDefinitions.Count - 1);
        grid.Children.Add(control);
    }

    public static Visibility VisibleIfTrue(this bool b) {
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public static string ReadNullTermString(this BinaryReader reader) {
        var stringBytes = new List<byte>();
        do {
            stringBytes.Add(reader.ReadByte());
        } while (stringBytes[^1] != 0);

        return Encoding.UTF8.GetString(stringBytes.Subsequence(0, stringBytes.Count).ToArray());
    }

    public static char[] ToNullTermCharArray(this string str) {
        str ??= "\0";
        if (!str.EndsWith("\0")) str += "\0";
        return str.ToCharArray();
    }

    public static List<byte> ReadRemainderAsByteArray(this BinaryReader reader) {
        var list = new List<byte>();
        while (reader.BaseStream.Position < reader.BaseStream.Length) {
            list.Add(reader.ReadByte());
        }
        return list;
    }

    public static void PadTill(this BinaryWriter writer, ulong targetPos) {
        while (writer.BaseStream.Position < (long) targetPos) {
            writer.Write((byte) 0);
        }
    }

    public static void PadTill(this BinaryWriter writer, Func<bool> predicate) {
        while (predicate.Invoke()) {
            writer.Write((byte) 0);
        }
    }

    public static void WriteValueAtOffset(this BinaryWriter writer, ulong value, long whereToWrite) {
        var tempPos = writer.BaseStream.Position;
        writer.BaseStream.Seek(whereToWrite, SeekOrigin.Begin);
        writer.Write(value);
        writer.BaseStream.Seek(tempPos, SeekOrigin.Begin);
    }

    public static void WriteCurrentPositionAtOffset(this BinaryWriter writer, long whereToWrite) {
        var tempPos = writer.BaseStream.Position;
        writer.BaseStream.Seek(whereToWrite, SeekOrigin.Begin);
        writer.Write(tempPos);
        writer.BaseStream.Seek(tempPos, SeekOrigin.Begin);
    }

    /// <summary>
    /// Gets the filename without an MHR extension from an absolute path.
    /// </summary>
    /// <param name="absolutePath"></param>
    /// <returns></returns>
    public static string GetFileNameWithoutExtension(this string absolutePath) {
        var lastSlash       = absolutePath.LastIndexOf('\\');
        var filenameWithExt = absolutePath[(lastSlash + 1)..];
        var allPeriods      = filenameWithExt.AllIndexesOf(".").ToList();
        return allPeriods.Count switch {
            > 1 => filenameWithExt[..allPeriods[^2]],
            1 => filenameWithExt[..allPeriods[^1]],
            _ => filenameWithExt
        };
    }

    public static IEnumerable<int> AllIndexesOf(this string str, string searchString) {
        if (string.IsNullOrEmpty(str)) yield break;
        var minIndex = str.IndexOf(searchString, StringComparison.Ordinal);
        while (minIndex != -1) {
            yield return minIndex;
            minIndex = str.IndexOf(searchString, minIndex + searchString.Length, StringComparison.Ordinal);
        }
    }
}