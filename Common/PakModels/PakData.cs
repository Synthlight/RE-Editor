using System.IO;
using RE_Editor.Common.PakModels.Encryption;

namespace RE_Editor.Common.PakModels;

public class PakData(PakList pakList) {
    private         int                          entrySize;
    private         List<PakEntry>               entryTable         = [];
    public readonly Dictionary<string, PakEntry> filenameToEntryMap = [];
    public          string?                      pakFile { get; private set; }

    public void ReadEntries(string pakFile) {
        this.pakFile = pakFile;
        using var pakStream = new BinaryReader(File.OpenRead(pakFile));
        if (pakStream.BaseStream.Length <= 16) return;

        var header = pakStream.Read<PakHeader>();

        if (header.magic != 0x414B504B) throw new("[ERROR]: Invalid magic of PAK archive file");

        if (header.majorVersion != 2 && header.majorVersion != 4 || header.minorVersion != 0 && header.minorVersion != 1 && header.minorVersion != 2) throw new($"[ERROR]: Invalid version of PAK archive file -> {header.majorVersion}.{header.minorVersion}, expected 2.0, 4.0, 4.1, 4.2");

        if ((header.feature & ~PakFeatureFlags.KnownFlags) != 0) throw new("[ERROR]: Archive is using unknown flags");

        entrySize = header.majorVersion switch {
            2 => 24,
            4 => 48,
            _ => entrySize
        };

        if ((header.feature & PakFeatureFlags.ExtraInteger) != 0) {
            pakStream.BaseStream.Seek(4, SeekOrigin.Current);
        }

        if ((header.feature & PakFeatureFlags.ExtraData) != 0) {
            pakStream.BaseStream.Seek(9, SeekOrigin.Current);
        }

        var decryptedEntryData = pakStream.ReadBytes(header.totalFiles * entrySize);

        if ((header.feature & PakFeatureFlags.RemapEntries) != 0) {
            var remapCount = pakStream.ReadInt64();
            pakStream.BaseStream.Seek(remapCount * 16, SeekOrigin.Current);
        }

        if ((header.feature & ~PakFeatureFlags.EncryptEntryList) != 0) {
            var encryptedKey = pakStream.ReadBytes(128);
            decryptedEntryData = PakCipher.DecryptData(decryptedEntryData, encryptedKey);
        }

        entryTable = [];
        using var entryReader = new BinaryReader(new MemoryStream(decryptedEntryData));
        for (var i = 0; i < header.totalFiles; i++) {
            var entry = new PakEntry();

            if (header is {majorVersion: 2, minorVersion: 0}) {
                entry.offset           = entryReader.ReadInt64();
                entry.decompressedSize = entryReader.ReadInt64();
                entry.hashNameLower    = entryReader.ReadUInt32();
                entry.hashNameUpper    = entryReader.ReadUInt32();
                entry.compressedSize   = 0;
                entry.compressionType  = 0;
                entry.checksum         = 0;
            } else if (header is {majorVersion: 4, minorVersion: 0} || header.minorVersion == 1 || header.minorVersion == 2) {
                entry.hashNameLower    = entryReader.ReadUInt32();
                entry.hashNameUpper    = entryReader.ReadUInt32();
                entry.offset           = entryReader.ReadInt64();
                entry.compressedSize   = entryReader.ReadInt64();
                entry.decompressedSize = entryReader.ReadInt64();
                entry.attributes       = entryReader.ReadInt64();
                entry.checksum         = entryReader.ReadUInt64();
                entry.compressionType  = (CompressionFlags) (entry.attributes & 0xF);
                entry.encryptionType   = (EncryptionFlags) ((entry.attributes & 0x00FF0000) >> 16);
            } else {
                throw new("[ERROR]: Something is wrong when reading the entry table");
            }

            entryTable.Add(entry);
        }

        foreach (var entry in entryTable) {
            var fileName = pakList.GetNameFromHashList((ulong) entry.hashNameUpper << 32 | entry.hashNameLower);
            if (fileName != null) {
                filenameToEntryMap[fileName] = entry;
            }
        }
    }

    public static byte[] ReadEntry(BinaryReader reader, PakEntry entry) {
        reader.BaseStream.Seek(entry.offset, SeekOrigin.Begin);
        switch (entry.compressionType) {
            case CompressionFlags.NONE:
                return ReadEntry(reader, entry.offset, entry.compressedSize);
            case CompressionFlags.DEFLATE:
            case CompressionFlags.ZSTD:
                reader.BaseStream.Seek(entry.offset, SeekOrigin.Begin);
                var srcBuffer = reader.ReadBytes((int) entry.compressedSize);

                if (entry.encryptionType != EncryptionFlags.None && entry.encryptionType <= EncryptionFlags.Type_Invalid) {
                    srcBuffer = ResourceCipher.DecryptResource(srcBuffer);
                }

                return entry.compressionType switch {
                    CompressionFlags.DEFLATE => PakCompression.DecompressDeflate(srcBuffer),
                    CompressionFlags.ZSTD => PakCompression.DecompressZstd(srcBuffer),
                    _ => null! // Unreachable.
                };
            default: throw new("[ERROR]: Unknown compression id detected -> " + entry.compressionType.ToString());
        }
    }

    private static byte[] ReadEntry(BinaryReader reader, long position, long size) {
        var chunks = ReadChunks(reader, position, size);

        return chunks.SelectMany(i => i).ToArray();
    }

    private static List<byte[]> ReadChunks(BinaryReader reader, long position, long size) {
        const int MAX_CHUNK = 1048576;

        var chunk        = new byte[MAX_CHUNK];
        var remainSize   = size;
        var bufferLength = MAX_CHUNK;

        List<byte[]> chunks = [];

        reader.BaseStream.Seek(position, SeekOrigin.Begin);

        while (true) {
            if (remainSize <= MAX_CHUNK) {
                chunk        = new byte[remainSize];
                bufferLength = (int) remainSize;
            }

            int numBytes;
            if ((numBytes = reader.Read(chunk, 0, bufferLength)) > 0) {
                remainSize -= bufferLength;
            } else {
                break;
            }

            var temp = new byte[numBytes];
            Array.Copy(chunk, temp, numBytes);

            chunks.Add(temp);
        }

        return chunks;
    }
}