using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Data;

namespace RE_Editor.Common.Util;

public static class Utils {
    public static bool IsRunningFromUnitTests { get; private set; }

    static Utils() {
        IsRunningFromUnitTests = AppDomain.CurrentDomain.GetAssemblies().Any(assem => assem.FullName!.ToLowerInvariant().StartsWith("testhost"));
    }

    public static void SetupKeybind(UIElement control, InputGesture gesture, Action command) {
        var changeItemValues = new RoutedCommand();
        var ib               = new InputBinding(changeItemValues, gesture);
        control.InputBindings.Add(ib);
        // Bind handler.
        var cb = new CommandBinding(changeItemValues);
        cb.Executed += (_, _) => command();
        control.CommandBindings.Add(cb);
    }

    public static dynamic GetDataSourceLookup(string? originalType) {
        return GetDataSourceLookup(GetDataSourceType(originalType));
    }

    public static dynamic GetDataSourceLookup(DataSourceType? dataSourceType) {
        dynamic dataSource = dataSourceType switch {
#if DD2
            DataSourceType.ITEMS => DataHelper.ITEM_NAME_LOOKUP[Global.locale],
#elif DRDR
            DataSourceType.ITEMS => DataHelper.ITEM_NAME_LOOKUP[Global.locale],
#elif MHR
            DataSourceType.DANGO_SKILLS => DataHelper.DANGO_SKILL_NAME_LOOKUP[Global.locale],
            DataSourceType.ITEMS => DataHelper.ITEM_NAME_LOOKUP[Global.locale],
            DataSourceType.RAMPAGE_SKILLS => DataHelper.RAMPAGE_SKILL_NAME_LOOKUP[Global.locale],
            DataSourceType.SKILLS => DataHelper.SKILL_NAME_LOOKUP[Global.locale],
            DataSourceType.SWITCH_SKILLS => DataHelper.SWITCH_SKILL_NAME_LOOKUP[Global.locale],
#elif MHWS
            DataSourceType.ARMOR_SERIES => DataHelper.ARMOR_SERIES_BY_ENUM_VALUE[Global.locale],
            DataSourceType.DECORATIONS => DataHelper.DECORATION_INFO_LOOKUP_BY_ENUM_VALUE[Global.locale],
            DataSourceType.ENEMIES => DataHelper.ENEMY_NAME_LOOKUP_BY_ENUM_VALUE[Global.locale],
            DataSourceType.ITEMS => DataHelper.ITEM_NAME_LOOKUP[Global.locale],
            DataSourceType.MEDALS => DataHelper.MEDAL_NAME_LOOKUP_BY_ENUM_VALUE[Global.locale],
            DataSourceType.NPCS => DataHelper.NPC_NAME_LOOKUP_BY_ENUM_VALUE[Global.locale],
            DataSourceType.QUESTS => DataHelper.QUEST_INFO_LOOKUP_BY_ENUM_VALUE[Global.locale],
            DataSourceType.SKILLS => DataHelper.SKILL_NAME_BY_ENUM_VALUE[Global.locale],
            DataSourceType.PENDANTS => DataHelper.PENDANT_NAME_LOOKUP_BY_ENUM_VALUE[Global.locale],
            DataSourceType.WEAPON_SERIES => DataHelper.WEAPON_SERIES_BY_ENUM_VALUE[Global.locale],
#elif RE4
            DataSourceType.ITEMS => DataHelper.ITEM_NAME_LOOKUP[Global.variant][Global.locale],
            DataSourceType.WEAPONS => DataHelper.WEAPON_NAME_LOOKUP[Global.variant][Global.locale],
#endif
            _ => throw new ArgumentOutOfRangeException(dataSourceType.ToString())
        };
        return dataSource;
    }

    public static DataSourceType? GetDataSourceType(string? originalType) {
        return originalType?.Replace("[]", "") switch {
#if DD2
            "app.ItemIDEnum" => DataSourceType.ITEMS,
#elif DRDR
            "app.MTData.ITEM_NO" => DataSourceType.ITEMS,
#elif MHR
            "snow.data.ContentsIdSystem.ItemId" => DataSourceType.ITEMS,
            "snow.data.DataDef.PlEquipSkillId" => DataSourceType.SKILLS,
            "snow.data.DataDef.PlHyakuryuSkillId" => DataSourceType.RAMPAGE_SKILLS,
            "snow.data.DataDef.PlKitchenSkillId" => DataSourceType.DANGO_SKILLS,
            "snow.data.DataDef.PlWeaponActionId" => DataSourceType.SWITCH_SKILLS,
#elif MHWS
            "app.ArmorDef.SERIES_Fixed" => DataSourceType.ARMOR_SERIES,
            "app.EnemyDef.ID_Fixed" => DataSourceType.ENEMIES,
            "app.EquipDef.ACCESSORY_ID_Fixed" => DataSourceType.DECORATIONS,
            "app.HunterDef.Skill_Fixed" => DataSourceType.SKILLS,
            "app.HunterProfileDef.MEDAL_ID_Fixed" => DataSourceType.MEDALS,
            "app.ItemDef.ID_Fixed" => DataSourceType.ITEMS,
            "app.MissionIDList.ID_Fixed" => DataSourceType.QUESTS,
            "app.NpcDef.ID_Fixed" => DataSourceType.NPCS,
            "app.WeaponCharmDef.TYPE_Fixed" => DataSourceType.PENDANTS,
            "app.WeaponDef.SERIES_Fixed" => DataSourceType.WEAPON_SERIES,
#elif RE2
            "app.ropeway.gamemastering.Item.ID" => DataSourceType.ITEMS,
            "app.ropeway.EquipmentDefine.WeaponType" => DataSourceType.WEAPONS,
#elif RE3
            "offline.EquipmentDefine.WeaponType" => DataSourceType.WEAPONS,
            "offline.gamemastering.Item.ID" => DataSourceType.ITEMS,
#elif RE4
            "chainsaw.ItemID" => DataSourceType.ITEMS,
            "chainsaw.WeaponID" => DataSourceType.WEAPONS,
#endif
            _ => null
        };
    }

    public static ConstructorInfo? GetGenericConstructor(Type target, List<Type> args, Type genericType) {
        return (from constructor in target.MakeGenericType(genericType).GetConstructors()
                let parameters = constructor.GetParameters()
                where args.Count == parameters.Length
                from parameter in parameters
                from arg in args
                where parameter.ParameterType.GetGenericTypeDefinition() == arg
                select constructor).FirstOrDefault();
    }

    public static List<Type> GetTypesInList<T>(IEnumerable<T> items) where T : notnull {
        return items.Select(item => item.GetType()).Distinct().ToList();
    }

    /***
     * A way of getting types from a list without using generics.
     */
    public static List<Type> GetTypesInList(IEnumerable items) {
        var types      = new List<Type>();
        var enumerator = items.GetEnumerator();
        while (enumerator.MoveNext()) {
            var type = enumerator.Current!.GetType();
            if (!types.Contains(type)) types.Add(type);
        }
        ((IDisposable) enumerator).Dispose();
        return types;
    }

    public static void RunOnUiThread(Action action) {
        Application.Current.Dispatcher.Invoke(action);
    }

    public static async Task RunOnUiThreadAsync(Action action) {
        await Application.Current.Dispatcher.InvokeAsync(action);
    }
}