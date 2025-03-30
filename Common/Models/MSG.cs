using System.Diagnostics.CodeAnalysis;
using System.IO;
#pragma warning disable CS8618

namespace RE_Editor.Common.Models;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public class MSG {
    private static readonly byte[] KEY = [207, 206, 251, 248, 236, 10, 51, 102, 147, 169, 29, 147, 80, 57, 95, 9];

    public uint        version;
    public uint        magic;
    public ulong       headerOffset;
    public uint        subCount;
    public uint        typeCount;
    public uint        langCount;
    public uint        zero;
    public ulong       data1Offset;
    public ulong       data2Offset;
    public ulong       langOffset;
    public ulong       typeOffset;
    public ulong       typenameOffset;
    public byte[]      data1;
    public ulong[]     subOffsets;
    public SubEntry[]? subEntries;
    public ulong       zeroOffset;
    public int[]       languages;
    public int[]       typeIds;
    public byte[]      unknown;
    public ulong[]     typeNameStringOffsets;
    public string[]    typeNames;
    public TypeEntry[] types;

    public static MSG Read(string targetFile, bool writeNameIds = false) {
        Global.Log($"Reading: {targetFile}");

        var       file   = new MSG();
        using var reader = new BinaryReader(File.OpenRead(targetFile));
        file.version        = reader.ReadUInt32();
        file.magic          = reader.ReadUInt32();
        file.headerOffset   = reader.ReadUInt64();
        file.subCount       = reader.ReadUInt32();
        file.typeCount      = reader.ReadUInt32();
        file.langCount      = reader.ReadUInt32();
        file.zero           = reader.ReadUInt32();
        file.data1Offset    = reader.ReadUInt64();
        file.data2Offset    = reader.ReadUInt64();
        file.langOffset     = reader.ReadUInt64();
        file.typeOffset     = reader.ReadUInt64();
        file.typenameOffset = reader.ReadUInt64();
        var position = reader.BaseStream.Position;

        reader.BaseStream.Seek((long) file.data1Offset, SeekOrigin.Begin);
        file.data1 = reader.ReadBytes((int) (reader.BaseStream.Length - (long) file.data1Offset));

        if (file.data1.Length == 0) return file;

        Decrypt(file.data1);
        using var decryptedStream = new BinaryReader(new MemoryStream(file.data1));

        reader.BaseStream.Position = position;

        file.subOffsets = new ulong[file.subCount];
        file.subEntries = new SubEntry[file.subCount];
        for (var subIndex = 0; subIndex < file.subCount; subIndex++) {
            var subOffset = reader.ReadUInt64();
            file.subOffsets[subIndex] = subOffset;
            file.subEntries[subIndex] = SubEntry.Read(file, reader, decryptedStream, subOffset);
        }

        file.zeroOffset = reader.ReadUInt64();

        file.languages = new int[file.langCount];
        for (var langIndex = 0; langIndex < file.langCount; langIndex++) {
            file.languages[langIndex] = reader.ReadInt32();
        }

        file.unknown = reader.ReadBytes((int) (reader.BaseStream.Position % 8));

        file.typeIds = new int[file.typeCount];
        for (var typeIndex = 0; typeIndex < file.typeCount; typeIndex++) {
            file.typeIds[typeIndex] = reader.ReadInt32();
        }

        file.unknown = reader.ReadBytes((int) (reader.BaseStream.Position % 8));

        file.typeNameStringOffsets = new ulong[file.typeCount];
        file.typeNames             = new string[file.typeCount];
        for (var typeIndex = 0; typeIndex < file.typeCount; typeIndex++) {
            var offset = reader.ReadUInt64();
            file.typeNameStringOffsets[typeIndex] = offset;
            file.typeNames[typeIndex]             = ReadWString(file, decryptedStream, offset);
        }

        file.types = new TypeEntry[file.subCount];
        for (var subIndex = 0; subIndex < file.subCount; subIndex++) {
            file.types[subIndex] = file.subEntries[subIndex].type;
        }

        if (writeNameIds) {
            foreach (var entry in file.subEntries) {
                Global.Log(entry.first);
            }
        }

        return file;
    }

