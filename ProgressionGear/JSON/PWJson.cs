using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using GTFO.API.JSON.Converters;
using ProgressionGear.Dependencies;
using System.Diagnostics.CodeAnalysis;

namespace ProgressionGear.JSON
{
    public static class PWJson
    {
        private static readonly JsonSerializerOptions _settings = new()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            IgnoreReadOnlyProperties = true,
        };

        static PWJson()
        {
            _settings.Converters.Add(new JsonStringEnumConverter());
            _settings.Converters.Add(new LocalizedTextConverter());
            _settings.Converters.Add(new ProgressionRequirementConverter());
            _settings.Converters.Add(new ProgressionLockDataConverter());
            if (PartialDataWrapper.HasPartialData)
                _settings.Converters.Add(PartialDataWrapper.PersistentIDConverter!);
        }

        public static T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _settings);
        }

        public static object? Deserialize(Type type, string json)
        {
            return JsonSerializer.Deserialize(json, type, _settings);
        }

        public static T? Deserialize<T>(ref Utf8JsonReader reader)
        {
            return JsonSerializer.Deserialize<T>(ref reader, _settings);
        }

        public static bool TryDeserialize<T>(ref Utf8JsonReader reader, [MaybeNullWhen(false)] out T value)
        {
            value = Deserialize<T>(ref reader);
            return value != null;
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, _settings);
        }

        public static void Serialize<T>(Utf8JsonWriter writer, T value)
        {
            JsonSerializer.Serialize(writer, value, _settings);
        }
    }
}
