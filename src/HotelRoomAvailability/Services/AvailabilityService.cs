using HotelRoomAvailability.Models;
using HotelRoomAvailability.Repositories.Abstractions;

namespace HotelRoomAvailability.Services;

public class AvailabilityService(IHotelsRepository hotelsRepository, IBookingsRepository bookingsRepository) : IAvailabilityService
{
    private readonly IHotelsRepository _hotelsRepository = hotelsRepository;

    private readonly IBookingsRepository _bookingsRepository = bookingsRepository;

    public IEnumerable<RoomsAvailability> Availability(params RoomAvailabilityCommand[] roomAvailabilityCommands)
    {
        var result = new List<RoomsAvailability>();

        foreach (var availabilityCommand in roomAvailabilityCommands)
        {
            var hotel = _hotelsRepository.Get(availabilityCommand!.HotelId!);
            if (hotel is null)
            {
                Console.WriteLine($"Hotel '{availabilityCommand!.HotelId!}' not found.");
                return [];
            }

            var roomsCount = hotel?.Rooms?.Count(r => r.RoomType == availabilityCommand.RoomType);

            var bookedRoomsCount = _bookingsRepository.Get(availabilityCommand!.HotelId!, availabilityCommand!.RoomType!, availabilityCommand.StartDate, availabilityCommand.EndDate).Count();

            int? roomsAvailabilityCount = roomsCount - bookedRoomsCount > 0
                ? roomsCount - bookedRoomsCount
                : hotel?.RoomTypes?.Any(rt => rt.Code == availabilityCommand.RoomType && rt.Overbooking && availabilityCommand.AllowOverbooking) == true
                    ? -1 : null;

            if (roomsAvailabilityCount is null)
            {
                continue;
            }

            var roomsAvailability = availabilityCommand.StartDate == availabilityCommand.EndDate
                ? new RoomsAvailability
                {
                    StartDate = availabilityCommand.StartDate,
                    EndDate = availabilityCommand.StartDate,
                    Count = roomsAvailabilityCount.Value,
                }
                : new RoomsAvailability
                {
                    StartDate = availabilityCommand.StartDate,
                    EndDate = availabilityCommand.EndDate,
                    Count = roomsAvailabilityCount.Value,
                };

            result.Add(roomsAvailability);
        };

        return result;
    }

    public IEnumerable<RoomsAvailability> Search(string roomType, string hotelId, int daysAhead)
    {
        var hotel = _hotelsRepository.Get(hotelId);
        if (hotel is null)
        {
            Console.WriteLine($"Hotel '{hotelId}' not found.");
            return [];
        }

        DateTime today = DateTime.Now;

        var availableRooms = hotel.Rooms?
            .Where(r => r.RoomType == roomType)
            .Select(r => new { r.RoomId, Availability = new List<(DateTime? AvailableFrom, DateTime? AvailableTo)> { (today.Date, today.Date.AddDays(daysAhead)) } })
            .ToList() ?? [];

        var bookedRooms = _bookingsRepository.Get(hotelId, roomType, today, today.AddDays(daysAhead));

        foreach (var bookedRoom in bookedRooms)
        {
            foreach (var availableRoom in availableRooms)
            {
                var bookedAvailabilityPeriod = availableRoom.Availability.FirstOrDefault(x => bookedRoom.Arrival == x.AvailableFrom && bookedRoom.Departure == x.AvailableTo);
                if (bookedAvailabilityPeriod is not (null, null))
                {
                    availableRoom.Availability.Remove(bookedAvailabilityPeriod);
                    break;
                }

                // When availabilityPeriod needs to be divided
                bookedAvailabilityPeriod = availableRoom.Availability.FirstOrDefault(x => bookedRoom.Arrival == x.AvailableFrom && bookedRoom.Departure < x.AvailableTo);
                if (bookedAvailabilityPeriod is not (null, null))
                {
                    (DateTime? AvailableFrom, DateTime? AvailableTo) availabilityPeriod = (bookedRoom.Departure, bookedAvailabilityPeriod.AvailableTo);
                    availableRoom.Availability.Add(availabilityPeriod);
                    availableRoom.Availability.Remove(bookedAvailabilityPeriod);
                    break;
                }

                bookedAvailabilityPeriod = availableRoom.Availability.FirstOrDefault(x => bookedRoom.Arrival > x.AvailableFrom && bookedRoom.Departure < x.AvailableTo);
                if (bookedAvailabilityPeriod is not (null, null))
                {
                    (DateTime? AvailableFrom, DateTime? AvailableTo) availabilityPeriodPart1 = (bookedAvailabilityPeriod.AvailableFrom, bookedRoom.Arrival);
                    (DateTime? AvailableFrom, DateTime? AvailableTo) availabilityPeriodPart2 = (bookedRoom.Departure, bookedAvailabilityPeriod.AvailableTo);
                    availableRoom.Availability.Add(availabilityPeriodPart1);
                    availableRoom.Availability.Add(availabilityPeriodPart2);
                    availableRoom.Availability.Remove(bookedAvailabilityPeriod);
                    break;
                }
            }
        }

        return availableRooms?
            .SelectMany(x => x.Availability)
            .GroupBy(pair => pair)
            .Select(group => new
            {
                StartDate = group.Key.AvailableFrom,
                EndDate = group.Key.AvailableTo,
                Count = group.Count()
            })
            .OrderBy(g => g.StartDate)
            .ThenBy(g => g.EndDate)
            .Select(g => new RoomsAvailability
            {
                StartDate = g.StartDate!.Value,
                EndDate = g.EndDate!.Value,
                Count = g.Count
            })
            .ToList() ?? [];
    }
}