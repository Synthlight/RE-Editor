using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using RE_Editor.Common.Models;

namespace RE_Editor.Common.Structs;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[TypeConverter(typeof(GuidTypeConverter))]
public class Guid : RszObject, ISimpleViaType {
    public System.Guid Value { get; set; }

    // Needed for struct generation to instance new (unique) GUIDs.
    public static Guid New() {
        return new() {
            Value = System.Guid.NewGuid()
        };
    }

    public void Read(BinaryReader reader) {
        Value = new(reader.ReadBytes(16));
    }

    public void Write(BinaryWriter writer) {
        writer.Write(Value.ToByteArray());
    }

    public Guid Copy() {
        return new() {
            Value = Value
        };
    }

    public override string ToString() {
        return Value.ToString();
    }

    public static implicit operator System.Guid(Guid input) {
        return input.Value;
    }

    public static implicit operator Guid(System.Guid input) {
        return new Guid {Value = input};
    }
}

public class GuidTypeConverter : TypeConverter {
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) {
        return true;
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) {
        return true;
    }

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) {
        return new Guid {Value = System.Guid.Parse((string) value)};
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) {
        return value?.ToString();
    }
}