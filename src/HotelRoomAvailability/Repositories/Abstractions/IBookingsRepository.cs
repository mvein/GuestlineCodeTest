using HotelRoomAvailability.Models;

namespace HotelRoomAvailability.Repositories.Abstractions;

public interface IBookingsRepository
{
    IAsyncEnumerable<Booking> Get(string hotelId, string roomType, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}