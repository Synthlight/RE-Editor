using System.Runtime.InteropServices;

namespace RE_Editor.Common.PakModels;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PakEntry {
    public UInt32           hashNameLower;
    public UInt32           hashNameUpper;
    public Int64            offset;
    public Int64            compressedSize;
    public Int64            decompressedSize;
    public Int64            attributes;
    public UInt64           checksum;
    public CompressionFlags compressionType { get; set; }
    public EncryptionFlags  encryptionType  { get; set; }
}