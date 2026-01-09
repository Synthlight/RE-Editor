using System;
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
public class DumpItemsForCE : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        using var normalIdWriter = new StreamWriter(File.Open($@"{PathHelper.MODS_PATH}\..\Dumped Data\Items (Normal ID, CE).txt", FileMode.Create, FileAccess.Write, FileShare.Read));
        using var fixedIdWriter  = new StreamWriter(File.Open($@"{PathHelper.MODS_PATH}\..\Dumped Data\Items (Fixed ID, CE).txt", FileMode.Create, FileAccess.Write, FileShare.Read));

        var itemEnumNames = Enum.GetNames<App_ItemDef_ID>();

        foreach (var enumName in itemEnumNames) {
            var normalIdValue = (int) Enum.Parse<App_ItemDef_ID>(enumName);
            var fixedIdValue  = (int) Enum.Parse<App_ItemDef_ID_Fixed>(enumName);

            switch (enumName) {
                case nameof(App_ItemDef_ID.MAX): continue;
                case nameof(App_ItemDef_ID.NONE):
                case nameof(App_ItemDef_ID.INVALID):
                    normalIdWriter.WriteLine($"{normalIdValue}:{enumName}");
                    fixedIdWriter.WriteLine($"{fixedIdValue}:{enumName}");
                    break;
                default:
                    if (DataHelper.ITEM_NAME_LOOKUP[Global.LangIndex.eng].TryGetValue((uint) fixedIdValue, out var name)) {
                        normalIdWriter.WriteLine($"{normalIdValue}:{name}");
                        fixedIdWriter.WriteLine($"{fixedIdValue}:{name}");
                    }
                    break;
            }
        }
    }
}