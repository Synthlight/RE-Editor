using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models.Structs;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class DumpWeapons : IMod {
    [UsedImplicitly]
    public static void Make() {
        var writer = new StreamWriter(File.Open($@"{PathHelper.MODS_PATH}\..\Weapon Models.csv", FileMode.Create, FileAccess.Write, FileShare.Read));
        writer.WriteLine("Weapon Type,Name,Path");

        var weaponTypes = Enum.GetValues<WeaponType>();
        for (var i = 0; i < weaponTypes.Length; i++) {
            var type           = weaponTypes[i];
            var weaponDataPath = @$"\natives\STM\GameDesign\Common\Weapon\{type}.user.3";
            var weaponData     = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{weaponDataPath}").rsz.GetEntryObject<App_user_data_WeaponData>().Values.Cast<App_user_data_WeaponData_cData>().ToList();

            foreach (var weapon in weaponData) {
                if (weapon.CustomModelId > 0) {
                    // Nothing using this exists yet.
                    throw new("I don't know how to handle `CustomModelId`s.");
                }
                var modelId       = weapon.ModelId;
                var modelPathBase = $@"Art/Model/Item/it{i:00}/{modelId:00/0000}/it{i:00}{modelId:00_0000}_";
                var typeName      = GetNameOfType(type);

                switch (type) {
                    case WeaponType.LongSword: // GS
                    case WeaponType.Hammer: // Ham
                    case WeaponType.Whistle: // HH
                    case WeaponType.SlashAxe: // SA
                    case WeaponType.Rod: // IG
                    case WeaponType.HeavyBowgun: // HBG
                    case WeaponType.LightBowgun: // LBG
                        writer.WriteLine($"{typeName},{weapon.Name_},natives/STM/{modelPathBase}0.mesh");
                        break;
                    case WeaponType.ShortSword: // S & S
                        writer.WriteLine($"{typeName},{weapon.Name_} (Sword),natives/STM/{modelPathBase}0.mesh");
                        writer.WriteLine($"{typeName},{weapon.Name_} (Shield),natives/STM/{modelPathBase}1.mesh");
                        break;
                    case WeaponType.TwinSword: // DB
                        writer.WriteLine($"{typeName},{weapon.Name_} (L),natives/STM/{modelPathBase}0.mesh");
                        writer.WriteLine($"{typeName},{weapon.Name_} (R),natives/STM/{modelPathBase}1.mesh");
                        break;
                    case WeaponType.Tachi: // LS
                        writer.WriteLine($"{typeName},{weapon.Name_} (Sword),natives/STM/{modelPathBase}0.mesh");
                        writer.WriteLine($"{typeName},{weapon.Name_} (Sheathe),natives/STM/{modelPathBase}1.mesh");
                        break;
                    case WeaponType.Lance: // Lance
                        writer.WriteLine($"{typeName},{weapon.Name_} (Lance),natives/STM/{modelPathBase}0.mesh");
                        writer.WriteLine($"{typeName},{weapon.Name_} (Shield),natives/STM/{modelPathBase}1.mesh");
                        break;
                    case WeaponType.GunLance: // GL
                        writer.WriteLine($"{typeName},{weapon.Name_} (Gunlance),natives/STM/{modelPathBase}0.mesh");
                        writer.WriteLine($"{typeName},{weapon.Name_} (Shield),natives/STM/{modelPathBase}1.mesh");
                        break;
                    case WeaponType.ChargeAxe: // CB
                        writer.WriteLine($"{typeName},{weapon.Name_} (Sword),natives/STM/{modelPathBase}0.mesh");
                        writer.WriteLine($"{typeName},{weapon.Name_} (Shield),natives/STM/{modelPathBase}1.mesh");
                        break;
                    case WeaponType.Bow: // Bow
                        writer.WriteLine($"{typeName},{weapon.Name_} (Bow),natives/STM/{modelPathBase}0.mesh");
                        writer.WriteLine($"{typeName},{weapon.Name_} (Quiver),natives/STM/{modelPathBase}1.mesh");
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }

        writer.Close();
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