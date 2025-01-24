namespace HotelRoomAvailability.Models;

public record RoomType
{
    public string? Code { get; init; }

    public string? Description { get; init; }

    public bool Overbooking { get; init; }

    public IEnumerable<string>? Amenities { get; init; }

    public IEnumerable<string>? Features { get; init; }
}
