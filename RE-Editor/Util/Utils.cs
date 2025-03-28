#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using RE_Editor.Common;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Data;

namespace RE_Editor.Util;

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

    public static dynamic GetDataSourceType(DataSourceType? dataSourceType) {
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
            DataSourceType.ENEMIES => DataHelper.ENEMY_NAME_LOOKUP_BY_ENUM_VALUE[Global.locale],
            DataSourceType.ITEMS => DataHelper.ITEM_NAME_LOOKUP[Global.locale],
            DataSourceType.MEDALS => DataHelper.MEDAL_NAME_LOOKUP_BY_ENUM_VALUE[Global.locale],
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
}