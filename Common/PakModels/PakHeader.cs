using System.Runtime.InteropServices;

namespace RE_Editor.Common.PakModels;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PakHeader {
    public UInt32 magic; //0x414B504B (KPKA)
    public Byte   majorVersion; // 2 (Kitchen Demo PS4), 4
    public Byte   minorVersion; // 0
    public Int16  feature; // 0, 8 (Encrypted -> PKC)
    public Int32  totalFiles;
    public UInt32 fingerprint;
}