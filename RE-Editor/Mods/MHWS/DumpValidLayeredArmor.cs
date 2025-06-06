using System;
using System.IO;
using System.Linq;
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
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        DumpValidPlayerArmorParts();
        DumpPlayerArmorSeriesByName();
        DumpValidOtomoArmorParts();
        DumpOtomoArmorSeriesByName();
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

        WriteToFile(playerValidArmorData, "ValidPlayerArmor.json");
    }

    private static void DumpPlayerArmorSeriesByName() {
        var playerArmorData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.ARMOR_LAYERED_DATA_PATH}").rsz.GetEntryObject<App_user_data_OuterArmorData>().Values.Cast<App_user_data_OuterArmorData_cData>().ToList();

        var playerArmorSeriesByName = (from series in playerArmorData
                                       where series.Series_Unwrapped != App_ArmorDef_SERIES_Fixed.ID_000 && series.Series_Unwrapped != App_ArmorDef_SERIES_Fixed.NONE
                                       where DataHelper.ARMOR_LAYERED_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng].ContainsKey(series.NameFemale)
                                       let name = DataHelper.ARMOR_SERIES_BY_ENUM_VALUE[Global.LangIndex.eng][(int) series.Series_Unwrapped]
                                       orderby name
                                       let seriesIdName = Enum.GetName(series.Series_Unwrapped)
                                       let seriesIdNormal = Enum.Parse<App_ArmorDef_SERIES>(seriesIdName)
                                       select new {name, seriesIdNormal}).DistinctBy(a => a.name)
                                                                         .ToDictionary(a => a.name, a => a.seriesIdNormal);

        WriteToFile(playerArmorSeriesByName, "PlayerArmorSeriesByName.json");
    }

    private static void DumpValidOtomoArmorParts() {
        var otomoArmorData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.OTOMO_ARMOR_LAYERED_DATA_PATH}").rsz.GetEntryObject<App_user_data_OtomoOuterArmorData>().Values.Cast<App_user_data_OtomoOuterArmorData_cData>().ToList();

        var otomoValidArmorData = (from armor in otomoArmorData
                                   where armor.Series_Unwrapped != App_OtEquipDef_EQUIP_DATA_ID_Fixed.NONE
                                   where DataHelper.OTOMO_LAYERED_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng].ContainsKey(armor.Name)
                                   let seriesIdName = Enum.GetName(armor.Series_Unwrapped)
                                   let seriesIdNormal = Enum.Parse<App_OtEquipDef_EQUIP_DATA_ID>(seriesIdName)
                                   let partsIdName = Enum.GetName(armor.EquipType_Unwrapped)
                                   let partsIdNormal = Enum.Parse<App_OtEquipDef_EQUIP_TYPE>(partsIdName)
                                   group partsIdNormal by seriesIdNormal
                                   into g
                                   select g).ToDictionary(a => a.Key, a => a.ToList());

        WriteToFile(otomoValidArmorData, "ValidOtomoArmor.json");
    }

    private static void DumpOtomoArmorSeriesByName() {
        var otomoArmorData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.OTOMO_ARMOR_LAYERED_DATA_PATH}").rsz.GetEntryObject<App_user_data_OtomoOuterArmorData>().Values.Cast<App_user_data_OtomoOuterArmorData_cData>().ToList();

        var otomoArmorSeriesByName = (from armor in otomoArmorData
                                      where armor.Series_Unwrapped != App_OtEquipDef_EQUIP_DATA_ID_Fixed.NONE
                                      where DataHelper.OTOMO_LAYERED_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng].ContainsKey(armor.Name)
                                      let name = DataHelper.OTOMO_SERIES_BY_ENUM_VALUE[Global.LangIndex.eng][(int) armor.Series_Unwrapped]
                                      orderby name
                                      let seriesIdName = Enum.GetName(armor.Series_Unwrapped)
                                      let seriesIdNormal = Enum.Parse<App_OtEquipDef_EQUIP_DATA_ID>(seriesIdName)
                                      select new {name, seriesIdNormal}).DistinctBy(a => a.name)
                                                                        .ToDictionary(a => a.name, a => a.seriesIdNormal);

        WriteToFile(otomoArmorSeriesByName, "OtomoArmorSeriesByName.json");
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
}