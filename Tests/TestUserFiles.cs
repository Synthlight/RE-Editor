using System.Diagnostics;
using System.Security.Cryptography;
using RE_Editor.Common;
using RE_Editor.Common.Models;

namespace RE_Editor.Tests;

[TestClass]
public class TestUserFiles {
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
    public void TestReadUserFile(string path) {
        try {
            if (pakData.Any()) {
                ReDataFile.Read(pakData[path]);
            } else {
                ReDataFile.Read(path);
            }
        } catch (FileNotSupported) {
            if (Debugger.IsAttached) throw;
            Assert.Inconclusive();
        }
    }

    [DynamicData(nameof(GetFilesToTest))]
    [TestMethod]
    public void TestWriteUserFile(string path) {
        try {
            ReDataFile data;
            long       sourceLength;
            byte[]     fileHash;
            var        forGp = path.Contains("MSG");

            try {
                if (pakData.Any()) {
                    data         = ReDataFile.Read(pakData[path]);
                    sourceLength = pakData[path].Length;
                    fileHash     = MD5.Create().ComputeHash(pakData[path]);
                } else {
                    data         = ReDataFile.Read(path);
                    sourceLength = new FileInfo(path).Length;
                    fileHash     = MD5.Create().ComputeHash(File.ReadAllBytes(path));
                }
            } catch (Exception e) {
                Assert.Inconclusive($"{e.Message}\n{e.StackTrace}");
                return;
            }


            try {
                using var writer = new BinaryWriter(new MemoryStream());
                data.Write(writer, testWritePosition: true, forGp: forGp);

                var destLength = writer.BaseStream.Length;
                Debug.Assert(sourceLength == destLength, $"Length expected {sourceLength}, found {destLength}.");

                // To byte arrays since MD5 unbelievably takes steam **position** into account.
                var newHash = MD5.Create().ComputeHash(((MemoryStream) writer.BaseStream).ToArray());
                Debug.Assert(fileHash.SequenceEqual(newHash), $"MD5 expected {BitConverter.ToString(fileHash)}, found {BitConverter.ToString(newHash)}.");
            } catch (Exception) {
                if (Debugger.IsAttached) {
                    // Re-read before write because write can cause issues if it fails part way through.
                    if (pakData.Any()) {
                        ReDataFile.Read(pakData[path]).Write(new BinaryWriter(File.OpenWrite($@"O:\Temp\{Path.GetFileName(path)}")), testWritePosition: true, forGp: forGp);
                    } else {
                        ReDataFile.Read(path).Write(new BinaryWriter(File.OpenWrite($@"O:\Temp\{Path.GetFileName(path)}")), testWritePosition: true, forGp: forGp);
                    }
                }
                throw;
            }
        } catch (FileNotSupported) {
            Assert.Inconclusive();
        }
    }
}