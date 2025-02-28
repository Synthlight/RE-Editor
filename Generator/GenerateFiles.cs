using System.CodeDom;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using Newtonsoft.Json;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Generator.Models;

namespace RE_Editor.Generator;

public partial class GenerateFiles {
    public const  string BASE_GEN_PATH    = @"..\..\..\Generated"; // @"C:\Temp\Gen"
    public const  string BASE_PROJ_PATH   = @"..\..\..";
    public const  string STRUCT_JSON_PATH = $@"{BASE_PROJ_PATH}\Dump-Parser\Output\{PathHelper.CONFIG_NAME}\rsz{PathHelper.CONFIG_NAME}.json";
    public const  string ENUM_GEN_PATH    = $@"{BASE_GEN_PATH}\Enums\{PathHelper.CONFIG_NAME}";
    public const  string STRUCT_GEN_PATH  = $@"{BASE_GEN_PATH}\Structs\{PathHelper.CONFIG_NAME}";
    private const string ASSETS_DIR       = $@"{BASE_PROJ_PATH}\RE-Editor\Data\{PathHelper.CONFIG_NAME}\Assets";
    public const  string ENUM_REGEX       = $@"namespace ((?:{ROOT_STRUCT_NAMESPACE}::[^ ]+|{ROOT_STRUCT_NAMESPACE}|via::[^ ]+|via|goatree::[^ ]+|goatree)) {{\s+(?:\/\/ (\[Flags\])\s+)?enum ([^ ]+) ({{[^}}]+}})"; //language=regexp

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression")]
    public static readonly List<string> UNSUPPORTED_DATA_TYPES = [ // TODO: Implement support for these.
        "AABB",
        "Area",
        "Capsule",
        "Cylinder",
        "Ellipsoid",
        "Frustum",
        "KeyFrame",
        "LineSegment",
        "Rect",
        "Rect3D",
        "Sphere",
        "Struct",
        "Torus",
        "Triangle",
    ];

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression")]
    public static readonly List<string> UNSUPPORTED_OBJECT_TYPES = [ // TODO: Implement support for these.
        "System.Collections.Generic.Dictionary`",
        "System.Collections.Generic.Queue`1<System.Tuple`", // Nested generics.
        "System.Collections.Generic.Queue`1<via.vec3>", // Because this breaks generation and I need a better way of handling generics.
        "System.ValueTuple`",
        "via.gui.Panel", // Too long, skip it for now.
#if DD2
        "app.ClassSelector`1[",
        "app.GUICharaEditData.PatternParam`",
        "app.lod.LODProcessDefine.ClassSelector`1[app.lod.opecon.LODProcessOperatingCondition][][]",
#elif DRDR
        "app.solid.CorrespondGroup`",
#elif MHR
        "snow.data.StmKeyconfigSystem.ConfigCodeSet`",
        "snow.enemy.aifsm.Ems043ActionSetRoot`",
        "snow.shell.EnemyShellManagerBase`",
        "System.Collections.Generic.List`1<snow.enemy.em134.Em", // Nested generics.
#elif MHWS
        "ace.user_data.DialogueManagerSettingBase`",
        "ace.user_data.EventResourceListBase`",
        "app.user_data.CharacterEditGenderedThumbnails`",
        "app.user_data.CharacterEditThumbnails`",
        "app.user_data.OptionDisplayData.cSetting`",
        "app.user_data.OptionGraphicsData.cItem`",
        "app.user_data.OptionGraphicsData.cSetting`",
#elif RE2
        "app.ropeway.camera.CameraCurveUserData.CurveParamTable`",
        "app.ropeway.CorrespondGroup`",
#elif RE3
        "offline.camera.CameraCurveUserData.CurveParamTable`",
        "offline.CorrespondGroup`",
#elif RE4
        "app.",
        "chainsaw.CameraCurveUserDataParam.CurveParamTable`", // Winds up with `ObservableCollection<System_String>` instead of `ObservableCollection<string>`.
#elif RE8
        "app.TPSCameraConditionSetting`",
#endif
    ];

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression")]
    public static readonly List<string> ALLOWED_GENERIC_TYPES = [
#if DD2
        ".Element<",
        "app.anime.EquipAdjustUserData.SizeData`",
        "app.AppEventCatalogBase`",
        "app.BodyScaleFormulaBase`",
        "app.BodyWeightDataBase`",
        "app.Ch221Parameter.StateChangeParameter.ElementDataBase`",
        "app.Ch221Parameter.StateChangeParameter.ElementDataColor`",
        "app.Ch221Parameter.StateChangeParameter.ElementDataFloat`",
        "app.CharacterVariationDataContainer`",
        "app.charaedit.ch000.useredit.LimitSpeciesAndGenderData`",
        "app.charaedit.EditInfluenceMapBase`",
        "app.ClassSelector`1<",
        "app.CutSceneIKModule.EquipmentOffset`",
        "app.FilterSettingMediator`",
        "app.JobUniqueParameter.AreaParameterList`",
        "app.JobUniqueParameter.CustomSkillLevelParameter`",
        "app.LocalWindSettings`",
        "app.lod.LODProcessDefine.ClassSelector`1<",
        "app.MaterialInterpolation.Variable`",
        "app.ModuleParametersUserData`",
        "app.retarget.RetargetLodJointSettingBase`",
        "app.SimpleFlightPathTracer`",
        "app.StringUtil.NameHash`",
        "soundlib.SoundSwitchApp`",
#elif DRDR
        "app.solid.camera.CameraCurveUserData.CurveParamTable`",
        "app.solid.DampingSetting`",
        "app.solid.weapon.generator.BombGeneratorUserDataBase`",
        "app.solid.weapon.shell.ShellPrefabSetting`",
#elif MHR
        // "snow.Bitset`", Doesn't actually have any fields as a class, just read it as an array.
        "snow.BitSetFlag`",
        "snow.camera.CameraUtility.BufferingParam`",
        "snow.enemy.EnemyCarryChangeTrack`",
        "snow.enemy.EnemyEditStepActionData`",
        "snow.envCreature.EnvironmentCreatureActionController`",
        "snow.eventcut.EventPlayerMediator.FaceMaterialConfig`",
        "snow.player.BowMaterialElement`",
        "snow.player.ChargeAxeMaterialElement`",
        "snow.player.DualBladesMaterialElement`",
        "snow.player.GreatSwordMaterialElement`",
        "snow.player.GunLanceMaterialElement`",
        "snow.player.HammerMaterialElement`",
        "snow.player.HeavyBowgunMaterialElement`",
        "snow.player.HornMaterialElement`",
        "snow.player.InsectGlaiveMaterialElement`",
        "snow.player.LanceMaterialElement`",
        "snow.player.LightBowgunMaterialElement`",
        "snow.player.LongSwordMaterialElement`",
        "snow.player.ShortSwordMaterialElement`",
        "snow.player.SlashAxeMaterialElement`",
        "snow.StmDefaultKeyconfigData.EnumSet2`",
        "snow.StmGuiKeyconfigData.EnumItemSystemMessage`",
        "snow.StmGuiKeyconfigData.EnumMessage`",
#elif MHWS
        "ace.Bitset`",
        "ace.btable.cEditFieldDropBox`",
        "ace.btable.cEditFieldEnum`",
        "ace.cInstanceGuidArray`",
        "ace.cUserDataArgumentHolder`",
        "ace.user_data.GraphicsSettingUsedVramSizeBase.cData`",
        "ace.voxel.cVoxelBuffer`",
        "ace.WorkRateManagerBase`",
        "app.AppActionUtil.cActionParamEditableArray`",
        "app.cAnimalLotteryTable`",
        "app.cEnumerableParam`",
        "app.cFieldGridTable`",
        "app.cNpcDerivedClassHolder`",
        "app.cParamsByEnv`",
        "app.DynamicClassSelector2`",
        "app.InstanceGuidArray`",
        "app.snd_user_data.EnumStateArray`",
        "app.snd_user_data.SoundCatalogData`",
        "app.snd_user_data.SoundGUIDefinition`",
        "app.snd_user_data.SoundMusicGameFlowSettings.GameFlowMusicAction`",
        "app.user_data.PorterRopesData`",
        "soundlib.SoundStateApp`",
        "soundlib.SoundSwitchApp`",
#elif RE2
        "app.ropeway.enemy.userdata.MotionUserDataBase.MotionInfo`",
        "app.ropeway.enemy.userdata.MotionUserDataBase.MotionTable`",
        "app.ropeway.BitFlag`",
        "app.ropeway.DampingSetting`",
        "app.ropeway.ParseBitFlag`",
        "app.ropeway.Percentable`",
        "app.ropeway.PercentValueTable`",
        "app.ropeway.weapon.generator.BombGeneratorUserDataBase`",
        "app.ropeway.weapon.shell.ShellPrefabSetting`",
        "app.ropeway.weapon.shell.RadiateShellUserDataBase`",
#elif RE3
        "offline.DampingSetting`",
        "offline.enemy.userdata.MotionUserDataBase.MotionInfo`",
        "offline.enemy.userdata.MotionUserDataBase.MotionTable`",
        "offline.ParseBitFlag`",
        "offline.Percentable`",
        "offline.PercentValueTable`",
        "offline.weapon.generator.BombGeneratorUserDataBase`",
        "offline.weapon.shell.RadiateShellUserDataBase`",
        "offline.weapon.shell.ShellPrefabSetting`",
#elif RE4
        "chainsaw.AIMapNodeScore.Param`",
        "chainsaw.AppEventCatalogBase`",
        "chainsaw.Ch1b7z0ParamUserData.NumData`",
        "chainsaw.Ch1e0z0ParamUserData.NumData`",
        "chainsaw.Ch1f1z0ParamUserData.NumData`",
        "chainsaw.Ch1f7z0ParamUserData.NumData`",
        "chainsaw.Ch1f8z0ParamUserData.NumData`",
        "chainsaw.Ch1fcz0ParamUserData.NumData`",
        "chainsaw.Ch1fdz0ParamUserData.NumData`",
        "chainsaw.Ch4fez0ParamUserData.NumData`",
        "chainsaw.CustomConditionBase`",
        "chainsaw.NetworkRankingSettingUserdata.BoardNameTable`",
        "chainsaw.SwitchFeatureParameter`",
        "soundlib.SoundStateApp`",
        "soundlib.SoundSwitchApp`",
#elif RE8
        "app.EnemyRankControlParameter.RankParam`",
        "app.ParamSetting`",
        "app.Spawn.ResumeOptionParameter`",
        "app.TPSCameraConditionContainer`",
#endif
    ];

