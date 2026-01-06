using System.Diagnostics.CodeAnalysis;
using RE_Editor.Common;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Data;
using RE_Editor.Models.Enums;

namespace RE_Editor.Models.Structs;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class App_user_data_WeaponData_cData {
    [SortOrder(50)]
    public string Name_ => DataHelper.WEAPON_INFO_LOOKUP_BY_GUID[Global.locale].TryGet(Name.Value);

    [SortOrder(int.MaxValue)]
    public string Description_ => DataHelper.WEAPON_INFO_LOOKUP_BY_GUID[Global.locale].TryGet(Explain.Value);

    public override string ToString() {
        return Name_;
    }

    public int GetUsedEnumIdValue() {
        if (LongSword != App_WeaponDef_LongSwordId_Fixed.INVALID) return (int) LongSword;
        if (ShortSword != App_WeaponDef_ShortSwordId_Fixed.INVALID) return (int) ShortSword;
        if (TwinSword != App_WeaponDef_TwinSwordId_Fixed.INVALID) return (int) TwinSword;
        if (Tachi != App_WeaponDef_TachiId_Fixed.INVALID) return (int) Tachi;
        if (Hammer != App_WeaponDef_HammerId_Fixed.INVALID) return (int) Hammer;
        if (Whistle != App_WeaponDef_WhistleId_Fixed.INVALID) return (int) Whistle;
        if (Lance != App_WeaponDef_LanceId_Fixed.INVALID) return (int) Lance;
        if (GunLance != App_WeaponDef_GunLanceId_Fixed.INVALID) return (int) GunLance;
        if (SlashAxe != App_WeaponDef_SlashAxeId_Fixed.INVALID) return (int) SlashAxe;
        if (ChargeAxe != App_WeaponDef_ChargeAxeId_Fixed.INVALID) return (int) ChargeAxe;
        if (Rod != App_WeaponDef_RodId_Fixed.INVALID) return (int) Rod;
        if (LightBowgun != App_WeaponDef_LightBowgunId_Fixed.INVALID) return (int) LightBowgun;
        if (HeavyBowgun != App_WeaponDef_HeavyBowgunId_Fixed.INVALID) return (int) HeavyBowgun;
        if (Bow != App_WeaponDef_BowId_Fixed.INVALID) return (int) Bow;
        throw new NotImplementedException("No valid weapon enum value found.");
    }

    public string GetWeaponType() {
        if (LongSword != App_WeaponDef_LongSwordId_Fixed.INVALID) return "LongSword";
        if (ShortSword != App_WeaponDef_ShortSwordId_Fixed.INVALID) return "ShortSword";
        if (TwinSword != App_WeaponDef_TwinSwordId_Fixed.INVALID) return "TwinSword";
        if (Tachi != App_WeaponDef_TachiId_Fixed.INVALID) return "Tachi";
        if (Hammer != App_WeaponDef_HammerId_Fixed.INVALID) return "Hammer";
        if (Whistle != App_WeaponDef_WhistleId_Fixed.INVALID) return "Whistle";
        if (Lance != App_WeaponDef_LanceId_Fixed.INVALID) return "Lance";
        if (GunLance != App_WeaponDef_GunLanceId_Fixed.INVALID) return "GunLance";
        if (SlashAxe != App_WeaponDef_SlashAxeId_Fixed.INVALID) return "SlashAxe";
        if (ChargeAxe != App_WeaponDef_ChargeAxeId_Fixed.INVALID) return "ChargeAxe";
        if (Rod != App_WeaponDef_RodId_Fixed.INVALID) return "Rod";
        if (LightBowgun != App_WeaponDef_LightBowgunId_Fixed.INVALID) return "LightBowgun";
        if (HeavyBowgun != App_WeaponDef_HeavyBowgunId_Fixed.INVALID) return "HeavyBowgun";
        if (Bow != App_WeaponDef_BowId_Fixed.INVALID) return "Bow";
        throw new NotImplementedException("No valid weapon enum value found.");
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class App_user_data_OuterWeaponData_cData {
    [SortOrder(50)]
    public string Name_ => DataHelper.WEAPON_LAYERED_INFO_LOOKUP_BY_GUID[Global.locale].TryGet(Name.Value);

    public override string ToString() {
        return Name_;
    }

    // I'd put `GetUsedEnumIdValue` / `GetWeaponType` here, but the files don't use the columns, despite having all the same ones.
}