using ProgressionGear.ProgressionLock;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgressionGear.JSON
{
    public sealed class ProgressionLockDataConverter : JsonConverter<ProgressionLockData>
    {
        public override ProgressionLockData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new JsonException("ProgressionLockData doesn't have a custom read!");
        }

        public override void Write(Utf8JsonWriter writer, ProgressionLockData? value, JsonSerializerOptions options)
        {
            if (value == null) return;

            writer.WriteStartObject();
            writer.WritePropertyName(nameof(value.Unlock));
            JsonSerializer.Serialize(writer, value.Unlock, options);
            writer.WritePropertyName(nameof(value.Lock));
            JsonSerializer.Serialize(writer, value.Lock, options);
            writer.WritePropertyName(nameof(value.OfflineIDs));
            JsonSerializer.Serialize(writer, value.OfflineIDs, options);
            writer.WriteNumber(nameof(value.Priority), value.Priority);
            writer.WriteString(nameof(value.Name), value.Name);
            writer.WriteEndObject();
        }
    }
}
