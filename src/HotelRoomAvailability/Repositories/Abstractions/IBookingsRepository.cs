using HotelRoomAvailability.Models;

namespace HotelRoomAvailability.Repositories.Abstractions;

public interface IBookingsRepository
{
    IEnumerable<Booking> Get(string hotelId, string roomType, DateTime startDate, DateTime endDate);
}