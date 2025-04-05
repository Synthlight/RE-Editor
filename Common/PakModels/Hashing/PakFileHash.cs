using System.Security.Cryptography;

namespace RE_Editor.Common.PakModels.Hashing;

public static class PakFileHash {
    public static string GetChecksum(byte[] bytes) {
        var sha      = SHA256.Create();
        var checksum = sha.ComputeHash(bytes);
        return BitConverter.ToString(checksum).Replace("-", "");
    }
}