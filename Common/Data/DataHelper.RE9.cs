using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS8618

namespace RE_Editor.Common.Data;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static partial class DataHelper {
    public static Dictionary<Global.LangIndex, Dictionary<string, string>> ITEM_NAME_LOOKUP_BY_VALUE;
}