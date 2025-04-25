using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Constants;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
[SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
[SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
[SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
public class MorePortCrystalsShopTweaks : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Ferrystones and Portcrystals in Every Shop";
        const string description = "Adds both items to every single shop in the game.";
        const string version     = "1.7";

        var mod = new NexusMod {
            Version = version,
            Name    = name,
            Desc    = description,
            Files   = [PathHelper.ITEM_SHOP_DATA_PATH],
            Action  = ShopData,
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true);
    }

    public static void ShopData(List<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_ItemShopParam shopData:
                    if (shopData.ShopId == 59) continue; // The `Seekers Token` shop which isn't really a shop.
                    if (shopData.BuyParams.Count > 0) {
                        shopData.BuyParams.Add(App_ItemShopBuyParam.MakeNewBuyItem(shopData.rsz, (int) ItemConstants.FERRYSTONE, 255));
                        shopData.BuyParams.Add(App_ItemShopBuyParam.MakeNewBuyItem(shopData.rsz, (int) ItemConstants.PORTCRYSTAL, 255));
                    }
                    break;
            }
        }
    }
}