using System.Collections.Generic;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Constants;
using RE_Editor.Generated.Models;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class OneHitKo : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "OHKO";
        const string description = "Changes Defender I weapons to have 50k base damage.";
        const string version     = "1.1.1";
        var          image       = $@"{PathHelper.MODS_PATH}\{name.ToSafeName()}\Pic.png";

        var mods = new List<NexusMod> {
            new() {
                Name    = name,
                Version = version,
                Desc    = description,
                Image   = image,
                Files   = PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Base),
                Action  = ChangeDamage
            },
            new() {
                Name    = name + " (GamePass)",
                Version = version,
                Desc    = description,
                Image   = image,
                Files   = PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Base, "MSG"),
                Action  = ChangeDamage,
                ForGp   = true
            }
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void ChangeDamage(IList<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case IWeapon weapon:
                    switch ((uint) weapon.Id) {
                        case BowConstants.DEFENDER_BOW_I:
                        case ChargeAxeConstants.DEFENDER_CHARGE_BLADE_I:
                        case DualBladesConstants.DEFENDER_DUAL_BLADES_I:
                        case GreatSwordConstants.DEFENDER_GREAT_SWORD_I:
                        case GunLanceConstants.DEFENDER_GUNLANCE_I:
                        case HammerConstants.DEFENDER_HAMMER_I:
                        case HeavyBowgunConstants.DEFENDER_HEAVY_BOWGUN_I:
                        case HornConstants.DEFENDER_HORN_I:
                        case InsectGlaiveConstants.DEFENDER_GLAIVE_I:
                        case LightBowgunConstants.DEFENDER_LIGHT_BOWGUN_I:
                        case LongSwordConstants.DEFENDER_LONG_SWORD_I:
                        case ShortSwordConstants.DEFENDER_SWORD_I:
                        case SlashAxeConstants.DEFENDER_SWITCH_AXE_I:
                        case LanceConstants.DEFENDER_LANCE_I:
                            weapon.Atk          = 50000;
                            weapon.CriticalRate = 200;
                            if (weapon is IMeleeWeapon meleeWeapon) {
                                meleeWeapon.MainElementType           = Snow_equip_PlWeaponElementTypes.Bomb;
                                meleeWeapon.MainElementVal            = 50000;
                                meleeWeapon.SharpnessValList[0].Value = 10;
                                meleeWeapon.SharpnessValList[1].Value = 10;
                                meleeWeapon.SharpnessValList[2].Value = 10;
                                meleeWeapon.SharpnessValList[3].Value = 10;
                                meleeWeapon.SharpnessValList[4].Value = 10;
                                meleeWeapon.SharpnessValList[5].Value = 10;
                                meleeWeapon.SharpnessValList[6].Value = 340;
                                foreach (var handicraft in meleeWeapon.HandicraftValList) {
                                    handicraft.Value = 0;
                                }
                            }
                            break;
                    }
                    break;
            }
        }
    }
}