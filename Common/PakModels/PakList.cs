using System.IO;
using System.Text;
using RE_Editor.Common.PakModels.Hashing;

namespace RE_Editor.Common.PakModels;

public class PakList {
    private readonly Dictionary<ulong, string> hashList = [];

    public PakList(string listFile) {
        using var projectFile = new StreamReader(listFile);
        while (projectFile.ReadLine() is {} line) {
            ParseLine(line);
        }
    }

    public PakList(string[] listLines) {
        foreach (var line in listLines) {
            ParseLine(line);
        }
    }

    private void ParseLine(string line) {
        var hashLower = GetStringHash(line.ToLower());
        var hashUpper = GetStringHash(line.ToUpper());
        var hash      = (ulong) hashUpper << 32 | hashLower;

        if (!hashList.TryAdd(hash, line)) {
            hashList.TryGetValue(hash, out var collision);

            throw new($"[COLLISION]: {collision} <-> {line}");
        }
    }

    private static uint GetStringHash(string path, uint seed = 0xFFFFFFFF) {
        uint hash   = 0;
        var  buffer = Encoding.Unicode.GetBytes(path);

        using var memoryStream = new MemoryStream(buffer);
        hash = Murmur3.HashCore32(memoryStream, seed);
        return hash;
    }

    public string? GetNameFromHashList(ulong hash) {
        return hashList.GetValueOrDefault(hash);
    }
}