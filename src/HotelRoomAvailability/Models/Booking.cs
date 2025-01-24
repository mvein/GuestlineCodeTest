using HotelRoomAvailability.Serialization;
using Newtonsoft.Json;

namespace HotelRoomAvailability.Models;

public record Booking
{
    public string? HotelId { get; init; }

    [JsonConverter(typeof(CustomDateFormatConverter))]
    public DateTime Arrival { get; init; }

    [JsonConverter(typeof(CustomDateFormatConverter))]
    public DateTime Departure { get; init; }

    public string? RoomType { get; init; }

    public string? RoomRate { get; init; }
}
