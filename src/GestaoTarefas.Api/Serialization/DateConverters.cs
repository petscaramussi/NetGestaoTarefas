using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GestaoTarefas.Api.Serialization;

public static class DateFormats
{
    public const string Date = "dd-MM-yyyy";
    public const string DateTime = "dd-MM-yyyy HH:mm:ss";

    public static bool TryParseDate(string? value, out DateOnly date) =>
        DateOnly.TryParseExact(value, Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}

/// <summary>
/// Serializa <see cref="DateOnly"/> no formato dd-MM-yyyy.
/// </summary>
public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateFormats.TryParseDate(reader.GetString(), out var date)
            ? date
            : throw new JsonException($"Data inválida. Use o formato {DateFormats.Date}.");

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(DateFormats.Date, CultureInfo.InvariantCulture));
}

/// <summary>
/// Serializa <see cref="DateTime"/> (UTC) no formato dd-MM-yyyy HH:mm:ss.
/// </summary>
public sealed class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateTime.TryParseExact(reader.GetString(), DateFormats.DateTime, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dateTime)
            ? dateTime
            : throw new JsonException($"Data/hora inválida. Use o formato {DateFormats.DateTime}.");

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToUniversalTime().ToString(DateFormats.DateTime, CultureInfo.InvariantCulture));
}
