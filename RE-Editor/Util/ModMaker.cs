#nullable enable
using System;
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
using RE_Editor.Windows;

namespace RE_Editor.Util;

public static class ModMaker {
    /// <summary>
    /// Creates mod folders and archives.
    /// </summary>
    /// <param name="mainWindow">The main window.</param>
    /// <param name="mods">All the mods to make under the root name `<param name="modFolderName"/>`.</param>
    /// <param name="modFolderName">Name without any path or `\` characters. Illegal characters will be replaced.</param>
    /// <param name="copyLooseToFluffy">If true, will copy the *loose* zip to FMM.</param>
    /// <param name="copyPakToFluffy">If true, will copy the *pak* zip to FMM.</param>
    /// <param name="noPakZip">Will skip the second zip containing pak files if true.</param>
    /// <param name="workingDir">Will build the mod here and copy the output zip to the normal dir.</param>
    public static void WriteMods<T>(MainWindow mainWindow, IEnumerable<T> mods, string modFolderName, bool copyLooseToFluffy = false, bool copyPakToFluffy = false, bool noPakZip = false, string? workingDir = null) where T : INexusMod {
#if DD2
        var usedLuaFiles = new List<string>();
#elif MHWS
        var usedLuaFiles = new List<string>();
#endif
        var estimatedThreadCount = 0;
        var threadHandler        = new ThreadHandler();
        var bundles              = new Dictionary<string, List<T>>();

        foreach (var mod in mods) {
            estimatedThreadCount++;
#if DD2
            switch (mod) {
                case ItemDbTweak tweak:
                    if (usedLuaFiles.Contains(tweak.LuaName)) throw new DuplicateNameException($"Lua file `{tweak.LuaName}` already created.");
                    threadHandler.AddWorker($"WriteTweak: {tweak.LuaName}", () => { ItemDbTweakWriter.WriteTweak(tweak, modFolderName); });
                    usedLuaFiles.Add(tweak.LuaName);
                    break;
                case SwapDbTweak tweak:
                    if (usedLuaFiles.Contains(tweak.LuaName)) throw new DuplicateNameException($"Lua file `{tweak.LuaName}` already created.");
                    threadHandler.AddWorker($"WriteTweak: {tweak.LuaName}", () => { SwapDbTweakWriter.WriteTweak(tweak, modFolderName); });
                    usedLuaFiles.Add(tweak.LuaName);
                    break;
            }
#elif MHWS
            switch (mod) {
                case VariousDataTweak tweak:
                    if (usedLuaFiles.Contains(tweak.LuaName)) throw new DuplicateNameException($"Lua file `{tweak.LuaName}` already created.");
                    threadHandler.AddWorker($"WriteTweak: {tweak.LuaName}", () => { VariousDataWriter.WriteTweak(tweak, modFolderName); });
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

        threadHandler.AddEstimatedDoCount(estimatedThreadCount);

        foreach (var (bundleName, entries) in bundles) {
            threadHandler.AddWorker($"CreateModBundle: {bundleName}", () => { CreateModBundle(threadHandler, modFolderName, copyLooseToFluffy, copyPakToFluffy, bundleName, entries, noPakZip, workingDir); });
        }

        mainWindow.ShowThreadProgress(threadHandler, modFolderName);
    }

    private static void CreateModBundle<T>(ThreadHandler threadHandler, string modFolderName, bool copyLooseToFluffy, bool copyPakToFluffy, string? bundleName, List<T> entries, bool noPakZip, string? workingDir) where T : INexusMod {
        bundleName = bundleName == "" ? null : bundleName;
        var safeBundleName    = bundleName?.ToSafeName();
        var safeModFolderName = modFolderName.ToSafeName();
        var rootPath          = $@"{workingDir ?? PathHelper.MODS_PATH}\{safeModFolderName}";
        var destRootPath      = $@"{PathHelper.MODS_PATH}\{safeModFolderName}";
        var modFiles          = new List<string>();
        var nativesFiles      = new List<string>();
        var pakFiles          = new List<string>();
        var listLock          = new Mutex();
        var zipPathNormal     = $@"{rootPath}\{safeBundleName ?? safeModFolderName}.zip";
        var zipPathPak        = $@"{rootPath}\{safeBundleName ?? safeModFolderName} (PAK).zip";

        if (File.Exists(zipPathNormal)) File.Delete(zipPathNormal);
        if (File.Exists(zipPathPak)) File.Delete(zipPathPak);

        List<Thread>                   localThreads    = [];
        Dictionary<ReDataFile, byte[]> reDataFileCache = [];

        foreach (var mod in entries) {
            localThreads.Add(threadHandler.AddWorker(mod.Name, () => {
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
                if (mod.NameAsBundle != null) {
                    modInfo.WriteLine($"NameAsBundle={bundleName}");
                }
                if (mod.AddonFor != null) {
                    modInfo.WriteLine($"AddonFor={mod.AddonFor}");
                }
                if (mod.Requirement != null) {
                    modInfo.WriteLine($"Requirement={mod.Requirement}");
                }

                var anyIncluded = false;
                if (mod.Files.Any()) {
                    if (mod.Action == null && mod.FilteredAction == null) {
                        throw new InvalidDataException("`mod.Action` or `mod.FilteredAction` are null but `mod.Files` is not empty.");
                    }
                    foreach (var modFile in mod.Files) {
                        var sourceFile = @$"{PathHelper.CHUNK_PATH}\{modFile}";
                        var outFile    = @$"{modPath}\{modFile}";

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
                            anyIncluded = true;
                            Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);
                            dataFile.Write(outFile, forGp: mod.ForGp);
                            lock (listLock) {
                                nativesFiles.Add(outFile);
                            }
                        }
                    }
                }

                if (mod.AdditionalFiles?.Any() == true) {
                    foreach (var (dest, obj) in mod.AdditionalFiles) {
                        var fixedDest = dest;
                        if (fixedDest.StartsWith('\\') || fixedDest.StartsWith('/')) {
                            fixedDest = dest[1..];
                        }
                        var outFile = @$"{modPath}\{dest}";
                        Directory.CreateDirectory(Path.GetDirectoryName(outFile)!);
                        switch (obj) {
                            case CustomCopy customCopy:
                                customCopy.DoCopy(outFile);
                                break;
                            case string sourceFile:
                                File.Copy(sourceFile, outFile);
                                break;
                            case byte[] bytes:
                                using (var file = new StreamWriter(File.Open(outFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))) {
                                    file.Write($"-- {mod.NameOverride ?? mod.Name}\n" +
                                               "-- By LordGregory\n\n" +
                                               $"local version = \"{mod.Version}\"\n" +
                                               $"log.info(\"Initializing `{mod.NameOverride ?? mod.Name}` v\" .. version)\n\n");
                                    file.Flush();
                                    file.BaseStream.Write(bytes);
                                }
                                break;
                            case ReDataFile reDataFile:
                                if (!reDataFileCache.ContainsKey(reDataFile)) {
                                    using var memoryStream = new MemoryStream();
                                    reDataFile.Write(new BinaryWriter(memoryStream));
                                    var bytes = memoryStream.ToArray();
                                    lock (listLock) {
                                        reDataFileCache[reDataFile] = bytes;
                                    }
                                }
                                using (var file = new BinaryWriter(File.Open(outFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))) {
                                    //reDataFile.Write(file);
                                    byte[] bytes;
                                    lock (listLock) {
                                        bytes = reDataFileCache[reDataFile];
                                    }
                                    file.Write(bytes);
                                    file.Flush();
                                }
                                break;
                            default:
                                throw new InvalidDataException($"Source LUA data is of an unsupported type: {obj.GetType()}");
                        }
                        anyIncluded = true;
                        if (fixedDest.StartsWith("natives")) {
                            lock (listLock) {
                                nativesFiles.Add(outFile);
                            }
                        } else {
                            lock (listLock) {
                                modFiles.Add(outFile); // Because it's basically anything NOT a pak since we can't mix those two types.
                            }
                        }
                    }
                }

                // If the mod has literally no files outside of metadata, just delete the folder and continue.
                if (!anyIncluded && !mod.AlwaysInclude) {
                    Directory.Delete(modPath, true);
                    return;
                }

                // Do late so we can skip the whole thing if all the files are filtered out.
                if (mod.Image != null && File.Exists(mod.Image)) {
                    var imageFileName = Path.GetFileName(mod.Image);
                    var imagePath     = @$"{modPath}\{imageFileName}";
                    modInfo.WriteLine($"screenshot={imageFileName}");
                    File.Copy(mod.Image, imagePath);
                    lock (listLock) {
                        modFiles.Add(imagePath);
                    }
                }
                var modInfoPath = @$"{modPath}\modinfo.ini";
                File.WriteAllText(modInfoPath, modInfo.ToString());
                lock (listLock) {
                    modFiles.Add(modInfoPath);
                }

                if (mod.SkipPak) return;
                var processStartInfo = new ProcessStartInfo(@"R:\Games\Monster Hunter Rise\REtool\REtool.exe", $"{Global.PAK_CREATE_ARGS} -c \"{modPath}\"") {
                    WorkingDirectory = $@"{modPath}\..",
                    CreateNoWindow   = true
                };
                Process.Start(processStartInfo)?.WaitForExit();
                var pakFile = $@"{modPath}\{safeName}.pak";
                File.Move($@"{modPath}.pak", pakFile);
                lock (listLock) {
                    pakFiles.Add(pakFile);
                }
                Debug.WriteLine($"Wrote: {mod.Name}");
            }, addToCount: false));
        }
        localThreads.WaitAll();
        localThreads.Clear();

        localThreads.Add(threadHandler.AddWorker($"Compress Bundle (Loose): {bundleName}", () => { CompressTheMod(zipPathNormal, modFiles, nativesFiles, copyLooseToFluffy); }));
        if (!noPakZip) {
            localThreads.Add(threadHandler.AddWorker($"Compress Bundle (PAK): {bundleName}", () => { CompressTheMod(zipPathPak, modFiles, pakFiles, copyPakToFluffy); }));
        }
        localThreads.WaitAll();

        if (workingDir != null) {
            Directory.CreateDirectory(destRootPath);
            if (File.Exists(zipPathNormal)) File.Copy(zipPathNormal, zipPathNormal.Replace(rootPath, destRootPath), true);
            if (File.Exists(zipPathPak)) File.Copy(zipPathPak, zipPathPak.Replace(rootPath, destRootPath), true);
        }
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

    private static void WaitAll(this List<Thread> threads) {
        foreach (var thread in threads) {
            thread.Join();
        }
    }

    public class CustomCopy(string sourceFile, Action<string, string> doCopy) {
        public readonly string                 sourceFile = sourceFile;
        public readonly Action<string, string> doCopy     = doCopy;

        public void DoCopy(string destFile) {
            doCopy.Invoke(sourceFile, destFile);
        }
    }
}