using HotelRoomAvailability.Consts;
using HotelRoomAvailability.Models;
using HotelRoomAvailability.Repositories.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace HotelRoomAvailability.Repositories;

public class HotelRepository(IMemoryCache memoryCache) : FileSourceRepository<Hotel>(memoryCache), IHotelsRepository
{
    protected override string CacheKey => Args.Hotels;

    public async Task<Hotel?> Get(string id, CancellationToken cancellationToken = default)
    {
        await foreach (var hotel in LoadData(cancellationToken))
        {
            if (hotel.Id == id)
            {
                return hotel;
            }
        }

        return null;
    }
}