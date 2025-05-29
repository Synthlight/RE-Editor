using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Common.PakModels;
using RE_Editor.Common.PakModels.Hashing;
using RE_Editor.Obsolete_Detector.Data;
using RE_Editor.Obsolete_Detector.Models;
using Utils = RE_Editor.Common.Util.Utils;

namespace RE_Editor.Obsolete_Detector;

public partial class MainWindow {
// @formatter:off (Because it screws up the alignment of inactive sections.)
#if DD2
    public const string INITIAL_DIRECTORY = @"C:\Program Files (x86)\Steam\steamapps\common\Dragons Dogma 2";
    public const string FMM_GAME_FOLDER   = "DragonsDogma2";
#elif DRDR
    public const string INITIAL_DIRECTORY = @"C:\Program Files (x86)\Steam\steamapps\common\DRDR";
    public const string FMM_GAME_FOLDER   = "";
#elif MHR
    public const string INITIAL_DIRECTORY = @"C:\Program Files (x86)\Steam\steamapps\common\MonsterHunterRise";
    public const string FMM_GAME_FOLDER   = "MHRISE";
#elif MHWS
    public const string INITIAL_DIRECTORY = @"C:\Program Files (x86)\Steam\steamapps\common\MonsterHunterWilds";
    public const string FMM_GAME_FOLDER   = "MonsterHunterWilds";
#elif RE2
    public const string INITIAL_DIRECTORY = @"C:\Program Files (x86)\Steam\steamapps\common\RESIDENT EVIL 2  BIOHAZARD RE2";
    public const string FMM_GAME_FOLDER   = "";
#elif RE3
    public const string INITIAL_DIRECTORY = @"C:\Program Files (x86)\Steam\steamapps\common\RE3";
    public const string FMM_GAME_FOLDER   = "";
#elif RE4
    public const string INITIAL_DIRECTORY = @"C:\Program Files (x86)\Steam\steamapps\common\RESIDENT EVIL 4  BIOHAZARD RE4";
    public const string FMM_GAME_FOLDER   = "";
#elif RE8
    public const string INITIAL_DIRECTORY = @"C:\Program Files (x86)\Steam\steamapps\common\Resident Evil Village BIOHAZARD VILLAGE";
    public const string FMM_GAME_FOLDER   = "";
#endif
// @formatter:on

    public MainWindow() {
        InitializeComponent();
        main_text.Text = $"This detects mods that are outdated for the current version of {PathHelper.CONFIG_NAME}.\n" +
                         $"Currently designed for {Global.CURRENT_GAME_VERSION}. If you have a newer version of {PathHelper.CONFIG_NAME}, please update this program.\n" +
                         "\n" +
                         "This isn't perfect. It may miss something obsolete, it may have false positives. YMMV\n" +
                         "If you still have a blackscreen after, test without mods altogether.\n" +
                         "\n" +
                         "The short version of how it works: Checks file modification dates, lengths, and hashes, against known good and bad data.\n" +
                         "If you're installing an outdated mod after a big update, it might slip by due to modification dates.\n" +
                         "If you want more detail, read the Nexus page.\n" +
                         "\n" +
                         "Select game install folder (or any folder containing a `natives` folder or PAK files) and press scan to begin.";
        txt_path.Text = ObsoleteData.SETTINGS.GamePath ?? INITIAL_DIRECTORY;
        Utils.SetupKeybind(txt_path, new KeyGesture(Key.Enter), EnterKeyPressed);

        fmm_text.Text = "Optional. If set, this will look through active mods to try and detect which mod is the source of detected files.";
        fmm_path.Text = ObsoleteData.SETTINGS.FmmPath ?? @"C:\";
        Utils.SetupKeybind(fmm_path, new KeyGesture(Key.Enter), EnterKeyPressed);

        txt_path.Focus();
    }

    private void Browse_OnClick(object sender, RoutedEventArgs e) {
        try {
            var initialDirectory = txt_path.Text;
            if (!Directory.Exists(initialDirectory)) {
                initialDirectory = "C:\\";
            }
            var ofdResult = new CommonOpenFileDialog {
                IsFolderPicker   = true,
                InitialDirectory = initialDirectory,
                Multiselect      = false
            };
            var result = ofdResult.ShowDialog();
            if (result == CommonFileDialogResult.Ok) {
                txt_path.Text = ofdResult.FileName;
            }
        } catch (Exception err) when (!Debugger.IsAttached) {
            ShowError(err);
        }
    }

