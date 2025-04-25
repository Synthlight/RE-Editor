using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Data;
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
public class ItemTweaks : IMod {
    [UsedImplicitly]
    public static void Make(MainWindow mainWindow) {
        const string name        = "Item Tweaks";
        const string description = "Item/Equipment weight and cost changes.";
        const string version     = "1.10";

        var itemDataFiles = new List<string> {
            PathHelper.ARMOR_DATA_PATH,
            PathHelper.ITEM_DATA_PATH,
            PathHelper.WEAPON_DATA_PATH,
        };

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = $"{name} (PAK)",
            Desc         = description,
            Files        = itemDataFiles,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
        };

        var _1GoldArmorsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ARMOR,
            Action = writer => { writer.WriteLine($"        entry._{nameof(App_ItemDataParam.BuyPrice)} = 1"); }
        };
        var _1GoldItemsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ITEM,
            Action = writer => {
                writer.WriteLine($"        if entry._{nameof(App_ItemDataParam.Id)} ~= {ItemConstants._1_G} then");
                writer.WriteLine($"            entry._{nameof(App_ItemDataParam.BuyPrice)} = 1");
                writer.WriteLine("        end");
            }
        };
        var _1GoldWeaponsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.WEAPON,
            Action = writer => { writer.WriteLine($"        entry._{nameof(App_ItemDataParam.BuyPrice)} = 1"); }
        };
        var _1GoldFerrystoneOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ITEM,
            Action = writer => {
                writer.WriteLine($"        if entry._{nameof(App_ItemDataParam.Id)} == {ItemConstants.FERRYSTONE} then");
                writer.WriteLine($"            entry._{nameof(App_ItemDataParam.BuyPrice)} = 1");
                writer.WriteLine("        end");
            }
        };
        var weight0ArmorsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ARMOR,
            Action = writer => { writer.WriteLine($"        entry._{nameof(App_ItemDataParam.Weight)} = 0"); }
        };
        var weight0ItemsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ITEM,
            Action = writer => { WeightRef(writer, WeightOptions._0); }
        };
        var weight0MaterialsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ITEM,
            Action = writer => { WeightRef(writer, WeightOptions._0_Materials); }
        };
        var weight0WeaponsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.WEAPON,
            Action = writer => { writer.WriteLine($"        entry._{nameof(App_ItemDataParam.Weight)} = 0"); }
        };
        var weight0FerrystoneOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ITEM,
            Action = writer => { WeightRef(writer, WeightOptions._0_Ferry_Only); }
        };
        var sellPriceX10ArmorsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ARMOR,
            Action = writer => { writer.WriteLine($"        entry._{nameof(App_ItemDataParam.SellPrice)} = entry._{nameof(App_ItemDataParam.SellPrice)} * 10"); }
        };
        var sellPriceX10WeaponsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.WEAPON,
            Action = writer => { writer.WriteLine($"        entry._{nameof(App_ItemDataParam.SellPrice)} = entry._{nameof(App_ItemDataParam.SellPrice)} * 10"); }
        };
        var sellPriceX10ItemsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ITEM,
            Action = writer => {
                writer.WriteLine($"        if entry._{nameof(App_ItemDataParam.Id)} ~= {ItemConstants._1_G} then");
                writer.WriteLine($"            entry._{nameof(App_ItemDataParam.SellPrice)} = entry._{nameof(App_ItemDataParam.SellPrice)} * 10");
                writer.WriteLine("        end");
            }
        };
        var sellPriceX5ArmorsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ARMOR,
            Action = writer => { writer.WriteLine($"        entry._{nameof(App_ItemDataParam.SellPrice)} = entry._{nameof(App_ItemDataParam.SellPrice)} * 5"); }
        };
        var sellPriceX5ItemsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ITEM,
            Action = writer => {
                writer.WriteLine($"        if entry._{nameof(App_ItemDataParam.Id)} ~= {ItemConstants._1_G} then");
                writer.WriteLine($"            entry._{nameof(App_ItemDataParam.SellPrice)} = entry._{nameof(App_ItemDataParam.SellPrice)} * 5");
                writer.WriteLine("        end");
            }
        };
        var sellPriceX5WeaponsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.WEAPON,
            Action = writer => { writer.WriteLine($"        entry._{nameof(App_ItemDataParam.SellPrice)} = entry._{nameof(App_ItemDataParam.SellPrice)} * 5"); }
        };
        var sellPriceX2ArmorsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ARMOR,
            Action = writer => { writer.WriteLine($"        entry._{nameof(App_ItemDataParam.SellPrice)} = entry._{nameof(App_ItemDataParam.SellPrice)} * 2"); }
        };
        var sellPriceX2ItemsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ITEM,
            Action = writer => {
                writer.WriteLine($"        if entry._{nameof(App_ItemDataParam.Id)} ~= {ItemConstants._1_G} then");
                writer.WriteLine($"            entry._{nameof(App_ItemDataParam.SellPrice)} = entry._{nameof(App_ItemDataParam.SellPrice)} * 2");
                writer.WriteLine("        end");
            }
        };
        var sellPriceX2WeaponsOnly = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.WEAPON,
            Action = writer => { writer.WriteLine($"        entry._{nameof(App_ItemDataParam.SellPrice)} = entry._{nameof(App_ItemDataParam.SellPrice)} * 2"); }
        };
        var noDecay = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ITEM,
            Action = DecayRef
        };
        var stack999 = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ITEM,
            Action = writer => { StackRef(writer, StackOptions._999); }
        };
        var stack9999 = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ITEM,
            Action = writer => { StackRef(writer, StackOptions._9999); }
        };
        var stackShortMax = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ITEM,
            Action = writer => { StackRef(writer, StackOptions.SHORT_MAX); }
        };
        var favor1000 = new ItemDbTweak.Change {
            Target = ItemDbTweak.Target.ITEM,
            Action = writer => {
                writer.WriteLine($"        if entry._{nameof(App_ItemDataParam.Id)} ~= {ItemConstants._1_G} then");
                writer.WriteLine($"            entry._{nameof(App_ItemDataParam.Favor)} = 1000");
                writer.WriteLine("        end");
            }
        };

        var mods = new List<INexusMod> {
            baseMod
                .SetName($"{name} (PAK): Cost: 1 Gold")
                .SetAction(list => GoldCost(list, GoldOptions._1)),
            baseMod
                .SetName($"{name} (PAK): Cost: 1 Gold, x10 Sell Price")
                .SetAction(list => {
                    GoldCost(list, GoldOptions._1);
                    SellPrice(list, SellOptions.X10);
                }),
            baseMod
                .SetName($"{name} (PAK): Cost: 1 Gold, Weight: 0")
                .SetAction(list => {
                    GoldCost(list, GoldOptions._1);
                    Weight(list, WeightOptions._0);
                }),
            baseMod
                .SetName($"{name} (PAK): Cost: 1 Gold, Weight: 0 (Ferrystone Only)")
                .SetAction(list => {
                    GoldCost(list, GoldOptions._1_Ferry_Only);
                    Weight(list, WeightOptions._0_Ferry_Only);
                }),
            baseMod
                .SetName($"{name} (PAK): Weight: 0")
                .SetAction(list => Weight(list, WeightOptions._0)),
            baseMod
                .SetName($"{name} (PAK): Weight: 0, x10 Sell Price")
                .SetAction(list => {
                    Weight(list, WeightOptions._0);
                    SellPrice(list, SellOptions.X10);
                }),
            baseMod
                .SetName($"{name} (PAK): Weight: 0 (All Items Only)")
                .SetFiles([PathHelper.ITEM_DATA_PATH])
                .SetAction(list => Weight(list, WeightOptions._0)),
            baseMod
                .SetName($"{name} (PAK): Weight: 0 (Materials Only)")
                .SetAction(list => Weight(list, WeightOptions._0_Materials)),
            baseMod
                .SetName($"{name} (PAK): x10 Sell Price")
                .SetAction(list => SellPrice(list, SellOptions.X10)),
            baseMod
                .SetName($"{name} (PAK): All-in-One")
                .SetAction(list => {
                    GoldCost(list, GoldOptions._1);
                    SellPrice(list, SellOptions.X10);
                    Weight(list, WeightOptions._0);
                }),
            new ItemDbTweak {
                Name            = $"{name} (REF): All-in-One",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaksAllInOne.lua",
                SkipPak         = true,
                Changes = [
                    _1GoldArmorsOnly,
                    _1GoldItemsOnly,
                    _1GoldWeaponsOnly,
                    weight0ArmorsOnly,
                    weight0ItemsOnly,
                    weight0WeaponsOnly,
                    sellPriceX10ArmorsOnly,
                    sellPriceX10ItemsOnly,
                    sellPriceX10WeaponsOnly,
                    noDecay,
                    stackShortMax,
                    favor1000,
                ],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Cost: 1 Gold (Armors Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks1GoldArmors.lua",
                SkipPak         = true,
                Changes         = [_1GoldArmorsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Cost: 1 Gold (Items Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks1GoldItems.lua",
                SkipPak         = true,
                Changes         = [_1GoldItemsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Cost: 1 Gold (Weapons Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks1GoldWeapons.lua",
                SkipPak         = true,
                Changes         = [_1GoldWeaponsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Cost: 1 Gold (Ferrystone Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks1GoldFerrystone.lua",
                SkipPak         = true,
                Changes         = [_1GoldFerrystoneOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Weight: 0 (Armors Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0WeightArmors.lua",
                SkipPak         = true,
                Changes         = [weight0ArmorsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Weight: 0 (Items Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0WeightItems.lua",
                SkipPak         = true,
                Changes         = [weight0ItemsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Weight: 0 (Weapons Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0WeightWeapons.lua",
                SkipPak         = true,
                Changes         = [weight0WeaponsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Weight: 0 (Materials Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0WeightMaterials.lua",
                SkipPak         = true,
                Changes         = [weight0MaterialsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Weight: 0 (Ferrystone Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0WeightFerrystone.lua",
                SkipPak         = true,
                Changes         = [weight0FerrystoneOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Sell Price: x10 (Armors Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0SellPriceX10Armors.lua",
                SkipPak         = true,
                Changes         = [sellPriceX10ArmorsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Sell Price: x10 (Items Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0SellPriceX10Items.lua",
                SkipPak         = true,
                Changes         = [sellPriceX10ItemsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Sell Price: x10 (Weapons Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0SellPriceX10Weapons.lua",
                SkipPak         = true,
                Changes         = [sellPriceX10WeaponsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Sell Price: x5 (Armors Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0SellPriceX5Armors.lua",
                SkipPak         = true,
                Changes         = [sellPriceX5ArmorsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Sell Price: x5 (Items Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0SellPriceX5Items.lua",
                SkipPak         = true,
                Changes         = [sellPriceX5ItemsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Sell Price: x5 (Weapons Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0SellPriceX5Weapons.lua",
                SkipPak         = true,
                Changes         = [sellPriceX5WeaponsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Sell Price: x2 (Armors Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0SellPriceX2Armors.lua",
                SkipPak         = true,
                Changes         = [sellPriceX2ArmorsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Sell Price: x2 (Items Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0SellPriceX2Items.lua",
                SkipPak         = true,
                Changes         = [sellPriceX2ItemsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Sell Price: x2 (Weapons Only)",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaks0SellPriceX2Weapons.lua",
                SkipPak         = true,
                Changes         = [sellPriceX2WeaponsOnly],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): No Bad Decay",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaksNoBadDecay.lua",
                SkipPak         = true,
                Changes         = [noDecay],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Stack Size: 999",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaksStackSize999.lua",
                SkipPak         = true,
                Changes         = [stack999],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Stack Size: 9999",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaksStackSize9999.lua",
                SkipPak         = true,
                Changes         = [stack9999],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Stack Size: {short.MaxValue}",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = $"ItemTweaksStackSize{short.MaxValue}.lua",
                SkipPak         = true,
                Changes         = [stackShortMax],
            },
            new ItemDbTweak {
                Name            = $"{name} (REF): Gift Favor: 1000",
                NameAsBundle    = $"{name} (REF)",
                Version         = version,
                Desc            = description,
                Image           = $@"{PathHelper.MODS_PATH}\{name}\Merchant.png",
                Files           = [],
                AdditionalFiles = [],
                LuaName         = "ItemTweaksGiftFavor1000.lua",
                SkipPak         = true,
                Changes         = [favor1000],
            },
        };

        ModMaker.WriteMods(mainWindow, mods, name, copyLooseToFluffy: true);
    }

    public static void GoldCost(List<RszObject> rszObjectData, GoldOptions option) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_ItemDataParam itemData:
                    if (itemData.Id == ItemConstants._1_G) continue;

                    switch (option) {
                        case GoldOptions._1:
                            itemData.BuyPrice = 1;
                            break;
                        case GoldOptions._1_Ferry_Only:
                            if (itemData.Id == ItemConstants.FERRYSTONE) {
                                itemData.BuyPrice = 1;
                            }
                            break;
                    }
                    break;
                case App_ItemWeaponParam itemData:
                    switch (option) {
                        case GoldOptions._1:
                            itemData.BuyPrice = 1;
                            break;
                    }
                    break;
                case App_ItemArmorParam itemData:
                    switch (option) {
                        case GoldOptions._1:
                            itemData.BuyPrice = 1;
                            break;
                    }
                    break;
            }
        }
    }

    public static void SellPrice(List<RszObject> rszObjectData, SellOptions option) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_ItemDataParam itemData:
                    if (itemData.Id == ItemConstants._1_G) continue;

                    switch (option) {
                        case SellOptions.X10:
                            itemData.SellPrice *= 10;
                            break;
                    }
                    break;
                case App_ItemWeaponParam itemData:
                    switch (option) {
                        case SellOptions.X10:
                            itemData.SellPrice *= 10;
                            break;
                    }
                    break;
                case App_ItemArmorParam itemData:
                    switch (option) {
                        case SellOptions.X10:
                            itemData.SellPrice *= 10;
                            break;
                    }
                    break;
            }
        }
    }

    public static void Weight(List<RszObject> rszObjectData, WeightOptions option) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_ItemDataParam itemData:
                    if (itemData.Id == ItemConstants._1_G) continue;

                    switch (option) {
                        case WeightOptions._0:
                            itemData.Weight = 0;
                            break;
                        case WeightOptions._0_Ferry_Only:
                            if (itemData.Id == ItemConstants.FERRYSTONE) {
                                itemData.Weight = 0;
                            }
                            break;
                        case WeightOptions._0_Materials:
                            if (itemData.Category == App_ItemCategory.Material
                                || itemData.SubCategory == App_ItemSubCategory.Material) {
                                itemData.Weight = 0;
                            }
                            break;
                    }
                    break;
                case App_ItemWeaponParam itemData:
                    switch (option) {
                        case WeightOptions._0:
                            itemData.Weight = 0;
                            break;
                    }
                    break;
                case App_ItemArmorParam itemData:
                    switch (option) {
                        case WeightOptions._0:
                            itemData.Weight = 0;
                            break;
                    }
                    break;
            }
        }
    }

    public static void WeightRef(StreamWriter writer, WeightOptions option) {
        switch (option) {
            case WeightOptions._0:
                writer.WriteLine($"        if entry._{nameof(App_ItemDataParam.Id)} ~= {ItemConstants._1_G} then");
                writer.WriteLine($"            entry._{nameof(App_ItemDataParam.Weight)} = 0");
                writer.WriteLine("        end");
                return;
            case WeightOptions._0_Ferry_Only:
                writer.WriteLine($"        if entry._{nameof(App_ItemDataParam.Id)} == {ItemConstants.FERRYSTONE} then");
                writer.WriteLine($"            entry._{nameof(App_ItemDataParam.Weight)} = 0");
                writer.WriteLine("        end");
                return;
            case WeightOptions._0_Materials:
                writer.WriteLine($"        if entry._{nameof(App_ItemDataParam.Category)} == {(uint) App_ItemCategory.Material}");
                writer.WriteLine($"            or entry._{nameof(App_ItemDataParam.SubCategory)} == {(uint) App_ItemSubCategory.Material} then");
                writer.WriteLine($"            entry._{nameof(App_ItemDataParam.Weight)} = 0");
                writer.WriteLine("        end");
                return;
        }
    }

    public static void DecayRef(StreamWriter writer) {
        var itemData = ReDataFile.Read($@"{PathHelper.CHUNK_PATH}\{PathHelper.ITEM_DATA_PATH}")
                                 .rsz.GetEntryObject<App_ItemData>();

        foreach (var item in itemData.Params) {
            if (item.Id == ItemConstants._1_G) continue;

            var decayItemName = DataHelper.ITEM_NAME_LOOKUP[Global.LangIndex.eng].TryGet((uint) item.DecayedItemId, "");
            if (decayItemName.StartsWith("Rotten") || decayItemName.StartsWith("Dried")) {
                writer.WriteLine($"        if entry._{nameof(App_ItemDataParam.Id)} == {item.Id} then");
                writer.WriteLine($"            entry._{nameof(App_ItemDataParam.Decay)} = 0");
                writer.WriteLine($"            entry._{nameof(App_ItemDataParam.DecayedItemId)} = 0");
                writer.WriteLine("        end");
            }
        }
    }

    public static void StackRef(StreamWriter writer, StackOptions option) {
        writer.WriteLine($"        if entry._{nameof(App_ItemDataParam.Id)} ~= {ItemConstants._1_G}");
        writer.WriteLine($"            and entry._{nameof(App_ItemDataParam.StackNum)} > 1 then");
        var newSize = option switch {
            StackOptions._999 => 999,
            StackOptions._9999 => 9999,
            StackOptions.SHORT_MAX => short.MaxValue,
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
        };
        writer.WriteLine($"            entry._{nameof(App_ItemDataParam.StackNum)} = {newSize}");
        writer.WriteLine("        end");
    }

    public enum GoldOptions {
        _1,
        _1_Ferry_Only,
    }

    public enum WeightOptions {
        _0,
        _0_Ferry_Only,
        _0_Materials,
    }

    public enum SellOptions {
        X10,
    }

    public enum StackOptions {
        _999,
        _9999,
        SHORT_MAX,
    }
}