using System.IO;
using Newtonsoft.Json;
using RE_Editor.Common.Models;

namespace RE_Editor.Common;

public static partial class PathHelper {
    public const string PYTHON38_PATH = @"R:\Games\Monster Hunter Rise\REFramework\reversing\rsz\venv\Scripts\python.exe";

    public const string SUPPORTED_FILES_NAME = "SUPPORTED_FILES.txt";

    public static List<string> GetCachedFileList(FileListCacheType cacheType, bool msg = false) {
        var fileType = cacheType switch {
            FileListCacheType.MSG => $"msg.{Global.MSG_VERSION}",
            FileListCacheType.USER => $"user.{Global.USER_VERSION}",
            _ => throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null)
        };
        var          userFileCache = $@"{CHUNK_PATH}\{cacheType}_{(msg ? "MSG" : "STM")}_FILE_LIST_CACHE.json";
        List<string> allUserFiles;
        if (File.Exists(userFileCache)) {
            var userFileCacheJson = File.ReadAllText(userFileCache);
            allUserFiles = JsonConvert.DeserializeObject<List<string>>(userFileCacheJson)!;
        } else {
            allUserFiles = (from basePath in TEST_PATHS
                            let path = msg ? CHUNK_PATH + basePath.Replace("STM", "MSG") : CHUNK_PATH + basePath
                            where Directory.Exists(path)
                            from file in Directory.EnumerateFiles(path, $"*.{fileType}", SearchOption.AllDirectories)
                            where File.Exists(file)
                            select file).ToList();
            var userFileCacheJson = JsonConvert.SerializeObject(allUserFiles);
            File.WriteAllText(userFileCache, userFileCacheJson);
        }
        return allUserFiles;
    }
}