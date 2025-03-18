using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RE_Editor.Common.Attributes;

namespace RE_Editor.Common.Models;

[UsedImplicitly]
public class StructJson {
    [UsedImplicitly] public string?      crc;
    [UsedImplicitly] public List<Field>? fields;
    [UsedImplicitly] public string?      name;
    [UsedImplicitly] public string?      parent;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [JsonIgnore] public Dictionary<string, Field> fieldNameMap;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    [OnDeserialized]
    [UsedImplicitly]
    public void Init(StreamingContext context) {
        fieldNameMap = [];
        if (fields == null) return;
        foreach (var field in fields) {
            if (field.name == null) continue;
            fieldNameMap[field.name] = field;
        }
    }

    public override string? ToString() {
        return name ?? base.ToString();
    }

    public class Field : IEquatable<Field> {
        [UsedImplicitly] public int     align;
        [UsedImplicitly] public bool    array;
        [UsedImplicitly] public string? name;
        [UsedImplicitly] public bool    native;
        [JsonProperty("original_type")]
        [UsedImplicitly] public string? originalType;
        [UsedImplicitly] public int     size;
        [UsedImplicitly] public string? type;

        // The two should be updated together, and are done so in `StructType.UpdateUsingCounts`.
        [JsonIgnore] public int overrideCount; // Used by children to mark they need to override a parent field.
        [JsonIgnore] public int virtualCount; // Used by children to mark their parent's fields as overwritten.

        [JsonIgnore] public DataSourceType? buttonType;
        [JsonIgnore] public string?         buttonPrimitive;

        [JsonIgnore] public TwoGenericsInfo? twoGenericsInfo; // For Tuples and Dictionaries and such.

        public override string? ToString() {
            return name ?? base.ToString();
        }

        public bool Equals(Field? other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return align == other.align && array == other.array && name == other.name && native == other.native && originalType == other.originalType && size == other.size && type == other.type;
        }

        public override bool Equals(object? obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Field) obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            return HashCode.Combine(align, array, name, native, originalType, size, type);
        }

        public static bool operator ==(Field? left, Field? right) {
            return Equals(left, right);
        }

        public static bool operator !=(Field? left, Field? right) {
            return !Equals(left, right);
        }

        /// <summary>
        /// Copies serializable fields only.
        /// Things such as button types or use counts are not copied.
        /// </summary>
        /// <returns></returns>
        public Field Copy() {
            return new() {
                align        = align,
                array        = array,
                name         = name,
                native       = native,
                originalType = originalType,
                size         = size,
                type         = type,
            };
        }
    }

    public string? GetGenericParam() {
        if (name == null) return null;
        if (!name.Contains('<')) return null;
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (name.Contains(',')) {
            return name.SubstringToEnd(name.IndexOf('<') + 1, name.IndexOf(','));
        }
        return name.SubstringToEnd(name.IndexOf('<') + 1, name.IndexOf('>'));
    }
}