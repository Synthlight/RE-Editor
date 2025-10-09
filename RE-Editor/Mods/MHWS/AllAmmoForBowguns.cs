using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class AllAmmoForBowguns : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name                   = "BG Ammo Tweaks"; // All Ammo for LBG & HBG
        const string descriptionSl1         = "All BG can use all ammo; existing shelling levels unchanged.";
        const string descriptionSl3         = "All BG can use all ammo; all shelling levels changed to the highest shell level available for that ammo.";
        const string descriptionAmmoCapHalf = "All added ammo is set to half the highest capacity (rounding up) for that ammo type across compatible weapons.";
        const string descriptionAmmoCapHigh = "All added ammo is set to the highest capacity for that ammo type across compatible weapons.";
        const string descriptionAmmoCap15   = "All ammo is set to 15.";
        const string version                = "1.0";

        var baseLuaMod = new VariousDataTweak {
            Version      = version,
            NameAsBundle = name
        };

        List<INexusMod> mods = [];

        var highestSl  = new App_BowgunShellLv_SHELL_LV_Fixed[20];
        var highestCap = new short[20];

        foreach (var weaponName in VariousDataWriter.WEAPON_TYPES_GUNS_ONLY) {
            var file       = $@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Common\Weapon\{weaponName}.user.3";
            var weaponData = ReDataFile.Read(file).rsz.GetEntryObject<App_user_data_WeaponData>().Values.Cast<App_user_data_WeaponData_cData>().ToList();
            Array.Fill(highestSl, App_BowgunShellLv_SHELL_LV_Fixed.NONE);

            foreach (var data in weaponData) {
                for (var i = 0; i < data.ShellLv.Count; i++) {
                    highestSl[i] = (App_BowgunShellLv_SHELL_LV_Fixed) Math.Max((int) highestSl[i], (int) data.ShellLv[i].Value); // Works, but be careful. It works *now* only because the random values go up in value.
                }
                for (var i = 0; i < data.ShellNum.Count; i++) {
                    highestCap[i] = Math.Max(highestCap[i], data.ShellNum[i].Value);
                }
            }
        }

        foreach (var weaponName in VariousDataWriter.WEAPON_TYPES_GUNS_ONLY) {
            var file       = $@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Common\Weapon\{weaponName}.user.3";
            var weaponData = ReDataFile.Read(file).rsz.GetEntryObject<App_user_data_WeaponData>().Values.Cast<App_user_data_WeaponData_cData>().ToList();

            var target = weaponName switch {
                "LightBowgun" => VariousDataTweak.Target.WEAPON_DATA_LBG,
                "HeavyBowgun" => VariousDataTweak.Target.WEAPON_DATA_HBG,
                _ => throw new ArgumentOutOfRangeException(nameof(weaponName), weaponName, null)
            };

            foreach (var levelMode in Enum.GetValues<ShellLevelMode>()) {
                string shortName;
                string desc;
                switch (levelMode) {
                    case ShellLevelMode.PRESERVE_LOWEST:
                        shortName = "Existing SL Unchanged";
                        desc      = descriptionSl1;
                        break;
                    case ShellLevelMode.SL3:
                        shortName = "All SL Maxed";
                        desc      = descriptionSl3;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(levelMode), levelMode, null);
                }

                Dictionary<int, Dictionary<int, App_BowgunShellLv_SHELL_LV_Fixed>> slChangesMap = [];

                foreach (var data in weaponData) {
                    var bgId = weaponName switch {
                        "LightBowgun" => (int) data.LightBowgun,
                        "HeavyBowgun" => (int) data.HeavyBowgun,
                        _ => throw new ArgumentOutOfRangeException(nameof(weaponName), weaponName, null)
                    };

                    for (var i = 0; i < data.ShellLv.Count; i++) {
                        if (highestSl[i] == App_BowgunShellLv_SHELL_LV_Fixed.NONE) throw new("Highest shell level entry is zero!");

                        if ((levelMode == ShellLevelMode.PRESERVE_LOWEST && data.ShellLv[i].Value == App_BowgunShellLv_SHELL_LV_Fixed.NONE)
                            || (levelMode == ShellLevelMode.SL3 && data.ShellLv[i].Value != highestSl[i])) {
                            if (!slChangesMap.ContainsKey(bgId)) slChangesMap[bgId] = [];
                            slChangesMap[bgId][i] = highestSl[i];
                        }
                    }
                }

                mods.Add(baseLuaMod
                         .SetName($"{name} - {shortName} ({weaponName})")
                         .SetDesc(desc)
                         .SetDefaultLuaName()
                         .SetChanges([
                             new() {
                                 Target = target,
                                 Action = writer => ModShellLevelsRef(writer, weaponName, slChangesMap)
                             }
                         ])
                         .SetSkipPak(true));
            }

            foreach (var ammoMode in Enum.GetValues<ShellAmmoMode>()) {
                string shortName;
                string desc;
                switch (ammoMode) {
                    case ShellAmmoMode.HALF:
                        shortName = "Half Ammo Cap";
                        desc      = descriptionAmmoCapHalf;
                        break;
                    case ShellAmmoMode.MAX:
                        shortName = "Max Ammo Cap";
                        desc      = descriptionAmmoCapHigh;
                        break;
                    case ShellAmmoMode.NUM_15:
                        shortName = "15 Ammo Cap";
                        desc      = descriptionAmmoCap15;
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(ammoMode), ammoMode, null);
                }

                Dictionary<int, Dictionary<int, short>> capChangesMap = [];

                foreach (var data in weaponData) {
                    var bgId = weaponName switch {
                        "LightBowgun" => (int) data.LightBowgun,
                        "HeavyBowgun" => (int) data.HeavyBowgun,
                        _ => throw new ArgumentOutOfRangeException(nameof(weaponName), weaponName, null)
                    };

                    for (var i = 0; i < data.ShellNum.Count; i++) {
                        if (highestCap[i] == 0) throw new("Highest ammo cap entry is zero!");

                        if (data.ShellNum[i].Value == 0 || ammoMode == ShellAmmoMode.NUM_15) {
                            if (!capChangesMap.ContainsKey(bgId)) capChangesMap[bgId] = [];
                            capChangesMap[bgId][i] = ammoMode switch {
                                ShellAmmoMode.HALF => (short) (Math.Ceiling(highestCap[i] / 2f)),
                                ShellAmmoMode.MAX => highestCap[i],
                                ShellAmmoMode.NUM_15 => 15,
                                _ => throw new ArgumentOutOfRangeException(nameof(ammoMode), ammoMode, null)
                            };
                        }
                    }
                }

                mods.Add(baseLuaMod
                         .SetName($"{name} - {shortName} ({weaponName})")
                         .SetDesc(desc)
                         .SetDefaultLuaName()
                         .SetChanges([
                             new() {
                                 Target = target,
                                 Action = writer => ModShellAmmoRef(writer, weaponName, capChangesMap)
                             }
                         ])
                         .SetSkipPak(true));
            }
        }

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static void ModShellLevelsRef(StreamWriter writer, string weaponName, Dictionary<int, Dictionary<int, App_BowgunShellLv_SHELL_LV_Fixed>> slChangesMap) {
        foreach (var (bgId, changes) in slChangesMap) {
            writer.WriteLine($"    if (entry._{weaponName} == {bgId}) then");
            foreach (var (index, newLevel) in changes) {
                writer.WriteLine($"        entry._{nameof(App_user_data_WeaponData_cData.ShellLv)}[{index}] = {((int) newLevel)}");
            }
            writer.WriteLine("        entry:call(\"onLoad\")");
            writer.WriteLine("    end");
        }
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static void ModShellAmmoRef(StreamWriter writer, string weaponName, Dictionary<int, Dictionary<int, short>> ammoChangesMap) {
        foreach (var (bgId, changes) in ammoChangesMap) {
            writer.WriteLine($"    if (entry._{weaponName} == {bgId}) then");
            foreach (var (index, newCap) in changes) {
                writer.WriteLine($"        entry._{nameof(App_user_data_WeaponData_cData.ShellNum)}[{index}] = {newCap}");
            }
            writer.WriteLine("        entry:call(\"onLoad\")");
            writer.WriteLine("    end");
        }
    }

    public enum ShellLevelMode {
        PRESERVE_LOWEST,
        SL3
    }

    public enum ShellAmmoMode {
        HALF,
        MAX,
        NUM_15
    }
}