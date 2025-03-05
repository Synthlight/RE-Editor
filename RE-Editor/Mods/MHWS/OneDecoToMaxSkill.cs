using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using RE_Editor.Common;
using RE_Editor.Common.Models;
using RE_Editor.Models;
using RE_Editor.Models.Structs;
using RE_Editor.Util;

namespace RE_Editor.Mods;

[UsedImplicitly]
public class OneDecoToMaxSkill : IMod {
    [UsedImplicitly]
    public static void Make() {
        const string name        = "One Deco to Max Skill";
        const string description = "One Deco to Max Skill.";
        const string version     = "1.1";

        var baseMod = new NexusMod {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Pic.png"
        };

        var baseLuaMod = new VariousDataTweak {
            Version      = version,
            NameAsBundle = name,
            Desc         = description,
            Image        = $@"{PathHelper.MODS_PATH}\{name}\Pic.png"
        };

        var mods = new List<INexusMod> {
            baseMod
                .SetName("One Deco to Max Skill (PAK)")
                .SetFiles([PathHelper.DECORATION_DATA_PATH])
                .SetAction(MaxSkills),
            baseLuaMod
                .SetName("One Deco to Max Skill (REF)")
                .SetDefaultLuaName()
                .SetChanges([
                    new() {
                        Target = VariousDataTweak.Target.DECORATION_DATA,
                        Action = MaxSkillsRef
                    }
                ])
                .SetSkipPak(true)
        };

        ModMaker.WriteMods(mods, name, copyLooseToFluffy: true);
    }

    public static void MaxSkills(List<RszObject> rszObjectData) {
        foreach (var obj in rszObjectData) {
            switch (obj) {
                case App_user_data_AccessoryData_cData deco:
                    foreach (var skillLevel in deco.SkillLevel) {
                        if (skillLevel.Value > 0) {
                            skillLevel.Value = 10;
                        }
                    }
                    break;
            }
        }
    }

    public static void MaxSkillsRef(StreamWriter writer) {
        writer.WriteLine("    for skillIndex = 0, entry._SkillLevel:get_size() - 1 do");
        writer.WriteLine("        if (entry._SkillLevel[skillIndex].m_value >= 1) then");
        writer.WriteLine("            entry._SkillLevel[skillIndex] = 10");
        writer.WriteLine("        end");
        writer.WriteLine("    end");
    }
}