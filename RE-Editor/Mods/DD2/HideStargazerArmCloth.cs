using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
public class HideStargazerArmCloth : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Hide Stargazer's Garb Arm Cloth";
        const string description = "Hides the arm cloth on the Stargazer's Garb.";
        const string version     = "1.1";

        var action = new Action<StreamWriter>(writer => {
            writer.WriteLine($"    if entry._{nameof(App_TopsSwapItem.TopsStyle)} == {(uint) (dynamic) StyleConstants.STARGAZERS_GARB} then");
            writer.WriteLine($"        entry._{nameof(App_TopsSwapItem.AmPartsEnable)} = {(long) App_TopsAmPartFlags.NONE}");
            writer.WriteLine("    end");
        });

        var mod = new SwapDbTweak {
            Name            = name,
            Version         = version,
            Desc            = description,
            Image           = $@"{PathHelper.MODS_PATH}\{name}\Title.png",
            Files           = [],
            AdditionalFiles = [],
            LuaName         = "HideStargazersGarbArmCloth.lua",
            SkipPak         = true,
            Changes = [
                new() {
                    Database = "TopsDB",
                    Gender   = App_Gender.Female,
                    Action   = action
                },
                new() {
                    Database = "TopsDB",
                    Gender   = App_Gender.Male,
                    Action   = action
                }
            ]
        };

        ModMaker.WriteMods(mainWindow, [mod], name, copyLooseToFluffy: true, noPakZip: true);
    }
}