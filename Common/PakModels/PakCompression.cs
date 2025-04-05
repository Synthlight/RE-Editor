using System.IO;
using System.IO.Compression;
using Zstandard.Net;

namespace RE_Editor.Common.PakModels;

public class PakCompression {
    public static byte[] DecompressDeflate(byte[] buffer) {
        using var deflateStream = new DeflateStream(new MemoryStream(buffer), CompressionMode.Decompress, false);
        var       dstStream     = new MemoryStream();
        deflateStream.CopyTo(dstStream);
        return dstStream.ToArray();
    }

    public static byte[] DecompressZstd(byte[] srcBuffer) {
        using var zStandardStream = new ZstandardStream(new MemoryStream(srcBuffer), CompressionMode.Decompress);
        using var dstStream       = new MemoryStream();
        zStandardStream.CopyTo(dstStream);
        return dstStream.ToArray();
    }
}