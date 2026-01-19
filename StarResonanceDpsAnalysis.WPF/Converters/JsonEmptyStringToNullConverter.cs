using System;
using Newtonsoft.Json;

namespace StarResonanceDpsAnalysis.WPF.Converters;

/// <summary>
/// JSON converter that converts empty strings to null during deserialization.
/// This prevents binding errors when WPF tries to convert empty strings to ImageSource.
/// </summary>
public class JsonEmptyStringToNullConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(string);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        // Handle null tokens
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        // Read string value
        var value = reader.Value?.ToString();
        
        // Return null for empty/whitespace strings
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
        }
        else
        {
            writer.WriteValue(value.ToString());
        }
    }
}

