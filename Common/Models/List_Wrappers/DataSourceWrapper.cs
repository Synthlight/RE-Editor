using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using RE_Editor.Common.Attributes;
using RE_Editor.Common.Controls.Models;
using RE_Editor.Common.Util;

namespace RE_Editor.Common.Models.List_Wrappers;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class DataSourceWrapper<T> : ListWrapper<T> {
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
                Dictionary<string, string> source => GetLookupText(source),
                // ReSharper disable once NotResolvedInText
                _ => throw new ArgumentOutOfRangeException("dataLookupSource", $"Don't know how to lookup from: {dataLookupSource.GetType()}")
            };
        }
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public DataSourceWrapper(int index, T value, StructJson.Field field) {
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        Index      = index;
        Value      = value;
        this.field = field;
    }

    /**
     * Leave as a separate function, it gets called via reflection by AutoDataGrid for DataSourceWrapper types.
     */
    public object GetDataLookupSource() {
        return Utils.GetDataSourceLookup(field);
    }

    // Can't constrain T to struct as it can be a string in a list, but should never be null in such a case.
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
    private string GetLookupText<LookupT>(Dictionary<LookupT, string> source) {
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        return source.TryGet((LookupT) Convert.ChangeType(Value, typeof(LookupT))).ToStringWithId(Value, ShowAsHex());
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
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