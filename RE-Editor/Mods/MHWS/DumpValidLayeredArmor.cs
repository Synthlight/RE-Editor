using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class DumpValidLayeredArmor : IMod {
    private const string ROOT_OUT_PATH = $@"{PathHelper.MODS_PATH}\Layered-Armor-Unlocker\LUA\Layered-Armor-Unlocker_Data";

    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        DumpValidPlayerArmorParts();
        DumpPlayerArmorSeriesByName();
        DumpValidOtomoArmorParts();
        DumpOtomoArmorSeriesByName();

        DumpForNativePlugin();
        DumpForLua();
    }

    private static void DumpValidPlayerArmorParts() {
        var playerArmorData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.ARMOR_LAYERED_DATA_PATH}").rsz.GetEntryObject<App_user_data_OuterArmorData>().Values.Cast<App_user_data_OuterArmorData_cData>().ToList();

        var playerValidArmorData = (from armor in playerArmorData
                                    where armor.Series_Unwrapped != App_ArmorDef_SERIES_Fixed.ID_000 && armor.Series_Unwrapped != App_ArmorDef_SERIES_Fixed.NONE
                                    where DataHelper.ARMOR_LAYERED_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng].ContainsKey(armor.NameFemale)
                                    let seriesIdName = Enum.GetName(armor.Series_Unwrapped)
                                    let seriesIdNormal = Enum.Parse<App_ArmorDef_SERIES>(seriesIdName)
                                    let partsIdName = Enum.GetName(armor.PartsType_Unwrapped)
                                    let partsIdNormal = Enum.Parse<App_ArmorDef_ARMOR_PARTS>(partsIdName)
                                    group partsIdNormal by seriesIdNormal
                                    into g
                                    select g).ToDictionary(a => a.Key, a => a.ToList());

        const string name = "ValidPlayerArmor";
        WriteToFile(playerValidArmorData, $"{name}.json");
        File.WriteAllText($@"{ROOT_OUT_PATH}\{name}.lua", MakeLuaTable(playerValidArmorData, name));
    }

    private static void DumpPlayerArmorSeriesByName() {
        var playerArmorData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.ARMOR_LAYERED_DATA_PATH}").rsz.GetEntryObject<App_user_data_OuterArmorData>().Values.Cast<App_user_data_OuterArmorData_cData>().ToList();

        var playerArmorSeriesByName = (from series in playerArmorData
                                       where series.Series_Unwrapped != App_ArmorDef_SERIES_Fixed.ID_000 && series.Series_Unwrapped != App_ArmorDef_SERIES_Fixed.NONE
                                       where DataHelper.ARMOR_LAYERED_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng].ContainsKey(series.NameFemale)
                                       let seriesName = DataHelper.ARMOR_SERIES_BY_ENUM_VALUE[Global.LangIndex.eng][(int) series.Series_Unwrapped]
                                       orderby seriesName
                                       let seriesIdName = Enum.GetName(series.Series_Unwrapped)
                                       let seriesIdNormal = Enum.Parse<App_ArmorDef_SERIES>(seriesIdName)
                                       select new {name = seriesName, seriesIdNormal}).DistinctBy(a => a.name)
                                                                                      .ToDictionary(a => a.name, a => a.seriesIdNormal);

        const string name = "PlayerArmorSeriesByName";
        WriteToFile(playerArmorSeriesByName, $"{name}.json");
        File.WriteAllText($@"{ROOT_OUT_PATH}\{name}.lua", MakeLuaTable(playerArmorSeriesByName, name));
    }

    private static void DumpValidOtomoArmorParts() {
        var otomoArmorData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.OTOMO_ARMOR_LAYERED_DATA_PATH}").rsz.GetEntryObject<App_user_data_OtomoOuterArmorData>().Values.Cast<App_user_data_OtomoOuterArmorData_cData>().ToList();

        var otomoValidArmorData = (from armor in otomoArmorData
                                   where armor.Series_Unwrapped != App_OtEquipDef_EQUIP_DATA_ID_Fixed.NONE
                                   where DataHelper.OTOMO_LAYERED_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng].ContainsKey(armor.Name)
                                   let seriesIdName = Enum.GetName(armor.Series_Unwrapped)
                                   let seriesIdNormal = Enum.Parse<App_OtEquipDef_EQUIP_DATA_ID>(seriesIdName)
                                   where armor.EquipType_Unwrapped != App_OtEquipDef_EQUIP_TYPE_Fixed.WEAPON
                                   let partsIdName = Enum.GetName(armor.EquipType_Unwrapped)
                                   let partsIdNormal = Enum.Parse<App_OtEquipDef_EQUIP_TYPE>(partsIdName)
                                   group partsIdNormal by seriesIdNormal
                                   into g
                                   select g).ToDictionary(a => a.Key, a => a.ToList());

        const string name = "ValidOtomoArmor";
        WriteToFile(otomoValidArmorData, $"{name}.json");
        File.WriteAllText($@"{ROOT_OUT_PATH}\{name}.lua", MakeLuaTable(otomoValidArmorData, name));
    }

    private static void DumpOtomoArmorSeriesByName() {
        var otomoArmorData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.OTOMO_ARMOR_LAYERED_DATA_PATH}").rsz.GetEntryObject<App_user_data_OtomoOuterArmorData>().Values.Cast<App_user_data_OtomoOuterArmorData_cData>().ToList();

        var otomoArmorSeriesByName = (from armor in otomoArmorData
                                      where armor.Series_Unwrapped != App_OtEquipDef_EQUIP_DATA_ID_Fixed.NONE
                                      where DataHelper.OTOMO_LAYERED_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng].ContainsKey(armor.Name)
                                      let seriesName = DataHelper.OTOMO_SERIES_BY_ENUM_VALUE[Global.LangIndex.eng][(int) armor.Series_Unwrapped]
                                      orderby seriesName
                                      let seriesIdName = Enum.GetName(armor.Series_Unwrapped)
                                      let seriesIdNormal = Enum.Parse<App_OtEquipDef_EQUIP_DATA_ID>(seriesIdName)
                                      select new {name = seriesName, seriesIdNormal}).DistinctBy(a => a.name)
                                                                                     .ToDictionary(a => a.name, a => a.seriesIdNormal);

        const string name = "OtomoArmorSeriesByName";
        WriteToFile(otomoArmorSeriesByName, $"{name}.json");
        File.WriteAllText($@"{ROOT_OUT_PATH}\{name}.lua", MakeLuaTable(otomoArmorSeriesByName, name));
    }

    private static void WriteToFile(object data, string filename) {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        var path = $@"{PathHelper.MODS_PATH}\Layered-Armor-Unlocker\Assets\{filename}";
        var dir  = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
        var writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read));
        writer.Write(json);
        writer.Close();
    }

    private static void DumpForNativePlugin() {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append("#pragma once\n\n");
        stringBuilder.Append("#include <vector>\n\n");

        AddEnum<App_ArmorDef_ARMOR_PARTS>(stringBuilder);
        AddEnum<App_ArmorDef_SERIES>(stringBuilder);
        AddEnum<App_CharacterDef_GENDER>(stringBuilder);
        AddEnum<App_OtEquipDef_EQUIP_TYPE>(stringBuilder);
        AddEnum<App_OtEquipDef_EQUIP_DATA_ID>(stringBuilder);

        File.WriteAllText(@"R:\Games\Monster Hunter Wilds\REF Plugin Mods\Layered-Armor-Unlocker-Native\Layered-Armor-Unlocker-Native\Enums.hpp", stringBuilder.ToString());
    }

    private static void AddEnum<T>(StringBuilder stringBuilder) where T : struct, Enum {
        var name   = typeof(T).Name;
        var names  = Enum.GetNames<T>();
        var values = Enum.GetValues<T>();
        stringBuilder.Append($"enum class {name} : int32_t {{\n");
        for (var i = 0; i < names.Length; i++) {
            stringBuilder.Append($"    {names[i]} = {((int) (object) values[i])}");
            if (i == names.Length - 1) {
                stringBuilder.Append('\n');
            } else {
                stringBuilder.Append(",\n");
            }
        }
        stringBuilder.Append("};\n\n");

        stringBuilder.Append($"std::vector {name}_values = {{");
        for (var i = 0; i < names.Length; i++) {
            stringBuilder.Append($"{name}::{names[i]}");
            if (i < names.Length - 1) {
                stringBuilder.Append(", ");
            }
        }
        stringBuilder.Append("};\n\n");
    }

    private static void DumpForLua() {
        var stringBuilder = new StringBuilder();

        AddLuaEnum<App_ArmorDef_ARMOR_PARTS>(stringBuilder);
        AddLuaEnum<App_ArmorDef_SERIES>(stringBuilder);
        AddLuaEnum<App_CharacterDef_GENDER>(stringBuilder);
        AddLuaEnum<App_OtEquipDef_EQUIP_DATA_ID>(stringBuilder);
        AddLuaEnum<App_OtEquipDef_EQUIP_TYPE>(stringBuilder);

        File.WriteAllText($@"{ROOT_OUT_PATH}\Enums.lua", stringBuilder.ToString());
    }

    public static void AddLuaEnum<T>(StringBuilder stringBuilder) where T : struct, Enum {
        var name   = typeof(T).Name;
        var names  = Enum.GetNames<T>();
        var values = Enum.GetValues<T>();
        stringBuilder.Append($"{name} = {{\n");
        for (var i = 0; i < names.Length; i++) {
            //stringBuilder.Append($"    {{name = \"{names[i]}\", value = {((int) (object) values[i])}}}"); // To ensure order.
            stringBuilder.Append($"    {names[i]} = {((int) (object) values[i])}");
            if (i == names.Length - 1) {
                stringBuilder.Append('\n');
            } else {
                stringBuilder.Append(",\n");
            }
        }
        stringBuilder.Append("}\n\n");
    }

    public static string MakeLuaTable<K, V>(Dictionary<K, List<V>> dict, string name) {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append($"{name} = {{\n");
        var keys = dict.Keys.OrderBy(key => key.ToString()).ToList();
        for (var i = 0; i < keys.Count; i++) {
            var key   = keys[i];
            var value = dict[key];
            stringBuilder.Append($"    {key.ToString()} = {{\"{string.Join("\", \"", value)}\"}}");
            if (i == keys.Count - 1) {
                stringBuilder.Append('\n');
            } else {
                stringBuilder.Append(",\n");
            }
        }
        stringBuilder.Append('}');

        return stringBuilder.ToString();
    }

    public static string MakeLuaTable<V>(Dictionary<string, V> dict, string name, int indentLevel = 0) {
        var indent = "";
        for (var i = 0; i < indentLevel; i++) {
            indent += "    ";
        }

        var stringBuilder = new StringBuilder();

        stringBuilder.Append($"{indent}{name} = {{\n");
        var keys = dict.Keys.OrderBy(key => key.ToString()).ToList();
        for (var i = 0; i < keys.Count; i++) {
            var key   = keys[i];
            var value = dict[key];
            stringBuilder.Append($"{indent}    {{name = \"{key.StripGreek()}\", value = \"{value}\"}}"); // To ensure order.
            if (i == keys.Count - 1) {
                stringBuilder.Append('\n');
            } else {
                stringBuilder.Append(",\n");
            }
        }
        stringBuilder.Append($"{indent}}}");

        return stringBuilder.ToString();
    }
}