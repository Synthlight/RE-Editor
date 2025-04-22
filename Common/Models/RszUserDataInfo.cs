using System.Diagnostics.CodeAnalysis;
using System.IO;

#pragma warning disable CS8618

namespace RE_Editor.Common.Models;

[SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
public class RszUserDataInfo {
    public  int    instanceId;
    public  uint   hash;
    public  string str;
    private ulong  tempOffsetPos; // Temp spot to place the position we write this object so we can update the offset later after we write the string.

    public static RszUserDataInfo Read(BinaryReader reader, RSZ rsz) {
        var userDataInfo = new RszUserDataInfo();
        userDataInfo.instanceId = reader.ReadInt32();
        userDataInfo.hash       = reader.ReadUInt32();
        var stringOffset = reader.ReadUInt64();
        userDataInfo.str = reader.ReadNullTermWString(rsz.position + (long) stringOffset);
        return userDataInfo;
    }

    public void Write(BinaryWriter writer) {
        if (instanceId == -1) throw new("Error: InstanceId is -1.");
        writer.Write(instanceId);
        writer.Write(hash);
        tempOffsetPos = (ulong) writer.BaseStream.Position;
        writer.Write(0ul);
    }

    public void UpdateWrite(BinaryWriter writer, ulong rszOffset) {
        writer.WriteValueAtOffset((ulong) (writer.BaseStream.Position - (long) rszOffset), (long) tempOffsetPos);
        writer.WriteNullTermWString(str);
    }

    public override string ToString() {
        return str;
    }
}