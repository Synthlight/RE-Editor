using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class DumpArmor : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        /*
         * Armor paths:
         * `natives/STM/Art/Model/Character/ch02/008/001/2/ch02_008_0012` (Chatacabra male chest.)
         * `natives/STM/Art/Model/Character/{male/female somehow}/{armor series mod id (pad 3)}/{variant (pad 3)}/{armor part type +1}/{male/female somehow}_{armor series mod id (pad 3)}_{variant (pad 3)}{armor part type +1}`
         */

        var armorData       = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.ARMOR_DATA_PATH}").rsz.GetEntryObject<App_user_data_ArmorData>().Values.Cast<App_user_data_ArmorData_cData>().ToList();
        var armorOuterData  = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.ARMOR_LAYERED_DATA_PATH}").rsz.GetEntryObject<App_user_data_OuterArmorData>().Values.Cast<App_user_data_OuterArmorData_cData>().ToList();
        var armorSeriesData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.ARMOR_SERIES_DATA_PATH}").rsz.GetEntryObject<App_user_data_ArmorSeriesData>().Values.Cast<App_user_data_ArmorSeriesData_cData>().ToList();

        var list = new HashSet<ArmorModelData>();
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var armor in armorData) {
            if (armor.Series_Unwrapped == App_ArmorDef_SERIES_Fixed.ID_000) continue;

            var modelData = new ArmorModelData {
                name   = armor.Name_,
                series = armorSeriesData.First(series => series.Series_Unwrapped == armor.Series_Unwrapped),
                part   = armor.PartsType_Unwrapped,
            };
            list.Add(modelData);
        }
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var outerArmor in armorOuterData) {
            if (outerArmor.Series_Unwrapped == App_ArmorDef_SERIES_Fixed.ID_000) continue;

            var modelData = new ArmorModelData {
                name   = string.IsNullOrWhiteSpace(outerArmor.Name_Male) ? outerArmor.Name_Female : outerArmor.Name_Male,
                series = armorSeriesData.First(series => series.Series_Unwrapped == outerArmor.Series_Unwrapped),
                part   = outerArmor.PartsType_Unwrapped,
            };
            list.Add(modelData);
        }

        var writer = new StreamWriter(File.Open($@"{PathHelper.MODS_PATH}\..\Dumped Data\Armor Models.csv", FileMode.Create, FileAccess.Write, FileShare.Read));
        writer.WriteLine("Name,Equip Gender,Style Gender,Path");
        foreach (var data in list) {
            WritePart(writer, data.name, data, Gender.Male, Gender.Male);
            WritePart(writer, data.name, data, Gender.Male, Gender.Female);
            WritePart(writer, data.name, data, Gender.Female, Gender.Male);
            WritePart(writer, data.name, data, Gender.Female, Gender.Female);
        }
        writer.Close();
    }

    private static void WritePart(StreamWriter writer, string name, ArmorModelData armor, Gender equipGender, Gender styleGender) {
        writer.Write(name);
        writer.Write(',');
        writer.Write(equipGender == Gender.Male ? "Male" : "Female");
        writer.Write(',');
        writer.Write(styleGender == Gender.Male ? "Male" : "Female");
        writer.Write(',');
        var genderChar = equipGender == Gender.Male ? "ch02" : "ch03";
        var modelId    = armor.series.ModId;
        var subId      = styleGender == Gender.Male ? armor.series.ModSubMaleId : armor.series.ModSubFemaleId;
        var part       = GetModelPathArmorPart(armor.part);
        writer.WriteLine($"natives/STM/Art/Model/Character/{genderChar}/{modelId:000}/{subId:000}/{part}/{genderChar}_{modelId:000}_{subId:000}{part}");
    }

    private static byte GetModelPathArmorPart(App_ArmorDef_ARMOR_PARTS_Fixed part) {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return part switch {
            App_ArmorDef_ARMOR_PARTS_Fixed.HELM => 3,
            App_ArmorDef_ARMOR_PARTS_Fixed.BODY => 2,
            App_ArmorDef_ARMOR_PARTS_Fixed.ARM => 1,
            App_ArmorDef_ARMOR_PARTS_Fixed.WAIST => 5,
            App_ArmorDef_ARMOR_PARTS_Fixed.LEG => 4,
            App_ArmorDef_ARMOR_PARTS_Fixed.SLINGER => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(part), part, null)
        };
    }

    private struct ArmorModelData : IEquatable<ArmorModelData> {
        public string                              name;
        public App_user_data_ArmorSeriesData_cData series;
        public App_ArmorDef_ARMOR_PARTS_Fixed      part;

        public bool Equals(ArmorModelData other) {
            return name == other.name && Equals(series.Series_Unwrapped, other.series.Series_Unwrapped) && part == other.part;
        }

        public override bool Equals(object obj) {
            return obj is ArmorModelData other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(name, series.Series_Unwrapped, (int) part);
        }

        public static bool operator ==(ArmorModelData left, ArmorModelData right) {
            return left.Equals(right);
        }

        public static bool operator !=(ArmorModelData left, ArmorModelData right) {
            return !left.Equals(right);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private enum Gender {
        Male,
        Female
    }
}