using System.Diagnostics.CodeAnalysis;
using RE_Editor.Common;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Data;

namespace RE_Editor.Models.Structs;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class App_user_data_OtArmorData_cData {
    [SortOrder(50)]
    public string Name_ => DataHelper.OTOMO_INFO_LOOKUP_BY_GUID[Global.locale].TryGet(Name.Value);

    [SortOrder(int.MaxValue)]
    public string Description_ => DataHelper.OTOMO_INFO_LOOKUP_BY_GUID[Global.locale].TryGet(Explain.Value);

    public override string ToString() {
        return Name_;
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class App_user_data_OtomoEquipSeriesData_cData {
    [SortOrder(50)]
    public string Name_ => DataHelper.OTOMO_SERIES_INFO_LOOKUP_BY_GUID[Global.locale].TryGet(Name.Value);

    public override string ToString() {
        return Name_;
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class App_user_data_OtomoOuterArmorData_cData {
    [SortOrder(50)]
    public string Name_ => DataHelper.OTOMO_LAYERED_INFO_LOOKUP_BY_GUID[Global.locale].TryGet(Name.Value);

    public override string ToString() {
        return Name_;
    }
}