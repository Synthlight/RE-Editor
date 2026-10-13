using System.Diagnostics.CodeAnalysis;
using RE_Editor.Common.Models;
using RE_Editor.Models.Enums;

namespace RE_Editor.Models.Structs;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class App_ItemShopBuyParam {
    public static App_ItemShopBuyParam MakeNewBuyItem(RSZ rsz, int item, ushort stock) {
        var newItem = Create(rsz);
        newItem.ItemId                  = item;
        newItem.PriceRateList           = [];
        newItem.Stock                   = stock;
        newItem.AddStockDays            = 1;
        newItem.AddStockNum             = stock;
        newItem.ReleaseQuestId          = App_QuestDefine_ID.Invalid;
        newItem.QuestResultNo           = -1;
        newItem.ReleaseSentimentRankBit = 1;
        newItem.ReleaseTotalPay         = 0;
        newItem.ReleaseFlag             = 0;
        newItem.IsCheckJob              = false;

        newItem.QuestVariableCondition[0].QuestID   = App_QuestDefine_ID.Invalid;
        newItem.QuestVariableCondition[0].BoolValue = true;
        return newItem;
    }
}