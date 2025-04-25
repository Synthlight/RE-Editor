using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class NoRequirements : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name = "No Crafting Requirements";

        var baseMod = new NexusMod {
            Version      = "1.14.1",
            NameAsBundle = name,
            Desc         = "Removes the item requirements when crafting."
        };

        var mods = new List<INexusMod> {
            baseMod
                .SetName("Weapons (Forge/Upgrade/Layer)")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Product) // Forge
                                    .Append(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Process)) // Upgrade
                                    .Append(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Change))
                                    .Append(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.OverwearProduct))) // Layer
                .SetAction(CheatMod.NoCost),
            baseMod
                .SetName("Weapons (Forge/Upgrade/Layer, Ignore Unlock Flags)")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Product) // Forge
                                    .Append(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Process)) // Upgrade
                                    .Append(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Change))
                                    .Append(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.OverwearProduct))) // Layer
                .SetAction(list => {
                    CheatMod.NoCost(list);
                    CheatMod.NoUnlockFlag(list);
                }),
            baseMod
                .SetName("Weapons (Layered Only)")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Change)
                                    .Append(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.OverwearProduct)))
                .SetAction(CheatMod.NoCost),
            baseMod
                .SetName("Weapons (Layered Only, Ignore Unlock Flags)")
                .SetFiles(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.Change)
                                    .Append(PathHelper.GetAllWeaponFilePaths(PathHelper.WeaponDataType.OverwearProduct)))
                .SetAction(list => {
                    CheatMod.NoCost(list);
                    CheatMod.NoUnlockFlag(list);
                }),
            baseMod
                .SetName("Rampage Skills")
                .SetFiles([PathHelper.RAMPAGE_SKILL_RECIPE_PATH])
                .SetAction(CheatMod.NoCost),
            baseMod
                .SetName("Decorations")
                .SetFiles([PathHelper.DECORATION_RECIPE_PATH, PathHelper.RAMPAGE_DECORATION_RECIPE_PATH])
                .SetAction(CheatMod.NoCost),
            baseMod
                .SetName("Decorations (Ignore Unlock Flags)")
                .SetFiles([PathHelper.DECORATION_RECIPE_PATH, PathHelper.RAMPAGE_DECORATION_RECIPE_PATH])
                .SetAction(list => {
                    CheatMod.NoCost(list);
                    CheatMod.NoUnlockFlag(list);
                }),
            baseMod
                .SetName("Armor (Normal)")
                .SetFiles([PathHelper.ARMOR_RECIPE_PATH])
                .SetAction(CheatMod.NoCost),
            baseMod
                .SetName("Armor (Normal, Ignore Unlock Flags)")
                .SetFiles([PathHelper.ARMOR_RECIPE_PATH])
                .SetAction(list => {
                    CheatMod.NoCost(list);
                    CheatMod.NoUnlockFlag(list);
                }),
            baseMod
                .SetName("Armor (Layered)")
                .SetFiles([PathHelper.LAYERED_ARMOR_RECIPE_PATH])
                .SetAction(CheatMod.NoCost),
            baseMod
                .SetName("Armor (Layered, Ignore Unlock Flags)")
                .SetFiles([PathHelper.LAYERED_ARMOR_RECIPE_PATH])
                .SetAction(list => {
                    CheatMod.NoCost(list);
                    CheatMod.NoUnlockFlag(list);
                }),
            baseMod
                .SetName("Cat/Dog (Armor/Weapons)")
                .SetFiles([
                    PathHelper.CAT_ARMOR_RECIPE_PATH,
                    PathHelper.CAT_WEAPON_RECIPE_PATH,
                    PathHelper.DOG_ARMOR_RECIPE_PATH,
                    PathHelper.DOG_WEAPON_RECIPE_PATH
                ])
                .SetAction(CheatMod.NoCost),
            baseMod
                .SetName("Cat/Dog (Layered Armor)")
                .SetFiles([PathHelper.CAT_DOG_LAYERED_ARMOR_RECIPE_PATH])
                .SetAction(CheatMod.NoCost),
            baseMod
                .SetName("Augment/Qurio Craft")
                .SetFiles([
                    PathHelper.AUGMENT_ARMOR_ENABLE_BASE_PATH,
                    PathHelper.AUGMENT_WEAPON_ENABLE_BASE_PATH,
                    PathHelper.AUGMENT_ARMOR_MATERIAL_BASE_PATH,
                    PathHelper.AUGMENT_WEAPON_MATERIAL_BASE_PATH
                ])
                .SetAction(CheatMod.NoCost)
        };

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var mod in new List<INexusMod>(mods)) {
            var newMod = (NexusMod) mod;
            newMod.NameAsBundle += " (GamePass)";
            newMod.Files = from file in newMod.Files
                           select file.Replace(@"\STM\", @"\MSG\");
            newMod.ForGp = true;
            mods.Add(newMod);
        }

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }
}