    private static void Decrypt(IList<byte> data) {
        byte b    = 0;
        var  num  = 0;
        var  num2 = 0;
        do {
            var b2 = b;
            b = data[num2];
            var num3 = num++ & 15;
            data[num2] = (byte) (b2 ^ b ^ KEY[num3]);
            num2       = num;
        } while (num < data.Count);
    }

    private static void Encrypt(IList<byte> data) {
        byte b    = 0;
        var  num  = 0;
        var  num2 = 0;
        do {
            var b2   = data[num2];
            var num3 = num++ & 15;
            data[num2] = (byte) (b2 ^ b ^ KEY[num3]);
            b          = data[num2];
            num2       = num;
        } while (num < data.Count);
    }

    public static string ReadWString(MSG file, BinaryReader decryptedStream, ulong offset) {
        var position = decryptedStream.BaseStream.Position;

        decryptedStream.BaseStream.Position = (long) (offset - file.data1Offset);
        var result = decryptedStream.ReadNullTermWString();

        decryptedStream.BaseStream.Position = position;
        return result;
    }

    public Dictionary<uint, string> GetIdMap(Global.LangIndex lang, uint type, bool startAtOne, uint idBaseNum) {
        if (subEntries == null) return [];
        var dict = new Dictionary<uint, string>(subEntries.Length);
        for (var i = startAtOne ? 1 : 0; i < subEntries.Length; i++) {
            var id   = idBaseNum + (type | (uint) (i - (startAtOne ? 1 : 0)));
            var text = subEntries[i].refs[(int) lang];
            if (text == "") continue;
            SetText(text, dict, id);
        }
        return dict;
    }

    public Dictionary<Global.LangIndex, Dictionary<uint, string>> GetLangIdMap(uint type, bool startAtOne, uint idBaseNum = 0) {
        var dict = new Dictionary<Global.LangIndex, Dictionary<uint, string>>(Global.LANGUAGES.Count);
        foreach (var lang in Global.LANGUAGES) {
            dict[lang] = GetIdMap(lang, type, startAtOne, idBaseNum);
        }
        return dict;
    }

    public Dictionary<T, string> GetIdMap<T>(Global.LangIndex lang, Func<string, Result<T>> parseName) where T : notnull {
        if (subEntries == null) return [];
        var dict = new Dictionary<T, string>(subEntries.Length);
        foreach (var entry in subEntries) {
            var name   = entry.first.Replace("_Name", "").Replace("_Explain", "");
            var result = parseName(name);
            if (result.skip) continue;
            var id   = result.data;
            var text = entry.refs[(int) lang];
            if (text == "") continue;
            SetText(text, dict, id);
        }
        return dict;
    }

    public Dictionary<Global.LangIndex, Dictionary<T, string>> GetLangIdMap<T>(Func<string, Result<T>> parseName) where T : notnull {
        var dict = new Dictionary<Global.LangIndex, Dictionary<T, string>>(Global.LANGUAGES.Count);
        foreach (var lang in Global.LANGUAGES) {
            dict[lang] = GetIdMap(lang, parseName);
        }
        return dict;
    }

    public Dictionary<T, string> GetRawMap<T>(Global.LangIndex lang, Func<SubEntry, Result<T>> parseName) where T : notnull {
        if (subEntries == null) return [];
        var dict = new Dictionary<T, string>(subEntries.Length);
        foreach (var entry in subEntries) {
            var result = parseName(entry);
            if (result.skip) continue;
            var id   = result.data;
            var text = entry.refs[(int) lang];
            if (text == "") continue;
            SetText(text, dict, id);
        }
        return dict;
    }

    public Dictionary<Global.LangIndex, Dictionary<T, string>> GetLangRawMap<T>(Func<SubEntry, Result<T>> parseName) where T : notnull {
        var dict = new Dictionary<Global.LangIndex, Dictionary<T, string>>(Global.LANGUAGES.Count);
        foreach (var lang in Global.LANGUAGES) {
            dict[lang] = GetRawMap(lang, parseName);
        }
        return dict;
    }