    private static readonly List<uint> GREYLIST = []; // Hashes used in a given location.

    public readonly  Dictionary<string, EnumType>   enumTypes        = [];
    public readonly  Dictionary<string, StructType> structTypes      = [];
    private readonly Dictionary<string, StructJson> structJsonByName = [];
    private readonly Dictionary<string, StructJson> structJson       = JsonConvert.DeserializeObject<Dictionary<string, StructJson>>(File.ReadAllText(STRUCT_JSON_PATH))!;
    public readonly  Dictionary<uint, uint>         gpCrcOverrides   = []; // Because the GP version uses the same hashes, but different CRCs.

    public void Go(string[] args) {
        var useWhitelist = args.Length > 0 && args.Contains("useWhitelist");
        var useGreylist  = args.Length > 0 && args.Contains("useGreylist");
        var dryRun       = args.Length > 0 && args.Contains("dryRun");

        Log("Finding enum placeholders in the struct json.");
        FindAllEnumUnderlyingTypes();

        if (!dryRun) {
            Log("Creating directories.");
            Directory.CreateDirectory(ENUM_GEN_PATH);
            Directory.CreateDirectory(STRUCT_GEN_PATH);
            Directory.CreateDirectory(ASSETS_DIR);

            Log("Removing existing generated files.");
            CleanupGeneratedFiles(ENUM_GEN_PATH);
            CleanupGeneratedFiles(STRUCT_GEN_PATH);
        }

        Log("Parsing enums.");
        ParseEnums();
        Log("Parsing structs.");
        ParseStructs();

        if (useWhitelist || useGreylist) {
            FilterWhitelisted();
        }
        if (useGreylist) {
            FindAllHashesBeingUsed();
            FilterGreylisted();
            FindCrcOverrides();
        }
        if (useWhitelist || useGreylist) {
            UpdateUsingCounts();
            RemoveUnusedTypes();
            UpdateButtons();
        }

        Log("Updating struct `Data` fields.");
        UpdateStructDataFields();

        Log($"Generating {enumTypes.Count} enums, {structTypes.Count} structs.");
        GenerateEnums(dryRun);
        Log("Enums written.");
        GenerateStructs(dryRun);
        Log("Structs written.");
        WriteStructInfo(dryRun);
        Log("Struct info written.");
    }

