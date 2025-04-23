using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Common.PakModels;
using RE_Editor.Common.PakModels.Hashing;

namespace RE_Editor.ID_Parser;

public static class ObsoleteMapMaker {
    public const string BASE_PROJ_PATH   = @"..\..\..";
    public const string STRUCT_JSON_PATH = $@"{BASE_PROJ_PATH}\Dump-Parser\Output\{PathHelper.CONFIG_NAME}\rsz{PathHelper.CONFIG_NAME}.json";

    private static readonly Regex EXTENSION = new(@"^.*?\.([^\.]+)\.\d+$");

    public static void Go() {
        BuildCrcDump();
        BuildHashJson();
    }

    private static void BuildCrcDump() {
        // Because it's so much easier than writing whatever is needed to deserialize dictionary keys as hex string->uint.
        var structJson = JsonConvert.DeserializeObject<Dictionary<string, StructJson>>(File.ReadAllText(STRUCT_JSON_PATH))!.KeyFromHexString();

        Dictionary<uint, uint> validHashToCrcMap = [];
        foreach (var (hash, @struct) in structJson) {
            validHashToCrcMap[hash] = @struct.crc;
        }
        Directory.CreateDirectory(Program.DETECTOR_ASSETS_DIR);
        File.WriteAllText($@"{Program.DETECTOR_ASSETS_DIR}\HASH_TO_CRC_MAP.json", JsonConvert.SerializeObject(validHashToCrcMap, Formatting.Indented));
    }

    private static void BuildHashJson() {
        var allFilesByPak = GetAllCoveredFilesByPak();
        var fileInfoMap   = new Dictionary<string, GoodPakInfo>();

        // So we go through with latest PAK files being first.
        var allFilesMapKeys = allFilesByPak.Keys.OrderByDescending(key => key).ToList();
        foreach (var pakFile in allFilesMapKeys) {
            var pakFileInfos = allFilesByPak[pakFile];

            foreach (var pakFileInfo in pakFileInfos) {
                // If it's there, then we're dealing with an outdated file.
                if (fileInfoMap.TryGetValue(pakFileInfo.filename, out var fileInfo)) {
                    fileInfo.badPakInfo.Add(pakFileInfo);
                } else {
                    // If it's not in the map, then, because we are going through with the newest first, it must be the latest version of the file.
                    fileInfoMap[pakFileInfo.filename] = new(pakFileInfo.filename, pakFile, pakFileInfo);
                }
            }
        }

        Dictionary<string, InfoByHash> obsoleteFileByHash = [];
        foreach (var (filename, goodPakInfo) in fileInfoMap) {
            foreach (var badPakInfo in goodPakInfo.badPakInfo) {
                obsoleteFileByHash[badPakInfo.hash] = new(filename, goodPakInfo.knownGoodPak, badPakInfo.pak, badPakInfo.length, goodPakInfo.goodPakInfo.length == badPakInfo.length);
            }
        }
        // Sort so the results don't change every time.
        obsoleteFileByHash = obsoleteFileByHash.Sort(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
        Directory.CreateDirectory(Program.ASSETS_DIR);
        File.WriteAllText($@"{Program.ASSETS_DIR}\OBSOLETE_BY_HASH.json", JsonConvert.SerializeObject(obsoleteFileByHash, Formatting.Indented));

        // Sort so the results don't change every time.
        // Also make the keys lowercase to make checking easier.
        fileInfoMap = fileInfoMap.Sort(pair => pair.Key).ToDictionary(pair => pair.Key.ToLower(), pair => pair.Value);
        Directory.CreateDirectory(Program.DETECTOR_ASSETS_DIR);
        File.WriteAllText($@"{Program.DETECTOR_ASSETS_DIR}\GOOD_PAK_MAP.json", JsonConvert.SerializeObject(fileInfoMap, Formatting.Indented));
    }

    private static Dictionary<string, List<PakFileInfo>> GetAllCoveredFilesByPak() {
        var pakList           = new PakList(PathHelper.PAK_LIST);
        var allFilesByPak     = new Dictionary<string, List<PakFileInfo>>();
        var allFilesByPakLock = new Mutex();
        var countdownEvents   = new List<CountdownEvent>();

        foreach (var pakFile in PathHelper.PAK_PATHS) {
            var pak     = new PakData(pakList);
            var pakPath = $@"{PathHelper.GAME_PATH}\{pakFile}";
            pak.ReadEntries(pakPath);

            List<ToHash> fileToHash = [];
            using var    pakStream  = new BinaryReader(File.Open(pakPath, FileMode.Open, FileAccess.Read, FileShare.Read));

            foreach (var (filename, entry) in pak.filenameToEntryMap) {
                var extension = EXTENSION.Match(filename).Groups[1].Value;
                if (!PathHelper.OBSOLETE_TYPES_TO_CHECK.ContainsIgnoreCase(extension)) continue;

                fileToHash.Add(new(filename, entry, PakData.ReadEntry(pakStream, entry)));
            }

            var countdownEvent = new CountdownEvent(fileToHash.Count);
            countdownEvents.Add(countdownEvent);
            foreach (var toHash in fileToHash) {
                ThreadPool.QueueUserWorkItem(_ => {
                    lock (allFilesByPakLock) {
                        if (!allFilesByPak.ContainsKey(pakFile)) allFilesByPak[pakFile] = [];
                    }

                    var fileInfo = new PakFileInfo(toHash.filename, PakFileHash.GetChecksum(toHash.bytes), toHash.entry.decompressedSize, pakFile);
                    lock (allFilesByPakLock) {
                        allFilesByPak[pakFile].Add(fileInfo);
                    }
                    countdownEvent.Signal();
                });
            }
        }

        foreach (var countdownEvent in countdownEvents) {
            countdownEvent.Wait();
            countdownEvent.Dispose();
        }

        return allFilesByPak;
    }

    private struct ToHash(string filename, PakEntry entry, byte[] bytes) {
        public readonly string   filename = filename;
        public readonly PakEntry entry    = entry;
        public readonly byte[]   bytes    = bytes;
    }
}