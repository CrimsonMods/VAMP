using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Stunlock.Core;

namespace VAMP.Utilities;

/// <summary>
/// Utility class for JSON serialization and deserialization operations.
/// </summary>
public static class JsonUtil
{
    /// <summary>
    /// Default JSON serializer options with pretty printing and field inclusion enabled.
    /// </summary>
    public static readonly JsonSerializerOptions PrettyJsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Custom JSON converter for handling tuples containing long and short name strings.
    /// </summary>
    public class LongShortNamesConverter : JsonConverter<(string Long, string Short)>
    {
        public override (string Long, string Short) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            string longName = string.Empty;
            string shortName = string.Empty;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return (longName, shortName);

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                string propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "Long":
                        longName = reader.GetString();
                        break;
                    case "Short":
                        shortName = reader.GetString();
                        break;
                    case "Item1":
                        longName = reader.GetString();
                        break;
                    case "Item2":
                        shortName = reader.GetString();
                        break;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, (string Long, string Short) value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Long", value.Long);
            writer.WriteString("Short", value.Short);
            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// Custom JSON converter for handling PrefabGUID objects.
    /// </summary>
    public class PrefabGUIDConverter : JsonConverter<PrefabGUID>
    {
        public override PrefabGUID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return new PrefabGUID(reader.GetInt32());
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                if (int.TryParse(reader.GetString(), out int value))
                {
                    return new PrefabGUID(value);
                }
            }

            throw new JsonException("Expected number or numeric string for PrefabGUID");
        }

        public override void Write(Utf8JsonWriter writer, PrefabGUID value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.GuidHash);
        }
    }

    #region Converters - Time
    /// <summary>
    /// Custom JSON converter for handling TimeOnly objects with hours, minutes, and seconds.
    /// </summary>
    public class TimeOnlyConverter : JsonConverter<TimeOnly>
    {
        private const string TimeFormat = "HH:mm:ss";

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return TimeOnly.Parse(value ?? "00:00:00");
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(TimeFormat));
        }
    }

    /// <summary>
    /// Custom JSON converter for handling TimeOnly objects with only hours and minutes.
    /// </summary>
    public class TimeOnlyHourMinuteConverter : JsonConverter<TimeOnly>
    {
        private const string TimeFormat = "HH:mm";

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return TimeOnly.ParseExact(value ?? "00:00", TimeFormat);
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(TimeFormat));
        }
    }

    /// <summary>
    /// Custom JSON converter for handling DayOfWeek values, displaying them as day names
    /// and representing null as "Daily".
    /// </summary>
    public class DayOfWeekConverter : JsonConverter<DayOfWeek?>
    {
        public override DayOfWeek? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            string dayString = reader.GetString();

            if (string.IsNullOrEmpty(dayString) || dayString.Equals("Daily", StringComparison.OrdinalIgnoreCase))
                return null;

            if (Enum.TryParse<DayOfWeek>(dayString, true, out var day))
                return day;

            throw new JsonException($"Unable to parse '{dayString}' as DayOfWeek");
        }

        public override void Write(Utf8JsonWriter writer, DayOfWeek? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString());
            }
            else
            {
                writer.WriteStringValue("Daily");
            }
        }
    }
}
#endregion