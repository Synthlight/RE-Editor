namespace RE_Editor.Common.PakModels;

public struct PakDateInfo(DateTime releaseDate, string gameVersion, List<string> pakFiles) {
    public readonly DateTime     releaseDate = releaseDate;
    public readonly string       gameVersion = gameVersion;
    public readonly List<string> pakFiles    = pakFiles;
    public          string?      updateName;
}