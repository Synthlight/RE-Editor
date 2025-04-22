using JetBrains.Annotations;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Data;

namespace RE_Editor.Common.Models;

public class UserDataShell : RszObject {
    public readonly uint hash;

    public UserDataShell(uint hash, RSZ rsz) {
        this.hash  = hash;
        this.rsz   = rsz;
        structInfo = DataHelper.STRUCT_INFO[hash];
    }

    [SortOrder(500)]
    public string Value {
        get {
            if (userDataRef == -1) {
                userDataRef = rsz.AddUserDataRef(hash);
            }
            return rsz.userDataInfo[userDataRef].str;
        }
        [UsedImplicitly] set => rsz.userDataInfo[userDataRef].str = value;
    }

    public UserDataShell Copy() {
        return new(hash, rsz) {
            Value = Value
        };
    }

    public override string ToString() {
        return Value;
    }
}