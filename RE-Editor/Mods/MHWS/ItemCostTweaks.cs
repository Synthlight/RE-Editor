using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class ItemCostTweaks : IMod {
    private const int ITEM_BUY_PRICE  = 1;
    private const int ITEM_SELL_PRICE = 99999;

    [UsedImplicitly]
    public static void Make() {
        const string name        = "Item Cost Tweaks - Buy for 1z, sell for 99999z";
        const string description = "Buy for 1z, sell for 99999z.";
        const string version     = "1.2";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Shop.png"
        };

        var baseLuaMod = new VariousDataTweak {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Shop.png"
        };

        var mods = new List<INexusMod> {
            baseMod
                .SetName("Item Cost Tweaks - Buy Price Only (PAK)")
                .SetFiles([PathHelper.ITEM_DATA_PATH])
                .SetAction(list => ItemCostTweak(list, Mode.BUY_PRICE)),
            baseLuaMod
                .SetName("Item Cost Tweaks - Buy Price Only (REF)")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.ITEM_DATA,
                        Action = writer => { ItemCostTweakRef(writer, Mode.BUY_PRICE); }
                    }
                ])
                .SetSkipPak(true),
            baseMod
                .SetName("Item Cost Tweaks - Sell Price Only (PAK)")
                .SetFiles([PathHelper.ITEM_DATA_PATH])
                .SetAction(list => ItemCostTweak(list, Mode.SELL_PRICE)),
            baseLuaMod
                .SetName("Item Cost Tweaks - Sell Price Only (REF)")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.ITEM_DATA,
                        Action = writer => { ItemCostTweakRef(writer, Mode.SELL_PRICE); }
                    }
                ])
                .SetSkipPak(true),
            baseMod
                .SetName("Item Cost Tweaks - Sell & Buy Price (PAK)")
                .SetFiles([PathHelper.ITEM_DATA_PATH])
                .SetAction(list => ItemCostTweak(list, Mode.BUY_PRICE | Mode.SELL_PRICE))
        };

        ModMaker.WriteMods(mods, name, copyLooseToFluffy: true);
    }

    public static void ItemCostTweak(IList<RszObject> rszObjectData, Mode mode) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_ItemData_cData item:
                    if (item.Type != App_ItemDef_TYPE_Fixed.POINT) {
                        if (mode.HasFlag(Mode.BUY_PRICE)) item.BuyPrice = ITEM_BUY_PRICE;
                    }
                    if (mode.HasFlag(Mode.SELL_PRICE)) item.SellPrice = ITEM_SELL_PRICE;
                    break;
            }
        }
    }

    public static void ItemCostTweakRef(StreamWriter writer, Mode mode) {
        if (mode.HasFlag(Mode.BUY_PRICE)) {
            writer.WriteLine($"    if (entry._Type ~= {(int) App_ItemDef_TYPE_Fixed.POINT}) then");
            writer.WriteLine($"        entry._BuyPrice = {ITEM_BUY_PRICE}");
            writer.WriteLine("    end");
        }
        if (mode.HasFlag(Mode.SELL_PRICE)) {
            writer.WriteLine($"    entry._SellPrice = {ITEM_SELL_PRICE}");
        }
    }

    [Flags]
    public enum Mode {
        BUY_PRICE = 1,
        SELL_PRICE
    }
}