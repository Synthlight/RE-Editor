using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Constants;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class InfiniteConsumables : IMod {
    private static readonly List<App_ItemDef_ID_Fixed> COVERED_ITEMS = [
        ItemConstants.POTION,
        ItemConstants.MEGA_POTION,
        ItemConstants.ANTIDOTE,
        ItemConstants.MAX_POTION,
        ItemConstants.HERBAL_MEDICINE,
        ItemConstants.NULBERRY,
        ItemConstants.WELL_DONE_STEAK,
        ItemConstants.RARE_STEAK,
        ItemConstants.LIFEPOWDER,
        ItemConstants.BARREL_BOMB,
        ItemConstants.LARGE_BARREL_BOMB,
        ItemConstants.FLASH_POD,
        ItemConstants.PITFALL_TRAP,
        ItemConstants.SHOCK_TRAP,
        ItemConstants.ANTIDOTE_HERB,
        ItemConstants.DUNG_POD,
        ItemConstants.LARGE_DUNG_POD,
        ItemConstants.LURING_POD,
        ItemConstants.SCREAMER_POD,
        ItemConstants.ARMORSKIN,
        ItemConstants.DEMONDRUG,
        ItemConstants.TRANQ_BOMB,
        ItemConstants.SMOKE_BOMB,
        ItemConstants.POISON_SMOKE_BOMB,
        ItemConstants.WHETFISH_FIN,
        ItemConstants.WHETFISH_FIN_PLUS,
        ItemConstants.FARCASTER
    ];

    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Infinite Consumables";
        const string description = "Infinite Consumables.";
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
                .SetAction(InfiniteConsumableItems),
            baseLuaMod
                .SetName($"{name} (REF)")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.ITEM_DATA,
                        Action = InfiniteConsumableItemsRef
                    }
                ])
                .SetSkipPak(true)
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void InfiniteConsumableItems(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_ItemData_cData item:
                    if (COVERED_ITEMS.Contains((App_ItemDef_ID_Fixed) item.ItemId)) {
                        item.Infinit = true;
                    }
                    break;
            }
        }
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static void InfiniteConsumableItemsRef(StreamWriter writer) {
        foreach (var itemId in COVERED_ITEMS) {
            writer.WriteLine($"    if (entry._ItemId == {itemId}) then");
            writer.WriteLine("        entry._Infinit = true");
            writer.WriteLine("    end");
        }
    }
}