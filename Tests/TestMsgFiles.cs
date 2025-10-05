using RE_Editor.Common;
using RE_Editor.Common.Models;

namespace RE_Editor.Tests;

[TestClass]
public class TestMsgFiles {
    private static Dictionary<string, byte[]> pakData = [];

    [ClassInitialize]
    public static void Init(TestContext context) {
        if (PathHelper.PAK_PATHS.Length > 0 && File.Exists($@"{PathHelper.GAME_PATH}\{PathHelper.PAK_PATHS[0]}")) {
            pakData = PathHelper.ForEachFileInPaks(FileListCacheType.MSG).ToDictionary(data => data.path, data => data.bytes);
        }
    }

    private static IEnumerable<object[]> GetFilesToTest() {
        if (PathHelper.PAK_PATHS.Length > 0 && File.Exists($@"{PathHelper.GAME_PATH}\{PathHelper.PAK_PATHS[0]}")) {
            return PathHelper.GetFilesInPaks(FileListCacheType.MSG).Select(s => new object[] {s.Key});
        }
        return PathHelper.GetCachedFileList(FileListCacheType.MSG).Select(s => new object[] {s});
    }

    [DynamicData(nameof(GetFilesToTest))]
    [TestMethod]
    public void TestReadTextFile(string path) {
        try {
            if (pakData.Any()) {
                MSG.Read(pakData[path]);
            } else {
                MSG.Read(path);
            }
        } catch (FileNotSupported) {
            Assert.Inconclusive();
        }
    }
}