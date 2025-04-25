using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class InfiniteIngredients : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Infinite Ingredients";
        const string description = "Infinite Ingredients.";
        const string version     = "1.0";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description
        };

        var baseLuaMod = new VariousDataTweak {
            Version      = version,
            NameAsBundle = name,
            Desc         = description
        };

        var mods = new List<INexusMod> {
            baseMod
                .SetName($"{name} (PAK)")
                .SetFiles([PathHelper.ITEM_DATA_PATH])
                .SetAction(InfiniteIngredientItems),
            baseLuaMod
                .SetName($"{name} (REF)")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.ITEM_DATA,
                        Action = InfiniteIngredientsRef
                    }
                ])
                .SetSkipPak(true)
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void InfiniteIngredientItems(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_ItemData_cData item:
                    if (item.AddIconType == App_IconDef_AddIcon_Fixed.INGREDIENTS) {
                        item.Infinit = true;
                    }
                    break;
            }
        }
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static void InfiniteIngredientsRef(StreamWriter writer) {
        writer.WriteLine($"    if (entry._AddIconType == {(int) App_IconDef_AddIcon_Fixed.INGREDIENTS}) then");
        writer.WriteLine("        entry._Infinit = true");
        writer.WriteLine("    end");
    }
}