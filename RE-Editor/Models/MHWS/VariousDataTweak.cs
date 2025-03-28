#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using RE_Editor.Common.Models;

namespace RE_Editor.Models;

public interface IVariousDataTweak : INexusMod {
    string                               LuaName { get; set; }
    public List<VariousDataTweak.Change> Changes { get; set; }
}

[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public struct VariousDataTweak : IVariousDataTweak {
    public string                       Name            { get; set; }
    public string?                      NameOverride    { get; set; }
    public string                       Desc            { get; set; }
    public string                       Version         { get; set; }
    public string?                      Image           { get; set; }
    public IEnumerable<string>          Files           { get; set; }
    public Action<List<RszObject>>?     Action          { get; set; }
    public Func<List<RszObject>, bool>? FilteredAction  { get; set; }
    public bool                         ForGp           { get; set; }
    public string?                      NameAsBundle    { get; set; }
    public string?                      AddonFor        { get; set; }
    public bool                         SkipPak         { get; set; }
    public Dictionary<string, object>?  AdditionalFiles { get; set; }
    public string                       LuaName         { get; set; }
    public List<Change>                 Changes         { get; set; }

    public struct Change {
        public Target               Target { get; set; }
        public Action<StreamWriter> Action { get; set; }
    }

    public enum Target {
        ARMOR_DATA,
        ARMOR_RECIPE_DATA,
        DECORATION_DATA,
        ITEM_DATA,
        INSECT_DATA,
        INSECT_RECIPE_DATA,
        PALICO_ARMOR_DATA,
        PALICO_WEAPON_DATA,
        PALICO_RECIPE_DATA,
        SKILL_DATA,
        TALISMAN_DATA,
        TALISMAN_RECIPE_DATA,
        WEAPON_DATA,
        WEAPON_RECIPE_DATA,
        WEAPON_TREE_DATA
    }
}

public static class ItemDbTweakExtensions {
    public static T SetLuaName<T>(this T nexusMod, string luaName) where T : IVariousDataTweak {
        nexusMod.LuaName = luaName;
        return nexusMod;
    }

    [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract")] // It's not guaranteed to be null, it's not unreachable.
    public static T SetDefaultLuaName<T>(this T nexusMod) where T : IVariousDataTweak {
        nexusMod.LuaName         ??= nexusMod.Name.Replace(' ', '_') + ".lua";
        nexusMod.Files           ??= [];
        nexusMod.AdditionalFiles ??= [];
        return nexusMod;
    }

    public static T SetChanges<T>(this T nexusMod, List<VariousDataTweak.Change> changes) where T : IVariousDataTweak {
        nexusMod.Changes = changes;
        return nexusMod;
    }
}