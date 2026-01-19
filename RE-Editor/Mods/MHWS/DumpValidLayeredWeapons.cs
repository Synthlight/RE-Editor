using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class DumpValidLayeredWeapons : IMod {
    private const string ROOT_OUT_PATH = $@"{PathHelper.MODS_PATH}\Layered-Weapon-Unlocker\LUA\Layered-Weapon-Unlocker_Data";

    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        DumpValidPlayerWeaponsByName();
        DumpValidOtomoWeaponsByName();

        DumpForLua();
    }

    private static void DumpValidPlayerWeaponsByName() {
        const string name          = "ValidPlayerWeaponsByName";
        var          stringBuilder = new StringBuilder();

        stringBuilder.Append($"{name} = {{\n");
        foreach (var weaponType in Global.WEAPON_TYPES) {
            var playerWeaponData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}\natives\STM\GameDesign\Common\Weapon\Outer{weaponType}Data.user.3").rsz.GetEntryObject<App_user_data_OuterWeaponData>().Values.Cast<App_user_data_OuterWeaponData_cData>().ToList();

            var validWeaponDataForType = (from weapon in playerWeaponData
                                          where DataHelper.WEAPON_LAYERED_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng].ContainsKey(weapon.Name)
                                          let seriesName = DataHelper.WEAPON_LAYERED_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng][weapon.Name].Replace("\"", "\\\"")
                                          let outerIdName = Enum.GetName(weapon.Id)
                                          let outerIdNormal = Enum.Parse<App_WeaponDef_OuterWeaponId>(outerIdName)
                                          select new {name = seriesName, outerIdNormal}).DistinctBy(a => a.name)
                                                                                        .ToDictionary(a => a.name, a => a.outerIdNormal);

            stringBuilder.Append(DumpValidLayeredArmor.MakeLuaTable(validWeaponDataForType, weaponType, 1));
            if (weaponType == Global.WEAPON_TYPES.Last()) {
                stringBuilder.Append('\n');
            } else {
                stringBuilder.Append(",\n");
            }
        }
        stringBuilder.Append("}\n\n");

        File.WriteAllText($@"{ROOT_OUT_PATH}\{name}.lua", stringBuilder.ToString());
    }

    private static void DumpValidOtomoWeaponsByName() {
        var otomoArmorData  = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.OTOMO_ARMOR_LAYERED_DATA_PATH}").rsz.GetEntryObject<App_user_data_OtomoOuterArmorData>().Values.Cast<App_user_data_OtomoOuterArmorData_cData>().ToList();
        var otomoWeaponData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.OTOMO_WEAPON_DATA_PATH}").rsz.GetEntryObject<App_user_data_OtWeaponData>().Values.Cast<App_user_data_OtWeaponData_cData>().ToList();

        // Weapons don't have any sort of layered specific file to determine what's valid and what's not.
        // They use the same series ID as armor though, so we need to build a list of valid potential IDs to filter by first.
        var validSeriesIds = (from armor in otomoArmorData
                              select armor.Series_Unwrapped).ToList();

        var validWeaponData = (from weapon in otomoWeaponData
                               where weapon.Series_Unwrapped != App_OtEquipDef_EQUIP_DATA_ID_Fixed.NONE
                               where validSeriesIds.Contains(weapon.Series_Unwrapped)
                               where DataHelper.OTOMO_WEAPON_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng].ContainsKey(weapon.Name)
                               let seriesName = DataHelper.OTOMO_WEAPON_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng][weapon.Name]
                                                          .Replace("\"", "\\\"")
                                                          .FixOtomoWeaponNames()
                               let equipIdName = Enum.GetName(weapon.Series_Unwrapped)
                               let equipIdNormal = Enum.Parse<App_OtEquipDef_EQUIP_DATA_ID>(equipIdName)
                               select new {name = seriesName, equipIdNormal}).DistinctBy(a => a.name)
                                                                             .ToDictionary(a => a.name, a => a.equipIdNormal);

        const string name = "ValidOtomoWeaponsByName";
        File.WriteAllText($@"{ROOT_OUT_PATH}\{name}.lua", DumpValidLayeredArmor.MakeLuaTable(validWeaponData, name));
    }

    private static void DumpForLua() {
        var stringBuilder = new StringBuilder();

        AddLuaStringTable(stringBuilder, "WeaponTypes", Global.WEAPON_TYPES);
        stringBuilder.Append("\n\n");

        stringBuilder.Append(DumpValidLayeredArmor.MakeLuaTable(Global.WEAPON_TYPE_NAME_TO_TYPE, "WeaponTypeToName"));
        stringBuilder.Append("\n\n");

        DumpValidLayeredArmor.AddLuaEnum<App_WeaponDef_OuterWeaponId>(stringBuilder);
        DumpValidLayeredArmor.AddLuaEnum<App_OtEquipDef_EQUIP_DATA_ID>(stringBuilder);

        File.WriteAllText($@"{ROOT_OUT_PATH}\Enums.lua", stringBuilder.ToString());
    }

    public static void AddLuaStringTable(StringBuilder stringBuilder, string name, List<string> data, int indentLevel = 0) {
        var indent = "";
        for (var i = 0; i < indentLevel; i++) {
            indent += "    ";
        }

        stringBuilder.Append($"{indent}{name} = {{\n");
        for (var i = 0; i < data.Count; i++) {
            stringBuilder.Append($"{indent}    \"{data[i]}\"");
            if (i == data.Count - 1) {
                stringBuilder.Append('\n');
            } else {
                stringBuilder.Append(",\n");
            }
        }
        stringBuilder.Append($"{indent}}}");
    }
}

public static class DumpValidLayeredWeaponsExtensions {
    public static string FixOtomoWeaponNames(this string input) {
        if (input.StartsWith("F ")) input = $"Felyne {input[2..]}";
        return input;
    }
}