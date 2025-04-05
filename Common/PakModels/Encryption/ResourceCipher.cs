using System.IO;
using System.Numerics;

namespace RE_Editor.Common.PakModels.Encryption;

public static class ResourceCipher {
    private static readonly BigInteger MODULUS = new([
        0x13, 0xD7, 0x9C, 0x89, 0x88, 0x91, 0x48, 0x10, 0xD7, 0xAA, 0x78, 0xAE, 0xF8, 0x59, 0xDF, 0x7D,
        0x3C, 0x43, 0xA0, 0xD0, 0xBB, 0x36, 0x77, 0xB5, 0xF0, 0x5C, 0x02, 0xAF, 0x65, 0xD8, 0x77, 0x03,
        0x00
    ]);

    private static readonly BigInteger EXPONENT = new([
        0xC0, 0xC2, 0x77, 0x1F, 0x5B, 0x34, 0x6A, 0x01, 0xC7, 0xD4, 0xD7, 0x85, 0x2E, 0x42, 0x2B, 0x3B,
        0x16, 0x3A, 0x17, 0x13, 0x16, 0xEA, 0x83, 0x30, 0x30, 0xDF, 0x3F, 0xF4, 0x25, 0x93, 0x20, 0x01,
        0x00
    ]);

    public static byte[] DecryptResource(byte[] lpBuffer) {
        using var memoryReader    = new BinaryReader(new MemoryStream(lpBuffer));
        var       dwOffset        = 0;
        var       dwBlockCount    = (lpBuffer.Length - 8) / 128;
        var       dwDecryptedSize = memoryReader.ReadInt64();

        var lpResult = new byte[dwDecryptedSize + 1];

        for (var i = 0; i < dwBlockCount; i++, dwOffset += 8) {
            var key  = new BigInteger(memoryReader.ReadBytes(64));
            var data = new BigInteger(memoryReader.ReadBytes(64));

            var mod    = BigInteger.ModPow(key, EXPONENT, MODULUS);
            var result = BigInteger.Divide(data, mod);

            var lpDecryptedBlock = result.ToByteArray();

            Array.Copy(lpDecryptedBlock, 0, lpResult, dwOffset, lpDecryptedBlock.Length);
        }

        return lpResult;
    }
}