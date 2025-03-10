#nullable enable
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;

namespace RE_Editor.Util;

public static class ModMaker {
    /// <summary>
    /// Creates mod folders and archives.
    /// </summary>
    /// <param name="mods">All the mods to make under the root name `<param name="modFolderName"/>`.</param>
    /// <param name="modFolderName">Name without any path or `\` characters. Illegal characters will be replaced.</param>
    /// <param name="copyLooseToFluffy">If true, will copy the *loose* zip to FMM.</param>
    /// <param name="copyPakToFluffy">If true, will copy the *pak* zip to FMM.</param>
    /// <param name="noPakZip">Will skip the second zip containing pak files if true.</param>
    public static void WriteMods<T>(IEnumerable<T> mods, string modFolderName, bool copyLooseToFluffy = false, bool copyPakToFluffy = false, bool noPakZip = false) where T : INexusMod {
#if DD2
        var usedLuaFiles = new List<string>();
#elif MHWS
        var usedLuaFiles = new List<string>();
#endif
        var bundles = new Dictionary<string, List<T>>();
        foreach (var mod in mods) {
#if DD2
            switch (mod) {
                case ItemDbTweak tweak:
                    if (usedLuaFiles.Contains(tweak.LuaName)) throw new DuplicateNameException($"Lua file `{tweak.LuaName}` already created.");
                    ItemDbTweakWriter.WriteTweak(tweak, modFolderName);
                    usedLuaFiles.Add(tweak.LuaName);
                    break;
                case SwapDbTweak tweak:
                    if (usedLuaFiles.Contains(tweak.LuaName)) throw new DuplicateNameException($"Lua file `{tweak.LuaName}` already created.");
                    SwapDbTweakWriter.WriteTweak(tweak, modFolderName);
                    usedLuaFiles.Add(tweak.LuaName);
                    break;
            }
#elif MHWS
            switch (mod) {
                case VariousDataTweak tweak:
                    if (usedLuaFiles.Contains(tweak.LuaName)) throw new DuplicateNameException($"Lua file `{tweak.LuaName}` already created.");
                    VariousDataWriter.WriteTweak(tweak, modFolderName);
                    usedLuaFiles.Add(tweak.LuaName);
                    break;
            }
#endif

            var bundle = mod.NameAsBundle ?? "";
            if (!bundles.ContainsKey(bundle)) {
                bundles[bundle] = [];
            }
            bundles[bundle].Add(mod);
        }

        var threads = new List<Thread>();
        foreach (var (bundleName, entries) in bundles) {
            var thread = new Thread(() => { CreateModBundle(modFolderName, copyLooseToFluffy, copyPakToFluffy, bundleName, entries, noPakZip); });
            threads.Add(thread);
            thread.Start();
        }
        threads.JoinAll();
    }

