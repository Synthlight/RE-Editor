using Newtonsoft.Json;

namespace RE_Editor.Common.PakModels;

public struct PakFileInfo(string filename, string hash, long length, string pak) {
    [JsonIgnore] public readonly string filename = filename;
    public readonly              string hash     = hash;
    public readonly              long   length   = length;
    public readonly              string pak      = pak;
}