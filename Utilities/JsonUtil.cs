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
        IncludeFields = true
    };

    /// <summary>
    /// Custom JSON converter for handling tuples containing long and short name strings.
    /// </summary>
    public class LongShortNamesConverter : JsonConverter<(string Long, string Short)>
    {
        /// <summary>
        /// Reads and converts the JSON to a tuple of (string Long, string Short).
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
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

        /// <summary>
        /// Writes a tuple of (string Long, string Short) as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
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
        /// <summary>
        /// Reads and converts the JSON to a PrefabGUID.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted PrefabGUID value.</returns>
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

        /// <summary>
        /// Writes a PrefabGUID as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, PrefabGUID value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.GuidHash);
        }
    }
}