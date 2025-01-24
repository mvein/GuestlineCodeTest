using HotelRoomAvailability.Consts;
using HotelRoomAvailability.Models;
using HotelRoomAvailability.Repositories.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace HotelRoomAvailability.Repositories;

public class BookingsRepository(IMemoryCache memoryCache) : FileSourceRepository<Booking>(memoryCache), IBookingsRepository
{
    protected override string CacheKey => Args.Bookings;

    public IEnumerable<Booking> Get(string hotelId, string roomType, DateTime startDate, DateTime endDate)
        => Data.Where(b => b.HotelId == hotelId
        && b.RoomType == roomType
        && b.Arrival <= endDate && b.Departure > startDate);
}