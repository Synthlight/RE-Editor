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
public class InfiniteAmmo : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Infinite Ammo and Coatings";
        const string description = "Infinite Ammo and Coatings.";
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
                .SetAction(InfiniteAmmoAndCoatings),
            baseLuaMod
                .SetName($"{name} (REF)")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.ITEM_DATA,
                        Action = InfiniteAmmoAndCoatingsRef
                    }
                ])
                .SetSkipPak(true)
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void InfiniteAmmoAndCoatings(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_ItemData_cData item:
                    if (item.Type is App_ItemDef_TYPE_Fixed.SHELL or App_ItemDef_TYPE_Fixed.BOTTLE) {
                        item.Infinit = true;
                    }
                    break;
            }
        }
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static void InfiniteAmmoAndCoatingsRef(StreamWriter writer) {
        writer.WriteLine($"    if (entry._Type == {(int) App_ItemDef_TYPE_Fixed.SHELL} or entry._Type == {(int) App_ItemDef_TYPE_Fixed.BOTTLE}) then");
        writer.WriteLine("        entry._Infinit = true");
        writer.WriteLine("    end");
    }
}