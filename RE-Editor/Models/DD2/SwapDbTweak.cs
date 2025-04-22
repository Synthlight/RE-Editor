#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using RE_Editor.Common.Models;
using RE_Editor.Models.Enums;

namespace RE_Editor.Models;

public interface ISwapDbTweak {
    string                   LuaName { get; set; }
    List<SwapDbTweak.Change> Changes { get; set; }
}

public struct SwapDbTweak : INexusMod, ISwapDbTweak {
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
    public string?                      Requirement     { get; set; }
    public bool                         SkipPak         { get; set; }
    public Dictionary<string, object>?  AdditionalFiles { get; set; }
    public string                       LuaName         { get; set; }
    public List<Change>                 Changes         { get; set; }

    public struct Change {
        public string               Database { get; set; }
        public App_Gender           Gender   { get; set; }
        public Action<StreamWriter> Action   { get; set; }
    }
}

public static class SwapDbTweakExtensions {
    public static T SetLuaName<T>(this T nexusMod, string luaName) where T : ISwapDbTweak {
        nexusMod.LuaName = luaName;
        return nexusMod;
    }

    public static T SetChanges<T>(this T nexusMod, List<SwapDbTweak.Change> changes) where T : ISwapDbTweak {
        nexusMod.Changes = changes;
        return nexusMod;
    }
}