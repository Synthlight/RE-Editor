using System.Diagnostics.CodeAnalysis;
using RE_Editor.Models.Enums;

namespace RE_Editor.Models.Structs;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class App_user_data_WeaponRecipeData_cData {
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
        if (Bow != App_WeaponDef_BowId_Fixed.INVALID) return (int) Bow;
        if (HeavyBowgun != App_WeaponDef_HeavyBowgunId_Fixed.INVALID) return (int) HeavyBowgun;
        if (LightBowgun != App_WeaponDef_LightBowgunId_Fixed.INVALID) return (int) LightBowgun;
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
        if (Bow != App_WeaponDef_BowId_Fixed.INVALID) return "Bow";
        if (HeavyBowgun != App_WeaponDef_HeavyBowgunId_Fixed.INVALID) return "HeavyBowgun";
        if (LightBowgun != App_WeaponDef_LightBowgunId_Fixed.INVALID) return "LightBowgun";
        throw new NotImplementedException("No valid weapon enum value found.");
    }
}