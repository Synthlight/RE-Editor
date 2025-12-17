using System.Diagnostics.CodeAnalysis;
using System.IO;
using RE_Editor.Common.Models;

namespace RE_Editor.Common.Structs;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class UInt4 : RszObject, IViaType {
    public uint X { get; set; }
    public uint Y { get; set; }
    public uint Z { get; set; }
    public uint W { get; set; }

    public void Read(BinaryReader reader) {
        X = reader.ReadUInt32();
        Y = reader.ReadUInt32();
        Z = reader.ReadUInt32();
        W = reader.ReadUInt32();
    }

    public void Write(BinaryWriter writer) {
        writer.Write(X);
        writer.Write(Y);
        writer.Write(Z);
        writer.Write(W);
    }

    public UInt4 Copy() {
        return new() {
            X = X,
            Y = Y,
            Z = Z,
            W = W
        };
    }
}