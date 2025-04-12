using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Controls.Models;
using RE_Editor.Common.Util;

#if MHR
using RE_Editor.Common.Data;
#elif MHWS
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
                Dictionary<int, string> source => GetLookupText(source),
                Dictionary<uint, string> source => GetLookupText(source),
                Dictionary<Guid, string> source => GetLookupText(source),
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

    /**
     * Leave as a separate function, it gets called via reflection by AutoDataGrid for DataSourceWrapper types.
     */
    public object GetDataLookupSource() {
        return Utils.GetDataSourceLookup(field.originalType);
    }

    private string GetLookupText<LookupT>(Dictionary<LookupT, string> source) where LookupT : struct {
        return source.TryGet((LookupT) Convert.ChangeType(Value, typeof(LookupT))).ToStringWithId(Value, ShowAsHex());
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