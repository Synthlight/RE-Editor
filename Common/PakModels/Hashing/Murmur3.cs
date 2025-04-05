using System.IO;

namespace RE_Editor.Common.PakModels.Hashing;

public class Murmur3 {
    public static uint HashCore32(Stream stream, uint seed) {
        const uint c1 = 0xcc9e2d51;
        const uint c2 = 0x1b873593;

        var  h1           = seed;
        uint streamLength = 0;

        using (var reader = new BinaryReader(stream)) {
            var chunk = reader.ReadBytes(4);
            while (chunk.Length > 0) {
                streamLength += (uint) chunk.Length;
                uint k1;
                switch (chunk.Length) {
                    case 4:
                        k1 =  (uint) (chunk[0] | chunk[1] << 8 | chunk[2] << 16 | chunk[3] << 24);
                        k1 *= c1;
                        k1 =  Rotl32(k1, 15);
                        k1 *= c2;
                        h1 ^= k1;
                        h1 =  Rotl32(h1, 13);
                        h1 =  h1 * 5 + 0xe6546b64;
                        break;
                    case 3:
                        k1 =  (uint) (chunk[0] | chunk[1] << 8 | chunk[2] << 16);
                        k1 *= c1;
                        k1 =  Rotl32(k1, 15);
                        k1 *= c2;
                        h1 ^= k1;
                        break;
                    case 2:
                        k1 =  (uint) (chunk[0] | chunk[1] << 8);
                        k1 *= c1;
                        k1 =  Rotl32(k1, 15);
                        k1 *= c2;
                        h1 ^= k1;
                        break;
                    case 1:
                        k1 =  (uint) (chunk[0]);
                        k1 *= c1;
                        k1 =  Rotl32(k1, 15);
                        k1 *= c2;
                        h1 ^= k1;
                        break;
                }
                chunk = reader.ReadBytes(4);
            }
        }

        h1 ^= streamLength;
        h1 =  Fmix32(h1);

        return h1;
    }

    public static ulong[] HashCore64(byte[] lpBuffer, uint seed) {
        const ulong c1 = 0x87c37b91114253d5;
        const ulong c2 = 0x4cf5ad432745937f;

        ulong k1 = 0;
        ulong k2 = 0;

        ulong h1 = seed;
        ulong h2 = seed;

        var blocks = (int) lpBuffer.Length / 16;

        for (var i = 0; i < blocks;) {
            k1 = BitConverter.ToUInt64(lpBuffer, i++ * 8);
            k2 = BitConverter.ToUInt64(lpBuffer, i++ * 8);

            k1 *= c1;
            k1 =  Rotl64(k1, 31);
            k1 *= c2;
            h1 ^= k1;

            h1 =  Rotl64(h1, 27);
            h1 += h2;
            h1 =  h1 * 5 + 0x52dce729;

            k2 *= c2;
            k2 =  Rotl64(k2, 33);
            k2 *= c1;
            h2 ^= k2;

            h2 =  Rotl64(h2, 31);
            h2 += h1;
            h2 =  h2 * 5 + 0x38495ab5;
        }

        k1 = 0;
        k2 = 0;

        switch (lpBuffer.Length % 16) {
            case 15:
                k2 ^= ((ulong) lpBuffer[blocks * 16 + 14]) << 48;
                goto case 14;
            case 14:
                k2 ^= ((ulong) lpBuffer[blocks * 16 + 13]) << 40;
                goto case 13;
            case 13:
                k2 ^= ((ulong) lpBuffer[blocks * 16 + 12]) << 32;
                goto case 12;
            case 12:
                k2 ^= ((ulong) lpBuffer[blocks * 16 + 11]) << 24;
                goto case 11;
            case 11:
                k2 ^= ((ulong) lpBuffer[blocks * 16 + 10]) << 16;
                goto case 10;
            case 10:
                k2 ^= ((ulong) lpBuffer[blocks * 16 + 9]) << 8;
                goto case 9;
            case 9:
                k2 ^= ((ulong) lpBuffer[blocks * 16 + 8]) << 0;
                k2 *= c2;
                k2 =  Rotl64(k2, 33);
                k2 *= c1;
                h2 ^= k2;
                goto case 8;
            case 8:
                k1 ^= ((ulong) lpBuffer[blocks * 16 + 7]) << 56;
                goto case 7;
            case 7:
                k1 ^= ((ulong) lpBuffer[blocks * 16 + 6]) << 48;
                goto case 6;
            case 6:
                k1 ^= ((ulong) lpBuffer[blocks * 16 + 5]) << 40;
                goto case 5;
            case 5:
                k1 ^= ((ulong) lpBuffer[blocks * 16 + 4]) << 32;
                goto case 4;
            case 4:
                k1 ^= ((ulong) lpBuffer[blocks * 16 + 3]) << 24;
                goto case 3;
            case 3:
                k1 ^= ((ulong) lpBuffer[blocks * 16 + 2]) << 16;
                goto case 2;
            case 2:
                k1 ^= ((ulong) lpBuffer[blocks * 16 + 1]) << 8;
                goto case 1;
            case 1:
                k1 ^= ((ulong) lpBuffer[blocks * 16]) << 0;
                k1 *= c1;
                k1 =  Rotl64(k1, 31);
                k1 *= c2;
                h1 ^= k1;
                break;
        }
        ;

        h1 ^= (uint) lpBuffer.Length;
        h2 ^= (uint) lpBuffer.Length;

        h1 += h2;
        h2 += h1;

        h1 = Fmix64(h1);
        h2 = Fmix64(h2);

        h1 += h2;
        h2 += h1;
        return [h1, h2];
    }

    // ReSharper disable once IdentifierTypo
    private static uint Rotl32(uint x, byte r) {
        return (x << r) | (x >> (32 - r));
    }

    // ReSharper disable once IdentifierTypo
    private static ulong Rotl64(ulong x, byte r) {
        return (x << r) | (x >> (64 - r));
    }

    // ReSharper disable once IdentifierTypo
    private static uint Fmix32(uint h) {
        h ^= h >> 16;
        h *= 0x85ebca6b;
        h ^= h >> 13;
        h *= 0xc2b2ae35;
        h ^= h >> 16;

        return h;
    }

    // ReSharper disable once IdentifierTypo
    private static ulong Fmix64(ulong k) {
        k ^= k >> 33;
        k *= 0xff51afd7ed558ccd;
        k ^= k >> 33;
        k *= 0xc4ceb9fe1a85ec53;
        k ^= k >> 33;

        return k;
    }

    public static uint GetHash32(byte[] lpBuffer, uint dwSeed = 0xFFFFFFFF) {
        uint dwHash = 0;

        using var memoryStream = new MemoryStream(lpBuffer);
        dwHash = HashCore32(memoryStream, dwSeed);
        return dwHash;
    }

    public static ulong[] GetHash64(byte[] lpBuffer, uint dwSeed = 0xFFFFFFFF) {
        return HashCore64(lpBuffer, dwSeed);
    }
}