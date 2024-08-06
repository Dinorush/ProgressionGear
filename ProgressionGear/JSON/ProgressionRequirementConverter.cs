using ProgressionGear.ProgressionLock;
using ProgressionGear.Utils;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProgressionGear.JSON
{
    public sealed class ProgressionRequirementConverter : JsonConverter<ProgressionRequirement>
    {
        public override ProgressionRequirement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            ProgressionRequirement req = new();

            if (ParseRequirement(ref reader, req, options))
                return req;

            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Expected progression requirement to be a tier, level layout ID, or object");

            // Full object case
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return req;

                if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Expected PropertyName token");

                string property = reader.GetString()!;
                reader.Read();
                switch (property.ToLowerInvariant().Replace(" ", ""))
                {
                    case "level":
                    case "levellayout":
                    case "levelid":
                    case "levellayoutid":
                    case "tier":
                    case "requirement":
                        ParseRequirement(ref reader, req, options);
                        break;
                    case "main":
                    case "high":
                        req.Main = reader.GetBoolean();
                        break;
                    case "secondary":
                    case "extreme":
                        req.Secondary = reader.GetBoolean();
                        break;
                    case "overload":
                        req.Overload = reader.GetBoolean();
                        break;
                    case "all":
                    case "pe":
                    case "prisonerefficiency":
                        req.All = reader.GetBoolean();
                        break;
                    case "allnobooster":
                    case "penobooster":
                    case "prisonerefficiencynobooster":
                        req.AllNoBooster = reader.GetBoolean();
                        break;
                }
            }

            throw new JsonException("Expected EndObject token");
        }

        private static bool ParseRequirement(ref Utf8JsonReader reader, ProgressionRequirement req, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return true;

            if (reader.TokenType == JsonTokenType.String)
            {
                req.Tier = reader.GetString()!.ToEnum(eRundownTier.Surface);
                if (req.Tier == eRundownTier.Surface) // Partial data ID case
                    req.LevelLayoutID = JsonSerializer.Deserialize<uint>(ref reader, options);
                return true;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                req.LevelLayoutID = reader.GetUInt32();
                return true;
            }
            return false;
        }

        public override void Write(Utf8JsonWriter writer, ProgressionRequirement? value, JsonSerializerOptions options)
        {
            if (value == null) return;

            if (value.LevelLayoutID == 0 && value.Tier == eRundownTier.Surface)
            {
                writer.WriteNullValue();
                return;
            }

            if (value.Main && !value.Secondary && !value.Overload && !value.All)
            {
                if (value.LevelLayoutID != 0)
                    writer.WriteNumberValue(value.LevelLayoutID);
                else
                    writer.WriteStringValue(value.Tier.ToString());
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("Level");
            if (value.LevelLayoutID != 0)
                writer.WriteNumberValue(value.LevelLayoutID);
            else
                writer.WriteStringValue(value.Tier.ToString());
            writer.WriteBoolean(nameof(value.Main), value.Main);
            writer.WriteBoolean(nameof(value.Secondary), value.Secondary);
            writer.WriteBoolean(nameof(value.Overload), value.Overload);
            writer.WriteBoolean(nameof(value.All), value.All);
            writer.WriteBoolean(nameof(value.AllNoBooster), value.AllNoBooster);
            writer.WriteEndObject();
        }
    }
}
