using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ResumeJobMatcher.Infrastructure.Serialization;

/// <summary>
/// JSON converter that handles deserializing date strings such as "present" or "current"
/// into <see cref="DateTime"/> values. Normal ISO 8601 date strings are also supported.
/// </summary>
public class PresentDateConverter : JsonConverter<DateTime>
{
    private static readonly string[] _presentKeywords = { "present", "current" };

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Expected a string token but got {reader.TokenType}.");

        string? value = reader.GetString();

        if (string.IsNullOrWhiteSpace(value))
            return default;

        // Handle sentinel keywords like "present" or "current"
        if (_presentKeywords.Contains(value.Trim().ToLowerInvariant()))
            return DateTime.Today;

        // Attempt to parse as a normal date string
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed;

        throw new JsonException($"Unable to parse date value: '{value}'.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}
