﻿using System.Diagnostics.CodeAnalysis;
using RE_Editor.Common;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Data;

namespace RE_Editor.Models.Structs;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class App_user_data_ArmorData_cData {
    [SortOrder(50)]
    public string Name_ => DataHelper.ARMOR_INFO_LOOKUP_BY_GUID[Global.locale].TryGet(Name.Value);

    [SortOrder(int.MaxValue)]
    public string Description_ => DataHelper.ARMOR_INFO_LOOKUP_BY_GUID[Global.locale].TryGet(Explain.Value);

    public override string ToString() {
        return Name_;
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class App_user_data_ArmorSeriesData_cData {
    [SortOrder(50)]
    public string Name_ => DataHelper.ARMOR_SERIES_INFO_LOOKUP_BY_GUID[Global.locale].TryGet(Name.Value);

    public override string ToString() {
        return Name_;
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class App_user_data_OuterArmorData_cData {
    [SortOrder(50)]
    public string Name_Male => DataHelper.ARMOR_LAYERED_INFO_LOOKUP_BY_GUID[Global.locale].TryGet(NameMale.Value);

    [SortOrder(51)]
    public string Name_Female => DataHelper.ARMOR_LAYERED_INFO_LOOKUP_BY_GUID[Global.locale].TryGet(NameFemale.Value);

    public override string ToString() {
        return Name_Male;
    }
}