using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Constants;
using RE_Editor.Models;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class ErikVoiceJpToEng : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Erik Voice (Jp to Eng)";
        const string description = "Changes Erik's Eng voice to use the Jp voice.";
        const string version     = "1.0";

        List<string> voiceFiles = new(200);
        voiceFiles.AddRange(Directory.EnumerateFiles($@"{PathHelper.CHUNK_PATH}\natives\STM\Sound\Wwise", $"{NpcConstants.ERIK}*.spck.1.X64.Ja", SearchOption.AllDirectories));
        voiceFiles.AddRange(Directory.EnumerateFiles($@"{PathHelper.CHUNK_PATH}\natives\STM\Streaming\Sound\Wwise", $"{NpcConstants.ERIK}*.spck.1.X64.Ja", SearchOption.AllDirectories));

        var fileCopyMap = (from file in voiceFiles
                           let dest = file.Replace(PathHelper.CHUNK_PATH, "")
                                          .Replace(".Ja", ".En")
                           select new KeyValuePair<string, ModMaker.CustomCopy>(dest, new(file, (sourceFile, destFile) => {
                               File.Copy(sourceFile, destFile);
                               var writer = new BinaryWriter(File.OpenWrite(destFile));
                               writer.BaseStream.Seek(48, SeekOrigin.Begin);
                               writer.Write("e\0n\0g\0l\0i\0s\0h\0\0\0"u8);
                               writer.Close();
                           }))).ToDictionary(pair => pair.Key, object (pair) => pair.Value);

        var mod = new NexusMod {
            Name            = name,
            Version         = version,
            Desc            = description,
            Files           = [],
            AdditionalFiles = fileCopyMap
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true);
    }
}