    private void WriteStructInfo(bool dryRun) {
        var structInfo = new Dictionary<uint, StructJson>();
        foreach (var (hashString, @struct) in structJson) {
            var hash = uint.Parse(hashString, NumberStyles.HexNumber);
            structInfo[hash] = @struct;
        }
        if (!dryRun) {
            Directory.CreateDirectory(ASSETS_DIR);
            File.WriteAllText($@"{ASSETS_DIR}\STRUCT_INFO.json", JsonConvert.SerializeObject(structInfo, Formatting.Indented));
            File.WriteAllText($@"{ASSETS_DIR}\GP_CRC_OVERRIDE_INFO.json", JsonConvert.SerializeObject(gpCrcOverrides, Formatting.Indented));
        }
    }

    /**
     * Parses the struct json looking for placeholder 'structs' which are most likely enums.
     */
    private void FindAllEnumUnderlyingTypes() {
        var compiler  = new CSharpCodeProvider();
        var enumTypes = new Dictionary<string, EnumType>();

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var (_, structInfo) in structJson) {
            if (structInfo.name == null
                || structInfo.name.Contains('<')
                || structInfo.name.Contains('`')
                || structInfo.name.StartsWith("System")
                || structInfo.fields?.Count == 0) continue;

            var field = structInfo.fields?[0];

            // We only want the enum placeholders.
            if (structInfo.fields?.Count != 1 || field?.name != "value__") continue;

            var name       = structInfo.name.ToConvertedTypeName()!;
            var boxedType  = GetTypeForName(field.originalType!);
            var type       = new CodeTypeReference(boxedType);
            var typeString = compiler.GetTypeOutput(type);

            enumTypes[name] = new(name, structInfo.name, typeString);
        }

