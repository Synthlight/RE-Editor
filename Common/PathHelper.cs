using System.IO;
using Newtonsoft.Json;
using RE_Editor.Common.Models;
using RE_Editor.Common.PakModels;

namespace RE_Editor.Common;

public static partial class PathHelper {
    public const string PYTHON38_PATH = @"R:\Games\Monster Hunter Rise\REFramework\reversing\rsz\.venv\Scripts\python.exe";
    public const string RETOOL_PATH   = @"R:\Games\Monster Hunter Rise\REtool\REtool.exe";

    public const string SUPPORTED_FILES_NAME = "SUPPORTED_FILES.txt";

    public const string BASE_PATH = @"\natives\STM\";

    public static List<string> GetCachedFileList(FileListCacheType cacheType, bool msg = false) {
        var fileType = cacheType switch {
            FileListCacheType.MSG => $"msg.{Global.MSG_VERSION}",
            FileListCacheType.USER => $"user.{Global.USER_VERSION}",
            _ => throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null)
        };
        var          userFileCache = $@"{CHUNK_PATH}\{cacheType}_{(msg ? "MSG" : "STM")}_FILE_LIST_CACHE.json";
        List<string> allFiles;
        if (File.Exists(userFileCache)) {
            var userFileCacheJson = File.ReadAllText(userFileCache);
            allFiles = JsonConvert.DeserializeObject<List<string>>(userFileCacheJson)!;
        } else {
            var path = msg ? CHUNK_PATH + BASE_PATH.Replace("STM", "MSG") : CHUNK_PATH + BASE_PATH;
            allFiles = Directory.Exists(path) ? Directory.EnumerateFiles(path, $"*.{fileType}", SearchOption.AllDirectories).ToList() : [];
            var cachedFileListJson = JsonConvert.SerializeObject(allFiles);
            File.WriteAllText(userFileCache, cachedFileListJson);
        }
        return allFiles;
    }

    public static Dictionary<string, PakData> GetFilesInPaks(FileListCacheType cacheType, bool msg = false) {
        var fileType = cacheType switch {
            FileListCacheType.MSG => $"msg.{Global.MSG_VERSION}",
            FileListCacheType.USER => $"user.{Global.USER_VERSION}",
            _ => throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null)
        };

        var pakDir       = msg ? GAME_PATH_MSG : GAME_PATH;
        var fileToPakMap = new Dictionary<string, PakData>();
        var pakList      = new PakList(msg ? PAK_LIST_MSG : PAK_LIST);
        // Go through in the reverse so the first entry we map is the newest version of the file.
        foreach (var pakFile in PAK_PATHS.Reverse()) {
            var pak     = new PakData(pakList);
            var pakPath = $@"{pakDir}\{pakFile}";
            if (!File.Exists(pakPath)) continue; // Happens when MSG is lagging behind.
            pak.ReadEntries(pakPath);
            // If it's not in the map, it's the newest copy of the file.
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var file in pak.filenameToEntryMap.Keys) {
                if (!file.EndsWith(fileType)) continue;
                fileToPakMap.TryAdd(file, pak);
            }
        }

        return fileToPakMap;
    }

    /**
     * A warning on `ToList()`, it'll store all the read bytes in mem and it might get large.
     */
    public static IEnumerable<FileData> ForEachFileInPaks(FileListCacheType cacheType, bool msg = false) {
        var fileToPakMap = GetFilesInPaks(cacheType, msg);
        var count        = fileToPakMap.Keys.Count;
        Global.Log($"Found {count} paths.");

        var now = DateTime.Now;
        Global.Log("");

        var i          = 0;
        var pakHandles = new Dictionary<PakData, BinaryReader>();
        foreach (var (path, pak) in fileToPakMap) {
            var newNow = DateTime.Now;
            if (newNow > now.AddSeconds(1)) {
                try {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                } catch (Exception) {
                    // Breaks tests so just ignore for those.
                }
                Global.Log($"Parsed {i}/{count}.");
                now = newNow;
            }

            BinaryReader reader;
            if (pakHandles.TryGetValue(pak, out var pakHandle)) {
                reader = pakHandle;
            } else {
                reader          = new(File.Open(pak.pakFile!, FileMode.Open, FileAccess.Read, FileShare.Read));
                pakHandles[pak] = reader;
            }

            var bytes = PakData.ReadEntry(reader, pak.filenameToEntryMap[path]);
            yield return new(path, bytes);

            i++;
        }

        foreach (var pakHandle in pakHandles.Values) {
            pakHandle.Close();
        }

        try {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        } catch (Exception) {
            // Breaks tests so just ignore for those.
        }
        Global.Log($"Parsed {count}/{count}.");
    }

    public static IEnumerable<string> ForEachExtractedFile(FileListCacheType cacheType, bool msg = false) {
        var allUserFiles = GetCachedFileList(FileListCacheType.USER, msg);
        var count        = allUserFiles.Count;
        Global.Log($"Found {count} files.");

        var now = DateTime.Now;
        Global.Log("");

        for (var i = 0; i < allUserFiles.Count; i++) {
            var newNow = DateTime.Now;
            if (newNow > now.AddSeconds(1)) {
                try {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                } catch (Exception) {
                    // Breaks tests so just ignore for those.
                }
                Global.Log($"Parsed {i}/{count}.");
                now = newNow;
            }

            var file = allUserFiles[i];
            yield return file;
        }

        try {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        } catch (Exception) {
            // Breaks tests so just ignore for those.
        }
        Global.Log($"Parsed {count}/{count}.");
    }

    public struct FileData(string path, byte[] bytes) {
        public readonly string path  = path;
        public readonly byte[] bytes = bytes;
    }
}