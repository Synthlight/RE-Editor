using System.Diagnostics.CodeAnalysis;
using System.IO;
using RE_Editor.Common.Models;

namespace RE_Editor.Common.Structs;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class KeyFrame : RszObject, IViaType {
    public float Value      { get; set; }
    public short CurveType  { get; set; }
    public Half  Time       { get; set; }
    public Half  InNormalY  { get; set; }
    public Half  InNormalX  { get; set; }
    public Half  OutNormalY { get; set; }
    public Half  OutNormalX { get; set; }

    public void Read(BinaryReader reader) {
        Value      = reader.ReadSingle();
        CurveType  = reader.ReadInt16();
        Time       = reader.ReadHalf();
        InNormalY  = reader.ReadHalf();
        InNormalX  = reader.ReadHalf();
        OutNormalY = reader.ReadHalf();
        OutNormalX = reader.ReadHalf();
    }

    public void Write(BinaryWriter writer) {
        writer.Write(Value);
        writer.Write(CurveType);
        writer.Write(Time);
        writer.Write(InNormalY);
        writer.Write(InNormalX);
        writer.Write(OutNormalY);
        writer.Write(OutNormalX);
    }

    public KeyFrame Copy() {
        return new() {
            Value      = Value,
            CurveType  = CurveType,
            Time       = Time,
            InNormalY  = InNormalY,
            InNormalX  = InNormalX,
            OutNormalY = OutNormalY,
            OutNormalX = OutNormalX
        };
    }
}