    private static void CreateModBundle<T>(string modFolderName, bool copyLooseToFluffy, bool copyPakToFluffy, string? bundleName, List<T> entries, bool noPakZip) where T : INexusMod {
        bundleName = bundleName == "" ? null : bundleName;
        var safeBundleName    = bundleName?.ToSafeName();
        var safeModFolderName = modFolderName.ToSafeName();
        var rootPath          = $@"{PathHelper.MODS_PATH}\{safeModFolderName}";
        var modFiles          = new List<string>();
        var nativesFiles      = new List<string>();
        var pakFiles          = new List<string>();
        var zipPathNormal     = $@"{rootPath}\{safeBundleName ?? safeModFolderName}.zip";
        var zipPathPak        = $@"{rootPath}\{safeBundleName ?? safeModFolderName} (PAK).zip";

        if (File.Exists(zipPathNormal)) File.Delete(zipPathNormal);
        if (File.Exists(zipPathPak)) File.Delete(zipPathPak);

        foreach (var mod in entries) {
            var    safeName = mod.Name.ToSafeName();
            string modPath;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (safeBundleName != null) {
                modPath = $@"{rootPath}\{safeBundleName}\{safeName}";
            } else {
                modPath = $@"{rootPath}\{safeName}";
            }

            if (Directory.Exists(modPath)) Directory.Delete(modPath, true);
            Directory.CreateDirectory(modPath);

            var modInfo = new StringWriter();
            modInfo.WriteLine($"name={mod.NameOverride ?? mod.Name}");
            modInfo.WriteLine($"version={mod.Version}");
            modInfo.WriteLine($"description={mod.Desc}");
            modInfo.WriteLine("author=LordGregory");
            if (mod.Image != null && File.Exists(mod.Image)) {
                var imageFileName = Path.GetFileName(mod.Image);
                var imagePath     = @$"{modPath}\{imageFileName}";
                modInfo.WriteLine($"screenshot={imageFileName}");
                File.Copy(mod.Image, imagePath);
                modFiles.Add(imagePath);
            }
            if (mod.NameAsBundle != null) {
                modInfo.WriteLine($"NameAsBundle={bundleName}");
            }
            if (mod.AddonFor != null) {
                modInfo.WriteLine($"AddonFor={mod.AddonFor}");
            }
            var modInfoPath = @$"{modPath}\modinfo.ini";
            File.WriteAllText(modInfoPath, modInfo.ToString());
            modFiles.Add(modInfoPath);

            if (mod.Files.Any()) {
                if (mod.Action == null && mod.FilteredAction == null) {
                    throw new InvalidDataException("`mod.Action` or `mod.FilteredAction` are null but `mod.Files` is not empty.");
                }
                foreach (var modFile in mod.Files) {
                    var sourceFile = @$"{PathHelper.CHUNK_PATH}\{modFile}";
                    var outFile    = @$"{modPath}\{modFile}";
                    Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);

                    var dataFile = ReDataFile.Read(sourceFile);
                    dataFile.Write(new BinaryWriter(new MemoryStream()), testWritePosition: true, forGp: mod.ForGp);

                    var data        = dataFile.rsz.objectData;
                    var includeFile = true;
                    if (mod.FilteredAction != null) {
                        includeFile = mod.FilteredAction.Invoke(data);
                    } else {
                        mod.Action!.Invoke(data);
                    }
                    if (includeFile) {
                        dataFile.Write(outFile, forGp: mod.ForGp);
                        nativesFiles.Add(outFile);
                    }
                }
            }

            if (mod.AdditionalFiles?.Any() == true) {
                foreach (var (dest, sourceFile) in mod.AdditionalFiles) {
                    var outFile = @$"{modPath}\{dest}";
                    Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);
                    File.Copy(sourceFile, outFile);
                    if (dest.StartsWith("natives")) {
                        nativesFiles.Add(outFile);
                    } else {
                        modFiles.Add(outFile); // Because it's basically anything NOT a pak since we can't mix those two types.
                    }
                }
            }

            if (mod.SkipPak) continue;
            var processStartInfo = new ProcessStartInfo(@"R:\Games\Monster Hunter Rise\REtool\REtool.exe", $"{Global.PAK_CREATE_ARGS} -c \"{modPath}\"") {
                WorkingDirectory = $@"{modPath}\..",
                CreateNoWindow   = true
            };
            Process.Start(processStartInfo)?.WaitForExit();
            var pakFile = $@"{modPath}\{safeName}.pak";
            File.Move($@"{modPath}.pak", pakFile);
            pakFiles.Add(pakFile);
        }

        var threads = new List<Thread> {new(() => { CompressTheMod(zipPathNormal, modFiles, nativesFiles, copyLooseToFluffy); })};
        if (!noPakZip) {
            threads.Add(new(() => { CompressTheMod(zipPathPak, modFiles, pakFiles, copyPakToFluffy); }));
        }
        threads.StartAll();
        threads.JoinAll();
    }

    private static void CompressTheMod(string zipPath, List<string> baseFiles, List<string> gameFiles, bool copyToFluffy) {
        DoZip(zipPath, baseFiles, gameFiles);
        if (copyToFluffy) {
            Directory.CreateDirectory(PathHelper.FLUFFY_MODS_PATH);
            File.Copy(zipPath, $@"{PathHelper.FLUFFY_MODS_PATH}\{Path.GetFileName(zipPath)}", true);
        }
    }

    private static void DoZip(string zipPath, List<string> baseFiles, List<string> gameFiles) {
        var parentDir = Directory.GetParent(zipPath);
        var zipFile   = new FileInfo(zipPath);

        using var zip = zipFile.Exists ? new(zipPath) : ZipFile.Create(zipPath);
        zip.BeginUpdate();

        foreach (var file in baseFiles) {
            var relativePath = file.Replace($@"{parentDir}\", "");
            zip.Add(file, relativePath);
        }
        foreach (var file in gameFiles) {
            var relativePath = file.Replace($@"{parentDir}\", "");
            zip.Add(file, relativePath);
        }

        zip.CommitUpdate();
    }

    public static string ToSafeName(this string s) {
        return s.Replace('/', '-')
                .Replace('\\', '-')
                .Replace(':', '-')
                .Replace('?', '？');
    }

    private static void StartAll(this List<Thread> threads) {
        foreach (var thread in threads) {
            thread.Start();
        }
    }

    private static void JoinAll(this List<Thread> threads) {
        foreach (var thread in threads) {
            thread.Join();
        }
    }
}