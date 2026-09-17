using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ERP_Government.Web.Infrastructure.Converters;

public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrEmpty(str))
            throw new JsonException("DateOnly value cannot be null or empty.");

        if (DateOnly.TryParseExact(str, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
            return dateOnly;

        if (DateOnly.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateOnly))
            return dateOnly;

        throw new JsonException($"Unable to deserialize '{str}' to DateOnly. Expected format: {Format}.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
