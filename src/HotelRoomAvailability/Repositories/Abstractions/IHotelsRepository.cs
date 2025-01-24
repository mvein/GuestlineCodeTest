using HotelRoomAvailability.Models;

namespace HotelRoomAvailability.Repositories.Abstractions;

public interface IHotelsRepository
{
    Hotel? Get(string id);
}