        foreach (var key in enumTypes.Keys.OrderBy(s => s)) {
            this.enumTypes[key] = enumTypes[key];
        }
    }

    private static Type GetTypeForName(string typeName) {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            var type = assembly.GetType(typeName);
            if (type != null) return type;
        }
        throw new InvalidOperationException($"Unable to find type for: {typeName}");
    }

    private static void CleanupGeneratedFiles(string path) {
        var files = Directory.EnumerateFiles(path, "*.cs", SearchOption.TopDirectoryOnly);
        foreach (var file in files) {
            File.Delete(file);
        }
    }

    private void ParseEnums() {
        PrepTemplatesForFoundTypes();
        CleanupEnumsWithMissingTemplates();
    }

    /**
     * Goes through the enum header file and preps templates to generate the enum code file.
     */
    private void PrepTemplatesForFoundTypes() {
        var enumHpp = File.ReadAllText(PathHelper.ENUM_HEADER_PATH);
        var regex   = new Regex(ENUM_REGEX, RegexOptions.Singleline);
        var matches = regex.Matches(enumHpp);
#pragma warning disable IDE0220 // Add explicit cast
        foreach (Match match in matches) {
            var hppName = $"{match.Groups[1].Value}::{match.Groups[3].Value}";
            if (hppName.Contains('<') || hppName.Contains('`')) continue;
            var name     = hppName.ToConvertedTypeName();
            var contents = match.Groups[4].Value;
            var isFlags  = match.Groups[2].Success;
            if (name != null && enumTypes.TryGetValue(name, out var enumType)) {
                if (contents.Contains("= -")) {
                    enumType.type = enumType.type switch {
                        "ushort" => "short",
                        "uint" => "int",
                        "ulong" => "long",
                        "byte" => "sbyte",
                        _ => enumType.type
                    };
                }
                enumType.Contents = contents;
                enumType.isFlags  = isFlags;
            }
        }
#pragma warning restore IDE0220 // Add explicit cast
    }

    /**
     * Removes all the remaining enums that didn't get a generation template.
     */
    private void CleanupEnumsWithMissingTemplates() {
        enumTypes.Keys
                 .Where(key => enumTypes[key].Contents == null)
                 .ToList()
                 .ForEach(key => enumTypes.Remove(key));
    }

    private void ParseStructs() {
        var structTypes = new Dictionary<string, StructType>();

        foreach (var (_, structInfo) in structJson) {
            if (string.IsNullOrEmpty(structInfo.name)) continue;
            structJsonByName[structInfo.name] = structInfo;
        }

        foreach (var (hash, structInfo) in structJson) {
            if (structInfo.name?.StartsWith("System.Action`") == true) {
            }

            // ReSharper disable once GrammarMistakeInComment
            // The dump has empty array entries like `.Element[[`, but we transform the field type to `.Element<` which bypasses these empty structs.
            if (structInfo.name?.Contains("[[") == true) continue;

            if (!IsStructNameValid(structInfo.name)) continue;
            // Also ignore structs that are just enum placeholders.
            if (structInfo.fields is [{name: "value__"}]) continue;

            if (structInfo.name!.ToLower() == "via.prefab") {
                if (structInfo.fields!.Count >= 2) {
                    structInfo.fields![0].name = "Enabled";
                    structInfo.fields![1].name = "Name";
                } else {
                    Log("Warning: via.prefab type has no fields.");
                }
            }

            // Ignore the 'via.thing' placeholders.
            if (structInfo.name!.GetViaType() != null) continue;
            var name       = structInfo.name.ToConvertedTypeName()!;
            var parent     = GetParent(structInfo);
            var structType = new StructType(name, parent, hash, structInfo);
            structTypes[name] = structType;

            // Check for and fix duplicate fields.
            // e.g. MHR `snow.player.fsm.PlayerFsm2CommandBow_ReplaceCheck` has two identical `atkType` & `replaceType` fields.
            if (structInfo.fields == null || structInfo.fields!.Count == 0) continue;
            HashSet<StructJson.Field> fields    = new(structInfo.fields!.Count);
            Dictionary<string, int>   nameCount = [];
            foreach (var field in structInfo.fields) {
                if (!fields.Add(field)) {
                    Log($"Warning: Found a duplicate field `{field.name}` in `{structInfo.name}`. Changing name.");
                    if (!nameCount.ContainsKey(field.name!)) nameCount[field.name!] = 1;
                    field.name += $"{nameCount[field.name!]++}";
                    fields.Add(field);
                }
            }
            structInfo.fields = fields.ToList();
        }

        foreach (var key in structTypes.Keys.OrderBy(s => s)) {
            this.structTypes[key] = structTypes[key];
        }
    }

    private string? GetParent(StructJson structInfo) {
        while (true) {
            var parent = structInfo.parent;
            if (string.IsNullOrEmpty(parent)) return null;

            /*
             * This whole thing is to deal with generics that have intermediary (empty) types that we skip.
             * Example:
             * `app.ShellAdditionalSurfaceStickerParameter`
             *     `app.ShellAdditionalParameter`1<app.ShellAdditionalSurfaceSticker>` <- We want to skip this.
             *         `app.ShellAdditionalParameter` <---------------------------------- And get this.
             */


            if (structJsonByName.TryGetValue(parent, out var parentStructInfo)) {
                if (parent.Contains('`') && parentStructInfo.parent?.Contains('`') == false && parentStructInfo.fields?.Count == 0) {
                    structInfo = parentStructInfo;
                    continue;
                }
            }

            if (IsStructNameValid(parent)) {
                var isViaType = parent.ToLower().StartsWith("via");

                if (!isViaType || (isViaType && parentStructInfo?.fields?.Count > 0)) {
                    parent = parent.ToConvertedTypeName();
                    return parent;
                }
            }
            return null;
        }
    }

    private static bool IsStructNameValid(string? structName) {
        var isBadName = structName == null
                        || structName.Contains('<')
                        || structName.Contains('>')
                        || structName.Contains('`')
                        || structName.Contains('[')
                        || structName.Contains(']')
                        || structName.Contains("List`")
                        || structName.Contains("Culture=neutral")
                        || structName.StartsWith("System");
        if (structName != null) {
            var isAllowed = structName.StartsWith(ROOT_STRUCT_NAMESPACE)
                            || structName.StartsWith("ace")
                            || structName.StartsWith("AISituation")
                            || structName.StartsWith("goatree")
                            || structName.StartsWith("share")
                            || structName.StartsWith("solid")
                            || structName.StartsWith("soundlib")
                            || structName.StartsWith("via");
            isBadName = !(!isBadName && isAllowed);
            if (isBadName && (ALLOWED_GENERIC_TYPES.Any(structName.StartsWith)
                              || structName.StartsWith("snow.enemy.aifsm") && structName.Contains("`"))) { // MHR
                isBadName = false;
            }
        }
        return !isBadName;
    }

    /**
     * Increases the useCount of enums/structs marked as whitelisted.
     */
    private void FilterWhitelisted() {
        // Whitelist more commonly used things.
        foreach (var name in WHITELIST.ToList()) {
            WHITELIST.Add(name + "_Param");
            WHITELIST.Add(name + "_Data");
        }
        // Make sure to keep whitelisted enums/structs.
        enumTypes.Keys
                 .Where(IsWhitelisted)
                 .ToList()
                 .ForEach(key => enumTypes[key].useCount++);
        structTypes.Keys
                   .Where(IsWhitelisted)
                   .ToList()
                   .ForEach(key => structTypes[key].useCount++);
    }

    private static bool IsWhitelisted(string key) {
#if MHR
        return WHITELIST.Contains(key)
               || key.ContainsIgnoreCase("ContentsIdSystem")
               || key.ContainsIgnoreCase("Snow_data_DataDef")
               || key.ContainsIgnoreCase("ProductUserData")
               || key.ContainsIgnoreCase("ChangeUserData")
               || key.ContainsIgnoreCase("ProcessUserData")
               || key.ContainsIgnoreCase("RecipeUserData")
               || key.ContainsIgnoreCase("PlayerUserData")
               || key.ContainsIgnoreCase("Snow_equip")
               || key.ContainsIgnoreCase("Snow_data")
               || key.ContainsIgnoreCase("Snow_player");
#elif RE4
        return WHITELIST.Contains(key)
               || key.ContainsIgnoreCase("ItemDefinitionUserData")
               || key.ContainsIgnoreCase("ItemUseResult")
               || key.ContainsIgnoreCase("ItemID");
#else
        return WHITELIST.Contains(key);
#endif
    }

    /**
     * Goes through structs and increases useCounts of enums/structs referenced by other structs.
     */
    private void UpdateUsingCounts() {
        var history = new List<string>(structTypes.Count);
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var structType in structTypes.Values) {
            if (structType.name.GetViaType() != null) continue;
            if (structType.useCount > 0) {
                structType.UpdateUsingCounts(this, history);
            }
        }
    }

    /**
     * Goes through structs and sets button types. Ideally should be done on parents. Call after types are filtered for best performance.
     * Definitely call after updating using counts since this ignores structs not being used.
     */
    private void UpdateButtons() {
        var history = new List<string>(structTypes.Count);
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var structType in structTypes.Values) {
            if (structType.name.GetViaType() != null) continue;
            if (structType.useCount > 0) {
                structType.UpdateButtons(this, history);
            }
        }
    }

    /**
     * Goes through enums/structs and removes ones that aren't referenced by any struct.
     */
    private void RemoveUnusedTypes() {
        enumTypes.Keys
                 .Where(key => enumTypes[key].useCount == 0)
                 .ToList()
                 .ForEach(key => enumTypes.Remove(key));
        structTypes.Keys
                   .Where(key => structTypes[key].useCount == 0)
                   .ToList()
                   .ForEach(key => structTypes.Remove(key));
        // Remove the struct data we're not using from the struct info.
        RemoveStructs(from entry in structJson
                      where IsStructNameValid(entry.Value.name)
                      let name = entry.Value.name
                      where !string.IsNullOrEmpty(name)
                      where name.GetViaType() == null
                      let typeName = name?.ToConvertedTypeName()
                      where !enumTypes.ContainsKey(typeName) && !structTypes.ContainsKey(typeName)
                      select entry.Key);
        // Now again to remove invalid/ignored when parsing structs. (Like those which are empty named as some generic list.)
        RemoveStructs(from entry in structJson
                      where !IsStructNameValid(entry.Value.name)
                      select entry.Key);
    }

    private void RemoveStructs(IEnumerable<string> enumerable) {
        foreach (var key in enumerable.ToList()) {
            structJson.Remove(key);
        }
    }

    private static void FindAllHashesBeingUsed() {
        foreach (var path in PathHelper.TEST_PATHS) {
            Log($"Finding all files in: {PathHelper.CHUNK_PATH + path}");
        }

        var allUserFiles = PathHelper.GetCachedFileList(FileListCacheType.USER);
        var count        = allUserFiles.Count;
        Log($"Found {count} files.");

        var now = DateTime.Now;
        Log("");

        for (var i = 0; i < allUserFiles.Count; i++) {
            var newNow = DateTime.Now;
            if (newNow > now.AddSeconds(1)) {
                try {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                } catch (Exception) {
                    // Breaks tests so just ignore for those.
                }
                Log($"Parsed {i}/{count}.");
                now = newNow;
            }

            var file = allUserFiles[i];
            var rsz  = ReDataFile.Read(file, justReadHashes: true);
            var hashes = from instanceInfo in rsz.rsz.instanceInfo
                         select instanceInfo.hash;
            hashes = hashes.Distinct();
            foreach (var hash in hashes) {
                if (GREYLIST.Contains(hash)) continue;
                GREYLIST.Add(hash);
            }
        }

        try {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        } catch (Exception) {
            // Breaks tests so just ignore for those.
        }
        Log($"Parsed {count}/{count}.");
    }

    /**
     * Increases the useCount of enums/structs marked as whitelisted.
     */
    private void FilterGreylisted() {
        // Make a list of structs by hash.
        var structsByHash = new Dictionary<uint, StructType>();
        foreach (var (_, structType) in structTypes) {
            if (structType.name.GetViaType() != null) continue;
            structsByHash[uint.Parse(structType.hash, NumberStyles.HexNumber)] = structType;
        }

        // Include structs for all the target files.
        foreach (var hash in GREYLIST.Where(structsByHash.ContainsKey)) {
            structsByHash[hash].useCount++;
        }
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var structType in structTypes.Values) {
            if (structType.useCount == 0) continue;
            if (structType.name.GetViaType() != null) continue;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var field in structType.structInfo.fields!) {
                if (string.IsNullOrEmpty(field.name) || string.IsNullOrEmpty(field.originalType)) continue;

                if (UNSUPPORTED_DATA_TYPES.Contains(field.type!)) continue;
                if (UNSUPPORTED_OBJECT_TYPES.Any(s => field.originalType!.Contains(s))) continue;

                var typeName = field.originalType!.ToConvertedTypeName();
                if (typeName == null) continue;

                if (enumTypes.TryGetValue(typeName, out var enumType)) {
                    enumType.useCount++;
                }
            }
        }

