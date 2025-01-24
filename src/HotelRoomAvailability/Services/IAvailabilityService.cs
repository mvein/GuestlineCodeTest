using HotelRoomAvailability.Models;

namespace HotelRoomAvailability.Services;

public interface IAvailabilityService
{
    IEnumerable<RoomsAvailability> Availability(params RoomAvailabilityCommand[] roomAvailabilityCommands); // HandleAvailability

    IEnumerable<RoomsAvailability> Search(string roomType, string hotelId, int daysAhead); // HandleSearch
}