using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Controls.Models;

#if MHR
using RE_Editor.Common.Data;
#elif MHWS
using RE_Editor.Common.Data;
#elif RE4
using RE_Editor.Common.Data;
#endif

namespace RE_Editor.Common.Models.List_Wrappers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class DataSourceWrapper<T> : ListWrapper<T> where T : struct {
    private readonly StructJson.Field field;

    public int Index { get; }

    private T Value_raw;
    public override T Value {
        get => Value_raw;
        set {
            if (EqualityComparer<T>.Default.Equals(Value_raw, value)) return;
            Value_raw = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Value_button));
        }
    }

    [CustomSorter(typeof(ButtonSorter))]
    [DisplayName("Value")]
    public string Value_button {
        get {
            var dataLookupSource = GetDataLookupSource();
            return dataLookupSource switch {
                Dictionary<Global.LangIndex, Dictionary<int, string>> source => GetLookupText(source),
                Dictionary<Global.LangIndex, Dictionary<uint, string>> source => GetLookupText(source),
                // ReSharper disable once NotResolvedInText
                _ => throw new ArgumentOutOfRangeException("dataLookupSource", $"Don't know how to lookup from: {dataLookupSource.GetType()}")
            };
        }
    }

    public DataSourceWrapper(int index, T value, StructJson.Field field) {
        Index      = index;
        Value      = value;
        this.field = field;
    }

    public object GetDataLookupSource() {
        return field.originalType?.Replace("[]", "") switch {
#if MHR
            "snow.data.ContentsIdSystem.ItemId" => DataHelper.ITEM_NAME_LOOKUP,
            "snow.data.DataDef.PlEquipSkillId" => DataHelper.SKILL_NAME_LOOKUP,
            "snow.data.DataDef.PlHyakuryuSkillId" => DataHelper.RAMPAGE_SKILL_NAME_LOOKUP,
            "snow.data.DataDef.PlKitchenSkillId" => DataHelper.DANGO_NAME_LOOKUP,
            "snow.data.DataDef.PlWeaponActionId" => DataHelper.SWITCH_SKILL_NAME_LOOKUP,
#elif MHWS
            "app.ArmorDef.SERIES_Fixed" => DataHelper.ARMOR_SERIES_BY_ENUM_VALUE,
            "app.HunterDef.Skill_Fixed" => DataHelper.SKILL_NAME_BY_ENUM_VALUE,
            "app.ItemDef.ID_Fixed" => DataHelper.ITEM_NAME_LOOKUP,
#elif RE4
            "chainsaw.ItemID" => DataHelper.ITEM_NAME_LOOKUP[Global.variant],
#endif
            _ => throw new InvalidOperationException($"No data source lookup known for: {field.originalType}")
        };
    }

    private string GetLookupText<LookupT>(Dictionary<Global.LangIndex, Dictionary<LookupT, string>> source) where LookupT : struct {
        return source[Global.locale].TryGet((LookupT) Convert.ChangeType(Value, typeof(LookupT))).ToStringWithId(Value, ShowAsHex());
    }

    private bool ShowAsHex() {
        return field.originalType?.Replace("[]", "") switch {
#if MHR
            "snow.data.ContentsIdSystem.ItemId" => true,
#elif RE4
            "chainsaw.ItemID" => true,
#endif
            _ => false
        };
    }
}