    private void BrowseFmm_OnClick(object sender, RoutedEventArgs e) {
        try {
            var initialDirectory = fmm_path.Text;
            if (!Directory.Exists(initialDirectory)) {
                initialDirectory = "C:\\";
            }
            var ofdResult = new OpenFileDialog {
                Filter           = "ModManager.exe|ModManager.exe",
                InitialDirectory = initialDirectory,
                Multiselect      = false
            };
            var result = ofdResult.ShowDialog();
            if (result == true) {
                fmm_path.Text = Path.GetDirectoryName(ofdResult.FileName)!;
            }
        } catch (Exception err) when (!Debugger.IsAttached) {
            ShowError(err);
        }
    }

    private async void Scan_OnClick(object? sender, RoutedEventArgs? e) {
        try {
            btn_scan.Visibility = Visibility.Hidden;
            progress.Visibility = Visibility.Visible;

            await DoScan();
        } catch (Exception err) when (!Debugger.IsAttached) {
            ShowError(err);
        } finally {
            btn_scan.Visibility = Visibility.Visible;
            progress.Visibility = Visibility.Hidden;
        }
    }

    private Task DoScan() {
        return Task.Run(() => {
            var gamePath         = GetOnUiThread(() => txt_path.Text);
            var fmmPath          = GetOnUiThread(() => fmm_path.Text);
            var nativesDir       = gamePath.PathCombine("natives");
            var obsoleteFileData = new List<ObsoleteFileData>();
            var fluffyMods       = ReadFmmMods(fmmPath);
            var fmmModByFiles    = new Dictionary<string, string>();

            if (!Directory.Exists(gamePath)) {
                MessageBox.Show("The entered folder cannot be found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ObsoleteData.SETTINGS.GamePath = gamePath;
            ObsoleteData.SETTINGS.FmmPath  = fmmPath;

            foreach (var (mod, files) in fluffyMods) {
                foreach (var file in files) {
                    fmmModByFiles[file] = mod;
                }
            }

            if (Directory.Exists(nativesDir)) {
                foreach (var type in PathHelper.OBSOLETE_TYPES_TO_CHECK) {
                    foreach (var file in Directory.EnumerateFiles(nativesDir, $"*.{type}.*", SearchOption.AllDirectories)) {
                        var checkPath = file.Replace(gamePath, "").Replace('\\', '/').ToLower();
                        if (checkPath.StartsWith('/')) {
                            checkPath = checkPath.Substring(1, checkPath.Length - 1);
                        }
                        if (ObsoleteData.GOOD_PAK_MAP.TryGetValue(checkPath, out var goodPakInfo)) {
                            var     modifiedDate = new FileInfo(file).LastWriteTime;
                            var     bytes        = File.ReadAllBytes(file);
                            var     length       = bytes.Length;
                            string? modName      = null;
                            if (fmmModByFiles.TryGetValue(checkPath, out var mod)) {
                                modName = mod;
                            }
                            CheckFile(file, bytes, goodPakInfo, modifiedDate, length, obsoleteFileData, mod: modName);
                        }
                    }
                }
            }

            var pakList = new PakList(ObsoleteData.PAK_LIST);
            foreach (var pakFile in Directory.EnumerateFiles(gamePath, "*.pak", SearchOption.TopDirectoryOnly)) {
                var pakFilename = Path.GetFileName(pakFile);
                if (PathHelper.PAK_PATHS.Contains(pakFilename)) continue; // Ignore vanilla PAK files.

                var modifiedDate = new FileInfo(pakFile).LastWriteTime;
                var pak          = new PakData(pakList);
                pak.ReadEntries(pakFile);
                using var pakStream = new BinaryReader(File.Open(pakFile, FileMode.Open, FileAccess.Read, FileShare.Read));

                foreach (var (file, entry) in pak.filenameToEntryMap) {
                    var checkPath = file.Replace('\\', '/').ToLower();
                    if (ObsoleteData.GOOD_PAK_MAP.TryGetValue(checkPath, out var goodPakInfo)) {
                        var     bytes   = PakData.ReadEntry(pakStream, entry);
                        var     length  = bytes.Length;
                        string? modName = null;
                        if (fmmModByFiles.TryGetValue(pakFilename, out var mod)) {
                            modName = mod;
                        }
                        CheckFile(file, bytes, goodPakInfo, modifiedDate, length, obsoleteFileData, pak: pakFilename, mod: modName);
                    }
                }
            }

            if (obsoleteFileData.Count == 0) {
                MessageBox.Show("No obsolete files found.", "Nothing Found", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            RunOnUiThread(() => {
                new ModListWindow(gamePath, obsoleteFileData).Show();
                Close();
            });
        });
    }

    private static Dictionary<string, List<string>> ReadFmmMods(string fmmPath) {
        const string fileLineHeader   = "file=";
        var          modFiles         = new Dictionary<string, List<string>>();
        var          installedIniFile = Path.Join(fmmPath, "Games", FMM_GAME_FOLDER, "installed.ini");

        if (File.Exists(installedIniFile)) {
            using var    iniFile = new StreamReader(installedIniFile);
            string?      section = null;
            List<string> files   = [];

            while (iniFile.ReadLine() is {} line) {
                var trim = line.Trim();
                if (trim.StartsWith('[')) {
                    if (section != null) {
                        modFiles[section] = files;
                    }

                    section = trim[1..^1];
                    files   = [];
                } else {
                    if (trim.StartsWith(fileLineHeader)) {
                        files.Add(trim[fileLineHeader.Length..].ToLower().Replace('\\', '/'));
                    }
                }
            }

            // For the final section we're reading.
            if (section != null) {
                modFiles[section] = files;
            }
        }

        return modFiles;
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static void CheckFile(string file, byte[] bytes, GoodPakInfo goodPakInfo, DateTime modifiedDate, long length, List<ObsoleteFileData> obsoleteFileData, string? pak = null, string? mod = null) {
        var hash               = PakFileHash.GetChecksum(bytes);
        var badPakHash         = goodPakInfo.badPakInfo.Cast<PakFileInfo?>().FirstOrDefault(info => info!.Value.hash == hash);
        var goodPakReleaseInfo = GetInfoForPak(goodPakInfo.knownGoodPak);
        if (goodPakReleaseInfo == null) return;

        var crcMismatch    = false;
        var dateMismatch   = modifiedDate < goodPakReleaseInfo.Value.releaseDate;
        var hashMismatch   = badPakHash != null;
        var lengthMismatch = length != goodPakInfo.goodPakInfo.length;

        if (file.EndsWith($"user.{Global.USER_VERSION}")) {
            var reDataFile = ReDataFile.Read(bytes, true);
            foreach (var (instanceHash, instanceCrc) in reDataFile.rsz.instanceInfo) {
                if (ObsoleteData.HASH_TO_CRC_MAP.TryGetValue(instanceHash, out var goodCrc)) {
                    if (instanceCrc != goodCrc) {
                        crcMismatch = true; // Because the object hash doesn't match.
                    }
                    // Else, the object CRC matches, ignore.
                } else {
                    crcMismatch = true; // Because the object hash doesn't exist.
                }
            }
        }

        if (crcMismatch || dateMismatch || hashMismatch || lengthMismatch) {
            // If the good pak is the first PAK, then ignore it.
            if (goodPakInfo.knownGoodPak == PathHelper.PAK_PATHS[0]) return;

            var obsoleteData = new ObsoleteFileData {
                obsoletedBy = GetObsoletedByString(goodPakInfo, goodPakReleaseInfo.Value),
                path        = goodPakInfo.filename,
                pak         = pak,
                modName     = mod
            };

            // Do in order of most likely to be a legit match -> least likely.
            if (hashMismatch) obsoleteData.reason        = Reason.HASH;
            else if (crcMismatch) obsoleteData.reason    = Reason.CRC;
            else if (dateMismatch) obsoleteData.reason   = Reason.DATE;
            else if (lengthMismatch) obsoleteData.reason = Reason.LENGTH;
            else throw new("Unknown reason!");

            obsoleteFileData.Add(obsoleteData);
        }
    }

    private static string GetObsoletedByString(GoodPakInfo goodPakInfo, PakDateInfo goodPakReleaseInfo) {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (goodPakReleaseInfo.updateName == null) {
            return $"{goodPakInfo.knownGoodPak} ({goodPakReleaseInfo.gameVersion})";
        }
        return $"{goodPakInfo.knownGoodPak} ({goodPakReleaseInfo.gameVersion}, {goodPakReleaseInfo.updateName})";
    }

    private static PakDateInfo? GetInfoForPak(string pak) {
        if (ObsoleteData.PAK_INFO_BY_NAME.TryGetValue(pak, out var info)) {
            return info;
        }
        return null;
    }

    private void EnterKeyPressed() {
        Scan_OnClick(null, null);
    }

    public void RunOnUiThread(Action action) {
        Dispatcher.BeginInvoke(action);
    }

    public T GetOnUiThread<T>(Func<T> action) {
        return Dispatcher.Invoke(action);
    }

    public static void ShowError(Exception err) {
        MessageBox.Show("Error occurred. Press Ctrl+C to copy the contents of ths window and report to the developer.\r\n\r\n" + err, "Error Occurred", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

public static class Extensions {
    public static string PathCombine(this string a, string b) {
        return Path.Combine(a, b);
    }
}