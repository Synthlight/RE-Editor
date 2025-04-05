namespace RE_Editor.Common.PakModels;

public struct InfoByHash(string filename, string knownGoodPak, string sourcePak, long length, bool doesFileSizeMatchGoodFileSize) {
    public readonly string filename                      = filename;
    public readonly string knownGoodPak                  = knownGoodPak;
    public readonly string sourcePak                     = sourcePak;
    public readonly long   length                        = length;
    public readonly bool   doesFileSizeMatchGoodFileSize = doesFileSizeMatchGoodFileSize;
}