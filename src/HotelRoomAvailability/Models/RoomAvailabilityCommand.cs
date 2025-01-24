namespace HotelRoomAvailability.Models;

public record RoomAvailabilityCommand
{
    // TODO: check whether overbooking is poperly handled it terms of being consistent with room type; whether room type allows to use overbooking
    //var overbooking

    public string? HotelId { get; init; }

    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public string? RoomType { get; init; }

    public bool AllowOverbooking { get; init; }
}