using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class MoreRealisticShotgunSpread : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "More Realistic Shotgun Spread";
        const string description = "Changes the shotgun spread of MSBG 500 / W870 Police / 990-TAC to all be the same more realistic spread.";
        const string version     = "1.0";

        List<string> files = [];

        // Doing infinite ammo this way is kinda broken. Not all weapons are covered for some reason. Doing it in LUA instead.
        files.AddRange(from file in Directory.EnumerateFiles($@"{PathHelper.CHUNK_PATH}\natives\STM\GameAssets\Weapon\UserData\Parameter", "arm*.user*", SearchOption.TopDirectoryOnly)
                       where File.Exists(file)
                       select file.Replace(PathHelper.CHUNK_PATH, ""));

        var mod = new NexusMod {
            Name           = name,
            Version        = version,
            Desc           = description,
            Image          = $@"{PathHelper.MODS_PATH}\{name}\Pic.png",
            Files          = files,
            FilteredAction = ModStuff
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true);
    }

    private static bool ModStuff(IList<RszObject> rszObjectData, string file) {
        var changesMade = false;

        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_PlayerShotgunWeaponParameterUserData shotgun:
                    var shotgunStructure = shotgun.ShotgunStructureParam[0];
                    shotgunStructure.InnerRadius   = 0.01f;
                    shotgunStructure.OuterRadius   = 0.01f;
                    shotgunStructure.RotationAngle = 1.0f;

                    changesMade = true;
                    break;
            }
        }

        return changesMade;
    }
}