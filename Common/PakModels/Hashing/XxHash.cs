namespace RE_Editor.Common.PakModels.Hashing;

public static class XxHash {
    private const uint PRIME32_1 = 0x9E3779B1;
    private const uint PRIME32_2 = 0x85EBCA77;
    private const uint PRIME32_3 = 0xC2B2AE3D;
    private const uint PRIME32_4 = 0x27D4EB2F;
    private const uint PRIME32_5 = 0x165667B1;

    private const ulong PRIME64_1 = 0x9E3779B185EBCA87;
    private const ulong PRIME64_2 = 0xC2B2AE3D27D4EB4F;
    private const ulong PRIME64_3 = 0x165667B19E3779F9;
    private const ulong PRIME64_4 = 0x85EBCA77C2B2AE63;
    private const ulong PRIME64_5 = 0x27D4EB2F165667C5;

    // ReSharper disable once IdentifierTypo
    private static uint Rotl32(uint x, byte r) {
        return (x << r) | (x >> (32 - r));
    }

    // ReSharper disable once IdentifierTypo
    private static ulong Rotl64(ulong x, byte r) {
        return (x << r) | (x >> (64 - r));
    }

    private static uint Round32(uint dwAccumulator, uint dwValue) {
        dwAccumulator += dwValue * PRIME32_2;
        dwAccumulator =  Rotl32(dwAccumulator, 13);
        dwAccumulator *= PRIME32_1;

        return dwAccumulator;
    }

    private static ulong Round64(ulong dwAccumulator, ulong dwValue) {
        dwAccumulator += dwValue * PRIME64_2;
        dwAccumulator =  Rotl64(dwAccumulator, 31);
        dwAccumulator *= PRIME64_1;

        return dwAccumulator;
    }

    private static ulong Merge64(ulong dwAccumulator, ulong dwValue) {
        dwValue       =  Round64(0, dwValue);
        dwAccumulator ^= dwValue;
        dwAccumulator =  dwAccumulator * PRIME64_1 + PRIME64_4;
        return dwAccumulator;
    }

    // ReSharper disable once RedundantAssignment
    private static uint HashCore32(byte[] lpBuffer, uint dwSeed, uint dwHash = 0) {
        var dwOffset = 0;
        var dwEnd    = lpBuffer.Length;

        if (lpBuffer.Length < 16) {
            dwHash = dwSeed + PRIME32_5;
        } else {
            var v1 = dwSeed + PRIME32_1 + PRIME32_2;
            var v2 = dwSeed + PRIME32_2;
            var v3 = dwSeed + 0;
            var v4 = dwSeed - PRIME32_1;

            while (dwOffset <= dwEnd - 16) {
                v1       =  Round32(v1, BitConverter.ToUInt32(lpBuffer, dwOffset));
                dwOffset += 4;
                v2       =  Round32(v2, BitConverter.ToUInt32(lpBuffer, dwOffset));
                dwOffset += 4;
                v3       =  Round32(v3, BitConverter.ToUInt32(lpBuffer, dwOffset));
                dwOffset += 4;
                v4       =  Round32(v4, BitConverter.ToUInt32(lpBuffer, dwOffset));
                dwOffset += 4;
            }

            dwHash = Rotl32(v1, 1) + Rotl32(v2, 7) + Rotl32(v3, 12) + Rotl32(v4, 18);
        }

        dwHash += (uint) lpBuffer.Length;

        while (dwOffset + 4 <= dwEnd) {
            dwHash   += BitConverter.ToUInt32(lpBuffer, dwOffset) * PRIME32_3;
            dwHash   =  Rotl32(dwHash, 17) * PRIME32_4;
            dwOffset += 4;
        }

        while (dwOffset < dwEnd) {
            dwHash   += (uint) (lpBuffer[dwOffset] & 0xFF) * PRIME32_5;
            dwHash   =  Rotl32(dwHash, 11) * PRIME32_1;
            dwOffset += 1;
        }

        dwHash ^= dwHash >> 15;
        dwHash *= PRIME32_2;
        dwHash ^= dwHash >> 13;
        dwHash *= PRIME32_3;
        dwHash ^= dwHash >> 16;

        return dwHash;
    }

    // ReSharper disable once RedundantAssignment
    private static ulong HashCore64(byte[] lpBuffer, ulong dwSeed, ulong dwHash = 0) {
        var dwOffset = 0;
        var dwEnd    = lpBuffer.Length;

        if (lpBuffer.Length < 32) {
            dwHash = dwSeed + PRIME64_5;
        } else {
            var v1 = dwSeed + PRIME64_1 + PRIME64_2;
            var v2 = dwSeed + PRIME64_2;
            var v3 = dwSeed + 0;
            var v4 = dwSeed - PRIME64_1;

            while (dwOffset <= dwEnd - 32) {
                v1       =  Round64(v1, BitConverter.ToUInt64(lpBuffer, dwOffset));
                dwOffset += 8;
                v2       =  Round64(v2, BitConverter.ToUInt64(lpBuffer, dwOffset));
                dwOffset += 8;
                v3       =  Round64(v3, BitConverter.ToUInt64(lpBuffer, dwOffset));
                dwOffset += 8;
                v4       =  Round64(v4, BitConverter.ToUInt64(lpBuffer, dwOffset));
                dwOffset += 8;
            }

            dwHash = Rotl64(v1, 1) + Rotl64(v2, 7) + Rotl64(v3, 12) + Rotl64(v4, 18);

            dwHash = Merge64(dwHash, v1);
            dwHash = Merge64(dwHash, v2);
            dwHash = Merge64(dwHash, v3);
            dwHash = Merge64(dwHash, v4);
        }

        dwHash += (ulong) lpBuffer.Length;

        while (dwOffset + 8 <= dwEnd) {
            dwHash   ^= Round64(0, BitConverter.ToUInt64(lpBuffer, dwOffset));
            dwHash   =  Rotl64(dwHash, 27) * PRIME64_1 + PRIME64_4;
            dwOffset += 8;
        }

        if (dwOffset + 4 <= dwEnd) {
            dwHash   ^= BitConverter.ToUInt32(lpBuffer, dwOffset) * PRIME64_1;
            dwHash   =  Rotl64(dwHash, 23) * PRIME64_2 + PRIME64_3;
            dwOffset += 4;
        }

        while (dwOffset < dwEnd) {
            dwHash   ^= (ulong) (lpBuffer[dwOffset] & 0xFF) * PRIME64_5;
            dwHash   =  Rotl64(dwHash, 11) * PRIME64_1;
            dwOffset += 1;
        }

        dwHash ^= dwHash >> 33;
        dwHash *= PRIME64_2;
        dwHash ^= dwHash >> 29;
        dwHash *= PRIME64_3;
        dwHash ^= dwHash >> 32;

        return dwHash;
    }


    public static uint GetHash32(byte[] lpBuffer, uint dwSeed = 0xFFFFFFFF) {
        return HashCore32(lpBuffer, dwSeed);
    }

    public static ulong GetHash64(byte[] lpBuffer, ulong dwSeed = 0xFFFFFFFF) {
        return HashCore64(lpBuffer, dwSeed);
    }
}