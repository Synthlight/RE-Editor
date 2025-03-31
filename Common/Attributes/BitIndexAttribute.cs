namespace RE_Editor.Common.Attributes;

public class BitIndexAttribute(int value) : Attribute {
    public readonly int value = value;
}