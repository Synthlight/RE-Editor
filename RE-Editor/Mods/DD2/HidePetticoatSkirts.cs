using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Constants;
using RE_Editor.Models;
using RE_Editor.Models.Enums;
using RE_Editor.Models.Structs;
using RE_Editor.Util;
using RE_Editor.Windows;

namespace RE_Editor.Mods;

[UsedImplicitly]
[SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
[SuppressMessage("ReSharper", "ConvertSwitchStatementToSwitchExpression")]
[SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
public class HidePetticoatSkirts : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string nameAsBundle        = "Hide Petticoat Parts";
        const string version             = "1.3";
        const string elegantPetticoat    = "Elegant Petticoat";
        const string gauntletedPetticoat = "Gauntleted Petticoat";

        var baseMod = new SwapDbTweak {
            Version         = version,
            NameAsBundle    = nameAsBundle,
            Image           = $@"{PathHelper.MODS_PATH}\{nameAsBundle}\Title.png",
            Files           = [],
            AdditionalFiles = [],
            SkipPak         = true,
        };

        var mods = new[] {
            baseMod
                .SetName($"Hide {elegantPetticoat} (Armbands)")
                .SetDesc($"Hides the armbands on the {elegantPetticoat}.")
                .SetLuaName($"Hide{elegantPetticoat.Replace(" ", "")}Armbands.lua")
                .SetAdditionalFiles([])
                .SetChanges([
                    new() {
                        Database = "TopsDB",
                        Gender   = App_Gender.Female,
                        Action = writer => {
                            writer.WriteLine($"    if entry._{nameof(App_TopsSwapItem.TopsStyle)} == {(uint) (dynamic) StyleConstants.ELEGANT_PETTICOAT} then");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} ~ {(long) App_TopsAmPartFlags.LeftArmUpper}");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} ~ {(long) App_TopsAmPartFlags.RightArmUpper}");
                            writer.WriteLine("    end");
                        }
                    }
                ]),
            baseMod
                .SetName($"Hide {gauntletedPetticoat} (Elbow Plate, Left)")
                .SetDesc($"Hides the elbow plates on the {gauntletedPetticoat}.")
                .SetLuaName($"Hide{gauntletedPetticoat.Replace(" ", "")}ElbowPlateLeft.lua")
                .SetAdditionalFiles([])
                .SetChanges([
                    new() {
                        Database = "TopsDB",
                        Gender   = App_Gender.Female,
                        Action = writer => {
                            writer.WriteLine($"    if entry._{nameof(App_TopsSwapItem.TopsStyle)} == {(uint) (dynamic) StyleConstants.GAUNTLETED_PETTICOAT} then");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmPartsEnable)} ~ {(long) App_TopsAmPartFlags.LeftElbow}");
                            writer.WriteLine("    end");
                        }
                    }
                ]),
            baseMod
                .SetName($"Hide {gauntletedPetticoat} (Elbow Plate, Right)")
                .SetDesc($"Hides the elbow plates on the {gauntletedPetticoat}.")
                .SetLuaName($"Hide{gauntletedPetticoat.Replace(" ", "")}ElbowPlateRight.lua")
                .SetAdditionalFiles([])
                .SetChanges([
                    new() {
                        Database = "TopsDB",
                        Gender   = App_Gender.Female,
                        Action = writer => {
                            writer.WriteLine($"    if entry._{nameof(App_TopsSwapItem.TopsStyle)} == {(uint) (dynamic) StyleConstants.GAUNTLETED_PETTICOAT} then");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmPartsEnable)} ~ {(long) App_TopsAmPartFlags.RightElbow}");
                            writer.WriteLine("    end");
                        }
                    }
                ]),
            baseMod
                .SetName($"Hide {elegantPetticoat} (Gauntlets) (Requires Invis Fixer)")
                .SetDesc($"Hides the gauntlets on the {elegantPetticoat}.")
                .SetLuaName($"Hide{elegantPetticoat.Replace(" ", "")}Gauntlets.lua")
                .SetAdditionalFiles([])
                .SetChanges([
                    new() {
                        Database = "TopsDB",
                        Gender   = App_Gender.Female,
                        Action = writer => {
                            writer.WriteLine($"    if entry._{nameof(App_TopsSwapItem.TopsStyle)} == {(uint) (dynamic) StyleConstants.ELEGANT_PETTICOAT} then");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmPartsEnable)} = {(ulong) App_TopsAmPartFlags.NONE}");
                            writer.WriteLine("    end");
                        }
                    }
                ]),
            baseMod
                .SetName($"Hide {gauntletedPetticoat} (Gauntlets, All) (Requires Invis Fixer)")
                .SetDesc($"Hides the gauntlets on the {gauntletedPetticoat}.")
                .SetLuaName($"Hide{gauntletedPetticoat.Replace(" ", "")}AllGauntlets.lua")
                .SetAdditionalFiles([])
                .SetChanges([
                    new() {
                        Database = "TopsDB",
                        Gender   = App_Gender.Female,
                        Action = writer => {
                            writer.WriteLine($"    if entry._{nameof(App_TopsSwapItem.TopsStyle)} == {(uint) (dynamic) StyleConstants.GAUNTLETED_PETTICOAT} then");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} = {(ulong) App_TopsAmPartFlags.NONE}");
                            writer.WriteLine("    end");
                        }
                    }
                ]),
            baseMod
                .SetName($"Hide {gauntletedPetticoat} (Gauntlet, Left) (Requires Invis Fixer)")
                .SetDesc($"Hides the gauntlets on the {gauntletedPetticoat}.")
                .SetLuaName($"Hide{gauntletedPetticoat.Replace(" ", "")}LeftGauntlets.lua")
                .SetAdditionalFiles([])
                .SetChanges([
                    new() {
                        Database = "TopsDB",
                        Gender   = App_Gender.Female,
                        Action = writer => {
                            writer.WriteLine($"    if entry._{nameof(App_TopsSwapItem.TopsStyle)} == {(uint) (dynamic) StyleConstants.GAUNTLETED_PETTICOAT} then");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} ~ {(long) App_TopsAmPartFlags.LeftArmUpper}");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} ~ {(long) App_TopsAmPartFlags.LeftArmLower}");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} ~ {(long) App_TopsAmPartFlags.LeftHand}");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} ~ {(long) App_TopsAmPartFlags.LeftPauldron}");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} ~ {(long) App_TopsAmPartFlags.LeftAccessory2}");
                            writer.WriteLine("    end");
                        }
                    }
                ]),
            baseMod
                .SetName($"Hide {gauntletedPetticoat} (Gauntlet, Right)")
                .SetDesc($"Hides the gauntlets on the {gauntletedPetticoat}.")
                .SetLuaName($"Hide{gauntletedPetticoat.Replace(" ", "")}RightGauntlets.lua")
                .SetAdditionalFiles([])
                .SetChanges([
                    new() {
                        Database = "TopsDB",
                        Gender   = App_Gender.Female,
                        Action = writer => {
                            writer.WriteLine($"    if entry._{nameof(App_TopsSwapItem.TopsStyle)} == {(uint) (dynamic) StyleConstants.GAUNTLETED_PETTICOAT} then");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} ~ {(long) App_TopsAmPartFlags.RightHand}");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} ~ {(long) App_TopsAmPartFlags.RightPauldron}");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} ~ {(long) App_TopsAmPartFlags.RightFastener}");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} ~ {(long) App_TopsAmPartFlags.RightAccessory1}");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.AmSubPartsEnable)} ~ {(long) App_TopsAmPartFlags.RightAccessory2}");
                            writer.WriteLine("    end");
                        }
                    }
                ]),
            baseMod
                .SetName($"Hide {elegantPetticoat} (Shoulder Plates)")
                .SetDesc($"Hides the shoulder plates on the {elegantPetticoat}.")
                .SetLuaName($"Hide{elegantPetticoat.Replace(" ", "")}ShoulderPlates.lua")
                .SetAdditionalFiles([])
                .SetChanges([
                    new() {
                        Database = "TopsDB",
                        Gender   = App_Gender.Female,
                        Action = writer => {
                            writer.WriteLine($"    if entry._{nameof(App_TopsSwapItem.TopsStyle)} == {(uint) (dynamic) StyleConstants.ELEGANT_PETTICOAT} then");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.BdPartsEnable)} = {(ulong) App_TopsBdPartFlags.NONE}");
                            writer.WriteLine("    end");
                        }
                    }
                ]),
            baseMod
                .SetName($"Hide {elegantPetticoat} (Skirt)")
                .SetDesc($"Hides the skirt on the {elegantPetticoat}.")
                .SetLuaName($"Hide{elegantPetticoat.Replace(" ", "")}Skirt.lua")
                .SetAdditionalFiles([])
                .SetChanges([
                    new() {
                        Database = "TopsDB",
                        Gender   = App_Gender.Female,
                        Action = writer => {
                            writer.WriteLine($"    if entry._{nameof(App_TopsSwapItem.TopsStyle)} == {(uint) (dynamic) StyleConstants.ELEGANT_PETTICOAT} then");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.WbPartsEnable)} = {(ulong) App_TopsWbPartFlags.Torso}");
                            writer.WriteLine("    end");
                        }
                    }
                ]),
            baseMod
                .SetName($"Hide {gauntletedPetticoat} (Skirt)")
                .SetDesc($"Hides the skirt on the {gauntletedPetticoat}.")
                .SetLuaName($"Hide{gauntletedPetticoat.Replace(" ", "")}Skirt.lua")
                .SetAdditionalFiles([])
                .SetChanges([
                    new() {
                        Database = "TopsDB",
                        Gender   = App_Gender.Female,
                        Action = writer => {
                            writer.WriteLine($"    if entry._{nameof(App_TopsSwapItem.TopsStyle)} == {(uint) (dynamic) StyleConstants.GAUNTLETED_PETTICOAT} then");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.WbPartsEnable)} = {(ulong) App_TopsWbPartFlags.Torso}");
                            writer.WriteLine("    end");
                        }
                    }
                ]),
            baseMod
                .SetName($"Hide {elegantPetticoat} (Waist)")
                .SetDesc($"Hides the waist on the {elegantPetticoat}.")
                .SetLuaName($"Hide{elegantPetticoat.Replace(" ", "")}Waist.lua")
                .SetAdditionalFiles([])
                .SetChanges([
                    new() {
                        Database = "TopsDB",
                        Gender   = App_Gender.Female,
                        Action = writer => {
                            writer.WriteLine($"    if entry._{nameof(App_TopsSwapItem.TopsStyle)} == {(uint) (dynamic) StyleConstants.ELEGANT_PETTICOAT} then");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.BtPartsEnable)} = entry._{nameof(App_TopsSwapItem.BtPartsEnable)} ~ {(long) App_TopsBtPartFlags.Waist}");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.BtSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.BtSubPartsEnable)} ~ {(long) App_TopsBtPartFlags.Waist}");
                            writer.WriteLine("    end");
                        }
                    }
                ]),
            baseMod
                .SetName($"Hide {gauntletedPetticoat} (Waist)")
                .SetDesc($"Hides the waist on the {gauntletedPetticoat}.")
                .SetLuaName($"Hide{gauntletedPetticoat.Replace(" ", "")}Waist.lua")
                .SetAdditionalFiles([])
                .SetChanges([
                    new() {
                        Database = "TopsDB",
                        Gender   = App_Gender.Female,
                        Action = writer => {
                            writer.WriteLine($"    if entry._{nameof(App_TopsSwapItem.TopsStyle)} == {(uint) (dynamic) StyleConstants.GAUNTLETED_PETTICOAT} then");
                            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.BdSubPartsEnable)} = entry._{nameof(App_TopsSwapItem.BdSubPartsEnable)} ~ {(long) App_TopsBdPartFlags.WaistAccessory1}");
                            writer.WriteLine("    end");
                        }
                    }
                ]),
        };

        foreach (var mod in mods) {
            var change = mod.Changes[0];
            change.Gender = App_Gender.Male;
            mod.Changes.Add(change);
        }

        ModMaker.WriteMods(mainWindow, mods, nameAsBundle, copyLooseToFluffy: true, noPakZip: true);
    }
}