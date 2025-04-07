namespace RE_Editor.Obsolete_Detector.Models;

public struct ObsoleteFileData {
    public string  path;
    public string  obsoletedBy;
    public Reason  reason;
    public string? pak;
    public string? modName;

    public string GetReasonText() {
        return reason switch {
            Reason.CRC => "CRC",
            Reason.DATE => "Date",
            Reason.HASH => "Hash",
            Reason.LENGTH => "Length",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public enum Reason {
    CRC,
    DATE,
    HASH,
    LENGTH
}