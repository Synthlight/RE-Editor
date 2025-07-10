using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Common.Models.List_Wrappers;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class RiceBallCharmRecipe : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Rice Ball Charm Recipe (Not Replacer)";
        const string description = "Adds a Rice Ball Charm recipe to the recipe list.";
        const string version     = "1.0";

        var mod = new NexusMod {
            Name    = name,
            Version = version,
            Desc    = description,
            Files   = [PathHelper.TALISMAN_RECIPE_DATA_PATH],
            Action  = ModStuff
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true);
    }

    private static void ModStuff(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_AmuletRecipeData recipeData:
                    var riceBall = App_user_data_AmuletRecipeData_cData.Create(recipeData.rsz);
                    riceBall.DataId         = 9001;
                    riceBall.AmuletType     = App_ArmorDef_AmuletType_Fixed.AT_0183;
                    riceBall.Lv             = 1;
                    riceBall.KeyItemId      = (int) App_ItemDef_ID_Fixed.NONE;
                    riceBall.KeyEnemyId     = (int) App_EnemyDef_ID_Fixed.INVALID;
                    riceBall.KeyStoryNo     = (int) App_MissionIDList_ID_Fixed.INVALID;
                    riceBall.FlagHunterRank = 0;
                    riceBall.ItemId = new([
                        new DataSourceWrapper<int>(0, (int) App_ItemDef_ID_Fixed.NONE, null!),
                        new DataSourceWrapper<int>(0, (int) App_ItemDef_ID_Fixed.NONE, null!),
                        new DataSourceWrapper<int>(0, (int) App_ItemDef_ID_Fixed.NONE, null!),
                        new DataSourceWrapper<int>(0, (int) App_ItemDef_ID_Fixed.NONE, null!)
                    ]);
                    riceBall.ItemNum = new([
                        new GenericWrapper<uint>(0, 0),
                        new GenericWrapper<uint>(0, 0),
                        new GenericWrapper<uint>(0, 0),
                        new GenericWrapper<uint>(0, 0)
                    ]);
                    recipeData.Values.Add(riceBall);
                    break;
            }
        }
    }
}