namespace HotelRoomAvailability.Models;

public record Hotel
{
    public string? Id { get; init; }

    public string? Name { get; init; }

    public IEnumerable<RoomType>? RoomTypes { get; init; }

    public IEnumerable<Room>? Rooms { get; init; }
}
