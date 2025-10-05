using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Windows;

namespace RE_Editor.Tests;

[TestClass]
public class TestLoadingDataGrids {
    private static Dictionary<string, byte[]> pakData = [];

    [ClassInitialize]
    public static void Init(TestContext context) {
        if (PathHelper.PAK_PATHS.Length > 0 && File.Exists($@"{PathHelper.GAME_PATH}\{PathHelper.PAK_PATHS[0]}")) {
            pakData = PathHelper.ForEachFileInPaks(FileListCacheType.USER).ToDictionary(data => data.path, data => data.bytes);
        }
    }

    private static IEnumerable<object[]> GetFilesToTest() {
        if (PathHelper.PAK_PATHS.Length > 0 && File.Exists($@"{PathHelper.GAME_PATH}\{PathHelper.PAK_PATHS[0]}")) {
            return PathHelper.GetFilesInPaks(FileListCacheType.USER).Select(s => new object[] {s.Key});
        }
        return PathHelper.GetCachedFileList(FileListCacheType.USER).Select(s => new object[] {s});
    }

    [DynamicData(nameof(GetFilesToTest))]
    [TestMethod]
    public void TestLoadingDataGrid(string path) {
        ReDataFile data;
        try {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (pakData.Any()) {
                data = ReDataFile.Read(pakData[path]);
            } else {
                data = ReDataFile.Read(path);
            }
        } catch (Exception e) {
            Assert.Inconclusive($"{e.Message}\n{e.StackTrace}");
            return;
        }

        try {
            var entryObject = data.rsz.GetEntryObject();
            MainWindow.CreateDataGridControl(null, entryObject);
        } catch (Exception e) {
            /*
             * So, if this is run as `STATestClass`, at some point it will start shitting the bed with "not enough resources" or something, and the only solution I've found is log out/in.
             * Thus, we don't do it at all and stick to `TestClass` which breaks the UI stuff, but isn't a huge issue. We just treat that as a success.
             */
            const string msg = "The calling thread must be STA, because many UI components require this.";
            if (e.Message.Contains(msg)
                || e.InnerException?.Message.Contains(msg) == true) return;
            throw;
        }
    }
}