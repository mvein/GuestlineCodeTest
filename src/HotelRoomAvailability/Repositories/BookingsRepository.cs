using HotelRoomAvailability.Consts;
using HotelRoomAvailability.Models;
using HotelRoomAvailability.Repositories.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.CompilerServices;

namespace HotelRoomAvailability.Repositories;

public class BookingsRepository(IMemoryCache memoryCache) : FileSourceRepository<Booking>(memoryCache), IBookingsRepository
{
    protected override string CacheKey => Args.Bookings;

    public async IAsyncEnumerable<Booking> Get(string hotelId, string roomType, DateTime startDate, DateTime endDate, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var booking in LoadData(cancellationToken))
        {
            if (booking.HotelId == hotelId && booking.RoomType == roomType && booking.Arrival <= endDate && booking.Departure > startDate)
            {
                yield return booking;
            }
        }
    }
}