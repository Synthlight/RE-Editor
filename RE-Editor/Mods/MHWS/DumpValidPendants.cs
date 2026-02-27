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
public class DumpValidPendants : IMod {
    private const string ROOT_OUT_PATH = $@"{PathHelper.MODS_PATH}\Layered-Pendant-Unlocker";

    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        DumpValidPendantsByName();
    }

    private static void DumpValidPendantsByName() {
        var pendantData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.PENDANT_DATA_PATH}").rsz.GetEntryObject<App_user_data_CharmData>().Values.Cast<App_user_data_CharmData_cData>().ToList();

        var validPendantDataByName = (from pendant in pendantData
                                      where pendant.Type_Unwrapped != App_WeaponCharmDef_TYPE_Fixed.NONE && pendant.Type_Unwrapped != App_WeaponCharmDef_TYPE_Fixed.MAX
                                      where DataHelper.PENDANT_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng].ContainsKey(pendant.Name)
                                      let pendantName = DataHelper.PENDANT_INFO_LOOKUP_BY_GUID[Global.LangIndex.eng][pendant.Name]
                                      orderby pendantName
                                      let typeIdName = Enum.GetName(pendant.Type_Unwrapped)
                                      let typeIdNormal = Enum.Parse<App_WeaponCharmDef_TYPE>(typeIdName)
                                      select new {pendantName, typeIdNormal}).ToDictionary(a => a.pendantName, a => a.typeIdNormal);

        WriteToFile(validPendantDataByName, $@"{ROOT_OUT_PATH}\Assets\ValidPendantsByName.json");
    }

    private static void WriteToFile(object data, string path) {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        var dir  = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
        var writer = new StreamWriter(File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read));
        writer.Write(json);
        writer.Close();
    }
}