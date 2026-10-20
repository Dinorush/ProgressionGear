using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgressionGear.JSON.Converters
{
    public sealed class OptionalArrayConverter<T> : JsonConverter<T[]>
    {
        public override T[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                List<T> target = new();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        return target.ToArray();

                    target.Add(JsonSerializer.Deserialize<T>(ref reader, options)!);
                }
                throw new JsonException("Expected EndArray when reading list");
            }

            return new T[] { JsonSerializer.Deserialize<T>(ref reader, options)! };
        }

        public override void Write(Utf8JsonWriter writer, T[] value, JsonSerializerOptions options)
        {
            if (value.Length == 1)
            {
                JsonSerializer.Serialize(writer, value[0], options);
                return;
            }

            writer.WriteStartArray();
            foreach (var item in value)
                JsonSerializer.Serialize(writer, item, options);
            writer.WriteEndArray();
        }
    }
}
