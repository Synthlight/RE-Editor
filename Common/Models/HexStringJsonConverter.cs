using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RE_Editor.Common.Models;

public sealed class HexStringJsonConverter : JsonConverter {
    public override bool CanConvert(Type objectType) {
        return typeof(uint) == objectType;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        writer.WriteValue(value?.ToString());
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
        if (reader.TokenType == JsonToken.Null) return null;
        var token = (JValue) JToken.Load(reader);
        return token.Value switch {
            uint value => value, // Shouldn't happen because we use it on string fields.
            string str => uint.Parse(str, NumberStyles.HexNumber),
            _ => throw new JsonSerializationException()
        };
    }
}