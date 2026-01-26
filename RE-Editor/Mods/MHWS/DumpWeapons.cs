using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class DumpWeapons : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        using var writer        = new StreamWriter(File.Open($@"{PathHelper.MODS_PATH}\..\Dumped Data\Weapon Models.csv", FileMode.Create, FileAccess.Write, FileShare.Read));
        using var layeredWriter = new StreamWriter(File.Open($@"{PathHelper.MODS_PATH}\..\Dumped Data\Weapon Models (Layered).csv", FileMode.Create, FileAccess.Write, FileShare.Read));
        writer.WriteLine("Weapon Type,Name,Path");
        layeredWriter.WriteLine("Weapon Type,Name,Path");

        var weaponTypes = Enum.GetValues<WeaponType>();
        for (var weaponType = 0; weaponType < weaponTypes.Length; weaponType++) {
            var weaponTypeName        = weaponTypes[weaponType];
            var weaponDataPath        = @$"\natives\STM\GameDesign\Common\Weapon\{weaponTypeName}.user.3";
            var layeredWeaponDataPath = @$"\natives\STM\GameDesign\Common\Weapon\Outer{weaponTypeName}Data.user.3";
            var weaponData            = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{weaponDataPath}").rsz.GetEntryObject<App_user_data_WeaponData>().Values.Cast<App_user_data_WeaponData_cData>().ToList();
            var layeredWeaponData     = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{layeredWeaponDataPath}").rsz.GetEntryObject<App_user_data_OuterWeaponData>().Values.Cast<App_user_data_OuterWeaponData_cData>().ToList();

            Dictionary<string, int> modelData = [];
            foreach (var weapon in weaponData) {
                if (weapon.CustomModelId > 0) {
                    // Nothing using this exists yet.
                    throw new("I don't know how to handle `CustomModelId`s.");
                }
                modelData[weapon.Name_] = weapon.ModelId;
            }
            WriteData(writer, weaponType, weaponTypeName, modelData);

            modelData.Clear();
            foreach (var layeredWeapon in layeredWeaponData) {
                modelData[layeredWeapon.Name_] = layeredWeapon.ModelId;
            }
            WriteData(layeredWriter, weaponType, weaponTypeName, modelData);
        }
    }

    private static void WriteData(StreamWriter writer, int weaponType, WeaponType type, Dictionary<string, int> modelData) {
        foreach (var (name, modelId) in modelData) {
            var modelPathBase = $@"Art/Model/Item/it{weaponType:00}/{modelId:00/0000}/it{weaponType:00}{modelId:00_0000}_";
            var typeName      = GetNameOfType(type);

            switch (type) {
                case WeaponType.LongSword: // GS
                case WeaponType.Hammer: // Ham
                case WeaponType.Whistle: // HH
                case WeaponType.SlashAxe: // SA
                case WeaponType.Rod: // IG
                case WeaponType.HeavyBowgun: // HBG
                case WeaponType.LightBowgun: // LBG
                    writer.WriteLine($"{typeName},{name},natives/STM/{modelPathBase}0.mesh");
                    break;
                case WeaponType.ShortSword: // S & S
                    writer.WriteLine($"{typeName},{name} (Sword),natives/STM/{modelPathBase}0.mesh");
                    writer.WriteLine($"{typeName},{name} (Shield),natives/STM/{modelPathBase}1.mesh");
                    break;
                case WeaponType.TwinSword: // DB
                    writer.WriteLine($"{typeName},{name} (L),natives/STM/{modelPathBase}0.mesh");
                    writer.WriteLine($"{typeName},{name} (R),natives/STM/{modelPathBase}1.mesh");
                    break;
                case WeaponType.Tachi: // LS
                    writer.WriteLine($"{typeName},{name} (Sword),natives/STM/{modelPathBase}0.mesh");
                    writer.WriteLine($"{typeName},{name} (Sheathe),natives/STM/{modelPathBase}1.mesh");
                    break;
                case WeaponType.Lance: // Lance
                    writer.WriteLine($"{typeName},{name} (Lance),natives/STM/{modelPathBase}0.mesh");
                    writer.WriteLine($"{typeName},{name} (Shield),natives/STM/{modelPathBase}1.mesh");
                    break;
                case WeaponType.GunLance: // GL
                    writer.WriteLine($"{typeName},{name} (Gunlance),natives/STM/{modelPathBase}0.mesh");
                    writer.WriteLine($"{typeName},{name} (Shield),natives/STM/{modelPathBase}1.mesh");
                    break;
                case WeaponType.ChargeAxe: // CB
                    writer.WriteLine($"{typeName},{name} (Sword),natives/STM/{modelPathBase}0.mesh");
                    writer.WriteLine($"{typeName},{name} (Shield),natives/STM/{modelPathBase}1.mesh");
                    break;
                case WeaponType.Bow: // Bow
                    writer.WriteLine($"{typeName},{name} (Bow),natives/STM/{modelPathBase}0.mesh");
                    writer.WriteLine($"{typeName},{name} (Quiver),natives/STM/{modelPathBase}1.mesh");
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static string GetNameOfType(WeaponType weaponType) {
        return weaponType switch {
            WeaponType.LongSword => "Great Sword",
            WeaponType.ShortSword => "Sword & Shield",
            WeaponType.TwinSword => "Dual Blades",
            WeaponType.Tachi => "Long Sword",
            WeaponType.Hammer => "Hammer",
            WeaponType.Whistle => "Hunting Horn",
            WeaponType.Lance => "Lance",
            WeaponType.GunLance => "Gunlance",
            WeaponType.SlashAxe => "Switch Axe",
            WeaponType.ChargeAxe => "Charge Blade",
            WeaponType.Rod => "Insect Glaive",
            WeaponType.Bow => "Bow",
            WeaponType.HeavyBowgun => "Heavy Bowgun",
            WeaponType.LightBowgun => "Light Bowgun",
            _ => throw new ArgumentOutOfRangeException(nameof(weaponType))
        };
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private enum WeaponType {
        LongSword   = 0,
        ShortSword  = 1,
        TwinSword   = 2,
        Tachi       = 3,
        Hammer      = 4,
        Whistle     = 5,
        Lance       = 6,
        GunLance    = 7,
        SlashAxe    = 8,
        ChargeAxe   = 9,
        Rod         = 10,
        Bow         = 11,
        HeavyBowgun = 12,
        LightBowgun = 13
    };
}