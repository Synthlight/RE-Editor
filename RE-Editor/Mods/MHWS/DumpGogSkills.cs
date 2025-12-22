using System.IO;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class DumpGogSkills : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        var skillGroupData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}{PathHelper.GOG_SKILL_GROUP_DATA_PATH}").rsz.GetEntryObject<App_user_data_ArtianSkillGroupData>().Values.Cast<App_user_data_ArtianSkillGroupData_cData>().ToList();

        var writer = new StreamWriter(File.Open($@"{PathHelper.MODS_PATH}\..\Dumped Data\Gog Skill Group Data.csv", FileMode.Create, FileAccess.Write, FileShare.Read));
        writer.WriteLine("ArtianSkillType,GroupSkillId,SeriesSkillId,Probability");
        foreach (var data in skillGroupData) {
            writer.WriteLine($"{data.ArtianSkillType},{DataHelper.SKILL_NAME_BY_ENUM_VALUE[Global.locale][data.GroupSkillId]},{DataHelper.SKILL_NAME_BY_ENUM_VALUE[Global.locale][data.SeriesSkillId]},{data.Probability}");
        }
        writer.Close();
    }
}