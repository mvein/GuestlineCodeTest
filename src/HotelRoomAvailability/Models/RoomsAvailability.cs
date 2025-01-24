namespace HotelRoomAvailability.Models;

public record RoomsAvailability
{
    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public int Count { get; init; }
}