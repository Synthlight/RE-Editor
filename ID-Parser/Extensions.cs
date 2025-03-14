using RE_Editor.Common;

namespace RE_Editor.ID_Parser;

public static class Extensions {
    public static Dictionary<Global.LangIndex, Dictionary<TOut, string>> ConvertTo<TIn, TOut>(this Dictionary<Global.LangIndex, Dictionary<TIn, string>> inDict) where TIn : Enum
                                                                                                                                                                 where TOut : notnull {
        var outDict = new Dictionary<Global.LangIndex, Dictionary<TOut, string>>(inDict.Count);
        foreach (var (lang, innerDict) in inDict) {
            outDict[lang] = new(innerDict.Count);
            foreach (var (key, value) in innerDict) {
                var outKey = (TOut) Convert.ChangeType(key, typeof(TOut));
                outDict[lang][outKey] = value;
            }
        }
        return outDict;
    }
}