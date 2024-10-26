using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using GTFO.API.JSON.Converters;
using ProgressionGear.Dependencies;

namespace ProgressionGear.JSON
{
    public static class PWJson
    {
        private static readonly JsonSerializerOptions _readSettings = new()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            IgnoreReadOnlyProperties = true,
        };

        private static readonly JsonSerializerOptions _writeSettings = new(_readSettings);

        static PWJson()
        {
            _readSettings.Converters.Add(new JsonStringEnumConverter());
            _readSettings.Converters.Add(new LocalizedTextConverter());
            _readSettings.Converters.Add(new ProgressionRequirementConverter());
            if (PartialDataWrapper.HasPartialData)
                _readSettings.Converters.Add(PartialDataWrapper.PersistentIDConverter!);

            foreach (var converter in _readSettings.Converters)
                _writeSettings.Converters.Add(converter);
            _writeSettings.Converters.Add(new ProgressionLockDataConverter());
        }

        public static T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _readSettings);
        }

        public static object? Deserialize(Type type, string json)
        {
            return JsonSerializer.Deserialize(json, type, _readSettings);
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, _writeSettings);
        }
    }
}
