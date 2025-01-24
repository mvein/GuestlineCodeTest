using HotelRoomAvailability.Models;

namespace HotelRoomAvailability.Services;

public interface IAvailabilityService
{
    Task<IEnumerable<RoomsAvailability>> Availability(params RoomAvailabilityCommand[] roomAvailabilityCommands);

    Task<IEnumerable<RoomsAvailability>> Search(string roomType, string hotelId, int daysAhead);
}