using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Data;
using RE_Editor.Common.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class DumpEnemyIds : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        using var writer = new StreamWriter(File.Open($@"{PathHelper.MODS_PATH}\..\Dumped Data\Enemy IDs.csv", FileMode.Create, FileAccess.Write, FileShare.Read));

        var          enemyEnumIdNames = Enum.GetNames<App_EnemyDef_ID_Fixed>();
        List<string> lines            = new(enemyEnumIdNames.Length);

        writer.WriteLine("Name,Enum Name,ID,Fixed ID");

        foreach (var idName in enemyEnumIdNames) {
            var normalIdValue = (int) Enum.Parse<App_EnemyDef_ID>(idName);
            var fixedIdValue  = (int) Enum.Parse<App_EnemyDef_ID_Fixed>(idName);
            var realName      = DataHelper.ENEMY_NAME_LOOKUP_BY_ENUM_VALUE[Global.LangIndex.eng].TryGetNull(fixedIdValue);

            if (realName == null) {
                Global.Log($"Unable to get a name for: {idName}");
                continue;
            }

            lines.Add($"{realName},{idName},{normalIdValue},{fixedIdValue}");
        }

        lines.Sort();
        foreach (var line in lines) {
            writer.WriteLine(line);
        }
    }
}