#if MHR
        // Because this one doesn't appear in the fields, but we still use it.
        enumTypes["Snow_data_ContentsIdSystem_SubCategoryType"].useCount++;
#endif
    }

    private void FindCrcOverrides() {
        foreach (var path in PathHelper.TEST_PATHS) {
            Log($"Finding all GP files in: {(PathHelper.CHUNK_PATH + path).Replace("STM", "MSG")}");
        }

        var allGpUserFiles = PathHelper.GetCachedFileList(FileListCacheType.USER, msg: true);
        var count          = allGpUserFiles.Count;
        Log($"Found {count} files.");

        var now = DateTime.Now;
        Log("");

        for (var i = 0; i < allGpUserFiles.Count; i++) {
            var newNow = DateTime.Now;
            if (newNow > now.AddSeconds(1)) {
                try {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                } catch (Exception) {
                    // Breaks tests so just ignore for those.
                }
                Log($"Parsed {i}/{count}.");
                now = newNow;
            }

            var file = allGpUserFiles[i];
            var rsz  = ReDataFile.Read(file, justReadHashes: true);
            var instanceInfos = from instanceInfo in rsz.rsz.instanceInfo
                                select instanceInfo;
            instanceInfos = instanceInfos.Distinct();
            foreach (var (hash, crc) in instanceInfos) {
                gpCrcOverrides.TryAdd(hash, crc);
            }
        }

        try {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        } catch (Exception) {
            // Breaks tests so just ignore for those.
        }
        Log($"Parsed {count}/{count}.");
        Log($"Created {gpCrcOverrides.Count} CRC overrides.");
    }

    private void UpdateStructDataFields() {
        var history = new List<string>(structTypes.Count);
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var structType in structTypes.Values) {
            structType.UpdateFields(this, history);
        }
    }

    private void GenerateEnums(bool dryRun) {
        DoWithConsoleProgressCount(enumTypes.Values.ToList(), enumType => { new EnumTemplate(enumType).Generate(dryRun); }, (i, count) => $"Wrote {i}/{count} enums.");
    }

    private void GenerateStructs(bool dryRun) {
        DoWithConsoleProgressCount(structTypes.Values.ToList(), structType => {
            if (structType.name.GetViaType() == null) {
                new StructTemplate(this, structType).Generate(dryRun);
            }
        }, (i, count) => $"Wrote {i}/{count} structs.");
    }

    public static void DoWithConsoleProgressCount<T>(IList<T> thingsToDo, Action<T> doThing, Func<int, int, string> progressFormat) {
        var count = thingsToDo.Count;
        var now   = DateTime.Now;
        Log("");

        for (var i = 0; i < thingsToDo.Count; i++) {
            var newNow = DateTime.Now;
            if (newNow > now.AddSeconds(1)) {
                try {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                } catch (Exception) {
                    // Breaks tests so just ignore for those.
                }
                Log(progressFormat(i, count));
                now = newNow;
            }

            var structType = thingsToDo[i];
            doThing(structType);
        }

        try {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        } catch (Exception) {
            // Breaks tests so just ignore for those.
        }
        Log(progressFormat(count, count));
    }

    public static void Log(string msg) {
        Console.WriteLine(msg);
        Debug.WriteLine(msg);
    }
}