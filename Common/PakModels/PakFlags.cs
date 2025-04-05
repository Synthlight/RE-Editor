using System.Diagnostics.CodeAnalysis;

namespace RE_Editor.Common.PakModels;

[Flags]
public enum CompressionFlags : int {
    NONE    = 0,
    DEFLATE = 1,
    ZSTD    = 2,
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum EncryptionFlags : int {
    None         = 0,
    Type_1       = 0x1, // pkc_key::c1n & pkc_key::c1d
    Type_2       = 0x2, // pkc_key::c2n & pkc_key::c2d
    Type_3       = 0x3, // pkc_key::c3n & pkc_key::c3d
    Type_4       = 0x4, // pkc_key::c4n & pkc_key::c4d
    Type_Invalid = 0x5,
}