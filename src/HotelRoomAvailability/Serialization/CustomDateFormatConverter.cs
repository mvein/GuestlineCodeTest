using Newtonsoft.Json;

namespace HotelRoomAvailability.Serialization;

public class CustomDateFormatConverter : JsonConverter<DateTime>
{
    public const string DateFormat = "yyyyMMdd";

    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(DateFormat));
    }

    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return default;
        }

        string? dateString = (string?)reader.Value;

        if (DateTime.TryParseExact(dateString, DateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime date))
        {
            return date;
        }

        throw new JsonSerializationException($"Unable to convert '{dateString}' to a date using the format '{DateFormat}'.");
    }
}
