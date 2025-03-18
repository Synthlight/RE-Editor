namespace RE_Editor.Common.Models;

public struct TwoGenericsInfo {
    private readonly StructJson.Field field;
    public readonly  GenericTypeInfo  type1;
    public readonly  GenericTypeInfo  type2;

    public TwoGenericsInfo(StructJson.Field field) {
        this.field = field;
        var openBrace  = field.originalType!.IndexOf('<');
        var comma      = field.originalType!.IndexOf(',');
        var closeBrace = field.originalType!.LastIndexOf('>');
        var rawType1   = field.originalType!.Substring(openBrace + 1, comma - openBrace - 1);
        type1 = new(rawType1);
        var rawType2 = field.originalType!.Substring(comma + 1, closeBrace - comma - 1);
        type2 = new(rawType2);
    }

    public readonly struct GenericTypeInfo {
        public readonly string rawType;
        public readonly string name;
        public readonly string convertedName;
        public readonly bool   isArray;

        public string AsArrayTypeName => isArray ? $"List<{convertedName}>" : convertedName;
        public Type   AsArrayType     => isArray ? typeof(List<>).MakeGenericType(convertedName.GetAsType()!) : convertedName.GetAsType()!;

        public GenericTypeInfo(string rawType) {
            this.rawType = rawType;
            if (rawType.StartsWith("System.Collections.Generic.List")) {
                var openBrace  = rawType.IndexOf('<');
                var closeBrace = rawType.LastIndexOf('>');
                name    = rawType.Substring(openBrace + 1, closeBrace - openBrace - 1);
                isArray = true;
            } else if (rawType.EndsWith("[]")) {
                name    = rawType[..^2];
                isArray = true;
            } else {
                name = rawType;
            }
            convertedName = name.ToConvertedTypeName()!;
        }
    }
}