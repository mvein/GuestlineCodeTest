using HotelRoomAvailability.Consts;
using HotelRoomAvailability.Models;
using HotelRoomAvailability.Repositories.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace HotelRoomAvailability.Repositories;

public class HotelRepository(IMemoryCache memoryCache) : FileSourceRepository<Hotel>(memoryCache), IHotelsRepository
{
    protected override string CacheKey => Args.Hotels;

    public Hotel? Get(string id)
        => Data.FirstOrDefault(h => h.Id == id);
}