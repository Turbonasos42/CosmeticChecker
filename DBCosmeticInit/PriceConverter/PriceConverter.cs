using System;
using System.Text.Json;
using System.Text.Json.Serialization;



public class PriceConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            // Если это число, возвращаем его как строку
            return reader.GetDecimal().ToString();
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            // Если это уже строка, просто возвращаем ее
            return reader.GetString();
        }

        throw new JsonException("Невозможно конвертировать значение в строку.");
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        // Просто записываем строку
        writer.WriteStringValue(value);
    }
}