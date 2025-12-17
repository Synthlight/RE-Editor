using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using RE_Editor.Common;

namespace RE_Editor.Dump_Parser;

public static class Program {
    public const  string BASE_PROJ_PATH = @"..\..\..";
    private const string SCRIPTS_DIR    = $@"{PathHelper.REFRAMEWORK_PATH}\reversing\rsz";
    private const string OUTPUT_DIR     = $@"{BASE_PROJ_PATH}\Dump-Parser\Output\{PathHelper.CONFIG_NAME}";

    public static int Main(string[] args) {
        var mode = ParseArgs(args);
        Directory.CreateDirectory(OUTPUT_DIR);

        if (mode is Mode.PART1 or Mode.ALL) {
            Console.WriteLine("Running part 1...");
            var processStartInfo = new ProcessStartInfo(PathHelper.PYTHON38_PATH) {
                WorkingDirectory = OUTPUT_DIR,
                ArgumentList = {
                    $@"{SCRIPTS_DIR}\emulation-dumper.py",
                    $"--p={PathHelper.EXE_PATH}",
                    $"--il2cpp_path={PathHelper.IL2CPP_DUMP_PATH}",
                }
            };
            Process.Start(processStartInfo)?.WaitForExit();
        }

        if (mode is Mode.PART2 or Mode.ALL) {
            Console.WriteLine("Running part 2...");
            var processStartInfo = new ProcessStartInfo(PathHelper.PYTHON38_PATH) {
                WorkingDirectory = OUTPUT_DIR,
                ArgumentList = {
                    $@"{SCRIPTS_DIR}\non-native-dumper.py",
                    $"--out_postfix={PathHelper.CONFIG_NAME}",
                    $"--il2cpp_path={PathHelper.IL2CPP_DUMP_PATH}",
                    $@"--natives_path={OUTPUT_DIR}\native_layouts_{Path.GetFileName(PathHelper.EXE_PATH)}.json",
                    "--use_typedefs=False",
                    "--use_hashkeys=True",
                    "--include_parents=True",
                }
            };
            Process.Start(processStartInfo)?.WaitForExit();
        }

        if (mode == Mode.R_EASY_RSZ) {
            if (string.IsNullOrWhiteSpace(PathHelper.R_EASY_RSZ)) {
                MessageBox.Show("No URL was given.", "Unable to DL RSZ JSON", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }

            Log($"Pulling: {PathHelper.R_EASY_RSZ}");
            using var    client  = new HttpClient();
            var          json    = client.GetStringAsync(PathHelper.R_EASY_RSZ).Result;
            const string outFile = $@"{OUTPUT_DIR}\rsz{PathHelper.CONFIG_NAME}.json";
            File.WriteAllText(outFile, json);
            Log($"Wrote: {outFile}");
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        return 0;
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static Mode ParseArgs(string[] args) {
        foreach (var arg in args) {
// Pointless since we throw alter anyway.
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
            return arg.ToLower() switch {
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                "all" => Mode.ALL,
                "part1" => Mode.PART1,
                "part2" => Mode.PART2,
                "reasyrsz" => Mode.R_EASY_RSZ
            };
        }
        throw new ArgumentOutOfRangeException(nameof(args));
    }

    private static void Log(string str) {
        Debug.WriteLine(str);
        Console.WriteLine(str);
    }

    private enum Mode {
        ALL,
        PART1,
        PART2,
        R_EASY_RSZ
    }
}