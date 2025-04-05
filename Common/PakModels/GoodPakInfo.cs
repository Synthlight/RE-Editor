namespace RE_Editor.Common.PakModels;

public struct GoodPakInfo(string filename, string knownGoodPak, PakFileInfo goodPakInfo) {
    public readonly string            filename     = filename;
    public readonly string            knownGoodPak = knownGoodPak;
    public readonly PakFileInfo       goodPakInfo  = goodPakInfo;
    public readonly List<PakFileInfo> badPakInfo   = [];
}