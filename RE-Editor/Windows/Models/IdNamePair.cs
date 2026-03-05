using JetBrains.Annotations;
using RE_Editor.Common;

namespace RE_Editor.Windows.Models;

[UsedImplicitly]
public struct IdNamePair<T> {
    public readonly T      id;
    public readonly string name;
    public readonly bool   showAsHex;

    public IdNamePair(T id, string name, bool showAsHex) {
        this.id        = id;
        this.name      = name;
        this.showAsHex = showAsHex;
    }

    public static IdNamePair<T> Unknown(T id) {
        return new(id, "Unknown", false);
    }

    public override string ToString() {
        return name.ToStringWithId(id, showAsHex);
    }

    public override int GetHashCode() {
        return id.GetHashCode();
    }

    public bool Equals(IdNamePair<T> other) {
        return id.Equals(other.id);
    }

    public override bool Equals(object obj) {
        return obj is IdNamePair<T> other && Equals(other);
    }

    public static bool operator ==(IdNamePair<T> left, IdNamePair<T> right) {
        return left.Equals(right);
    }

    public static bool operator !=(IdNamePair<T> left, IdNamePair<T> right) {
        return !left.Equals(right);
    }
}