using RE_Editor.Common;
using TrxFileParser;

namespace RE_Editor.Test_Result_Parser;

public static class Program {
    public const  string BASE_PROJ_PATH    = @"..\..\..";
    private const string TEST_RESULTS_PATH = $@"{BASE_PROJ_PATH}\TestResults\{PathHelper.CONFIG_NAME}\Results.trx";
    private const string ASSETS_DIR        = $@"{BASE_PROJ_PATH}\RE-Editor\Data\{PathHelper.CONFIG_NAME}\Assets";

    public static void Main() {
        var          results  = TrxDeserializer.Deserialize(TEST_RESULTS_PATH);
        const string testName = "TestWriteUserFile";

        var paths  = new List<string>(results.Results.UnitTestResults.Count);
        var passed = 0;
        var failed = 0;
        var total  = 0;
        foreach (var result in results.Results.UnitTestResults) {
            if (!result.TestName.StartsWith(testName)) continue;

            if (result.Outcome == "Passed") {
                passed++;
                var name = result.TestName[$"{testName} (".Length..^1]
                                 .Replace($@"{PathHelper.CHUNK_PATH}\", "")
                                 .Replace('/', '\\')
                                 .ToLower();
                if (name.StartsWith('"')) name = name[1..];
                if (name.EndsWith('"')) name   = name[..^1];
                paths.Add(name);
            } else {
                failed++;
            }
            total++;
        }

        using var assetFile  = File.CreateText($@"{ASSETS_DIR}\{PathHelper.SUPPORTED_FILES_NAME}");
        using var outputFile = File.CreateText(TEST_RESULTS_PATH.Replace("Results.trx", "Supported Files.txt"));

        paths.Sort();

        foreach (var path in paths) {
            assetFile.WriteLine(path);
            outputFile.WriteLine(path);
        }

        var percent = (int) ((float) passed / total * 100);
        Console.WriteLine($"Passed: {passed}\n" +
                          $"Failed: {failed}\n" +
                          $"Total: {total}\n\n" +
                          $"Passing: {percent}%\n");

        File.WriteAllText(TEST_RESULTS_PATH.Replace("Results.trx", "Pass Percent.txt"), $"{percent}");

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}