using HotelRoomAvailability.Models;

namespace HotelRoomAvailability.Repositories.Abstractions;

public interface IHotelsRepository
{
    Task<Hotel?> Get(string id, CancellationToken cancellationToken = default);
}