    public Dictionary<Guid, string> GetGuidMap(Global.LangIndex lang) {
        if (subEntries == null) return [];
        var dict = new Dictionary<Guid, string>(subEntries.Length);
        foreach (var entry in subEntries) {
            var id   = entry.id1;
            var text = entry.refs[(int) lang];
            if (text == "") continue;
            SetText(text, dict, id);
        }
        return dict;
    }

    public Dictionary<Global.LangIndex, Dictionary<Guid, string>> GetLangGuidMap() {
        var dict = new Dictionary<Global.LangIndex, Dictionary<Guid, string>>(Global.LANGUAGES.Count);
        foreach (var lang in Global.LANGUAGES) {
            dict[lang] = GetGuidMap(lang);
        }
        return dict;
    }

    private static void SetText<T>(string text, IDictionary<T, string> dict, T id) {
        if (text.Contains("#Rejected#")) text = "#Rejected#";
        text = text.Replace("\r\n", " ");
        text = text.Replace("\n", " ");
        text = text.Replace("\r", " ");
        // Because sometimes the rejected entry would overwrite the good entry.
        if (dict.ContainsKey(id) && text == "#Rejected#") return;
        dict[id] = text;
    }

    public class SubEntry {
        public Guid      id1;
        public uint      id2;
        public uint      index;
        public ulong     firstOffset;
        public ulong     typeOffset;
        public ulong[]   strOffsets;
        public TypeEntry type;
        public string    first;
        public string[]  refs;

        public static SubEntry Read(MSG file, BinaryReader reader, BinaryReader decryptedStream, ulong subOffset) {
            var subEntry = new SubEntry();
            var position = reader.BaseStream.Position;
            reader.BaseStream.Position = (long) subOffset;
            subEntry.id1               = new(reader.ReadBytes(16));
            subEntry.id2               = reader.ReadUInt32();
            subEntry.index             = reader.ReadUInt32();
            subEntry.firstOffset       = reader.ReadUInt64();
            subEntry.typeOffset        = reader.ReadUInt64();
            subEntry.strOffsets        = new ulong[file.langCount];
            for (var strIndex = 0; strIndex < file.langCount; strIndex++) {
                subEntry.strOffsets[strIndex] = reader.ReadUInt64();
            }

            reader.BaseStream.Seek((long) subEntry.typeOffset, SeekOrigin.Begin);
            subEntry.type  = TypeEntry.Read(file, reader, decryptedStream);
            subEntry.first = ReadWString(file, decryptedStream, subEntry.firstOffset);
            subEntry.refs  = new string[file.langCount];
            for (var langIndex = 0; langIndex < file.langCount; langIndex++) {
                subEntry.refs[langIndex] = ReadWString(file, decryptedStream, subEntry.strOffsets[langIndex]);
            }

            reader.BaseStream.Position = position;
            return subEntry;
        }

        public override string ToString() {
            return refs[(int) Global.LangIndex.eng];
        }
    }

    public class TypeEntry {
        public ulong[]   offsets;
        public string?[] strings;

        public static TypeEntry Read(MSG file, BinaryReader reader, BinaryReader decryptedStream) {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var typeEntry = new TypeEntry();
            typeEntry.offsets = new ulong[file.typeCount];
            typeEntry.strings = new string[file.typeCount];
            for (var index = 0; index < file.typeCount; index++) {
                var offset = reader.ReadUInt64();
                typeEntry.offsets[index] = offset;
                typeEntry.strings[index] = typeEntry.offsets[index] < file.data1Offset ? null : ReadWString(file, decryptedStream, offset);
            }
            return typeEntry;
        }
    }

    public struct Result<T>(T data, bool skip) where T : notnull {
        public readonly T    data = data;
        public readonly bool skip = skip;

        public Result(T data) : this(data, false) {
        }

        public static implicit operator Result<T>(T someValue) {
            return new(someValue);
        }
    }
}