using Newtonsoft.Json;
using System.Text;

namespace GuestlineCodeTest.Hotels;

public record Hotel
{
    public string? Id { get; init; }

    public string? Name { get; init; }

    public IEnumerable<RoomType>? RoomTypes { get; init; }

    public IEnumerable<Room>? Rooms { get; init; }
}

public record RoomType
{
    public string? Code { get; init; }

    public string? Description { get; init; }

    public bool Overbooking { get; init; }

    public IEnumerable<string>? Amenities { get; init; }

    public IEnumerable<string>? Features { get; init; }
}

public record Room
{
    public string? RoomId { get; init; }

    public string? RoomType { get; init; }
}

public record Booking
{
    public string? HotelId { get; init; }

    [JsonConverter(typeof(CustomDateFormatConverter))]
    public DateTime Arrival { get; init; }

    [JsonConverter(typeof(CustomDateFormatConverter))]
    public DateTime Departure { get; init; }

    public string? RoomType { get; init; }

    public string? RoomRate { get; init; }
}

public class CustomDateFormatConverter : JsonConverter<DateTime>
{
    private readonly string _dateFormat = "yyyyMMdd";

    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString(_dateFormat));
    }

    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return default;
        }

        string? dateString = (string?)reader.Value;

        if (DateTime.TryParseExact(dateString, _dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime date))
        {
            return date;
        }

        throw new JsonSerializationException($"Unable to convert '{dateString}' to a date using the format '{_dateFormat}'.");
    }
}

internal class Program
{
    private static List<Hotel>? Hotels;
    private static List<Booking>? Bookings;

    public static void Main(string[] args)
    {
        string hotelsFile = string.Empty;
        string bookingsFile = string.Empty;

        if (args.Length < 4)
        {
            Console.WriteLine("Usage: dotnet run --hotels [path_to_hotels_json_file] --bookings [path_to_bookings_json_file]");

            return;
        }

        if (args[0] != "--hotels")
        {
            Console.WriteLine("Cannot find param '--hotels'");
            Console.WriteLine("Usage: dotnet run --hotels [path_to_hotels_json_file] --bookings [path_to_bookings_json_file]");

            return;
        }

        if (args[0] == "--hotels")
        {
            hotelsFile = args[1];

            if (!File.Exists(hotelsFile))
            {
                Console.WriteLine($"Cannot load '{hotelsFile}' as it does not exist");

                return;
            }
        }

        if (args[2] != "--bookings")
        {
            Console.WriteLine("Cannot find param '--bookings'");
            Console.WriteLine("Usage: dotnet run --hotels [path_to_hotels_json_file] --bookings [path_to_bookings_json_file]");

            return;
        }

        if (args[2] == "--bookings")
        {
            bookingsFile = args[3];

            if (!File.Exists(bookingsFile))
            {
                Console.WriteLine($"Cannot load '{bookingsFile}' as it does not exist");

                return;
            }
        }

        Hotels = LoadData<Hotel>(hotelsFile);
        Bookings = LoadData<Booking>(bookingsFile);

        string? input;
        while (!string.IsNullOrWhiteSpace(input = Console.ReadLine()))
        {
            if (input.StartsWith("Availability"))
            {
                HandleAvailability(input);
            }
            else if (input.StartsWith("Search"))
            {
                HandleSearch(input);
            }
        }
    }

    private static List<T> LoadData<T>(string filePath)
    {
        try
        {
            return JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filePath)) ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading file {filePath}: {ex.Message}");
            return [];
        }
    }

    private static void HandleAvailability(string input)
    {
        var availabilities = input.Split(["  "], StringSplitOptions.RemoveEmptyEntries);

        var parsedAvailabilities = new List<(string HotelId, (DateTime StartDate, DateTime EndDate) DateRange, string RoomType, bool AllowOverbooking)>();

        foreach (var availability in availabilities)
        {
            var parts = availability.Split(['(', ',', ')'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4 || parts.Length > 5)
            {
                Console.WriteLine("Invalid command format.");
                return;
            }

            var hotelId = parts[1].Trim();
            var dateRange = parts[2].Trim();
            var roomType = parts[3].Trim();

            var overbooking = parts.Length == 5 && parts[4].Trim().Equals("ovb", StringComparison.CurrentCultureIgnoreCase);

            var dates = parts[2].Contains('-') ? parts[2].Split('-') : [parts[2], parts[2]];

            if (!DateTime.TryParseExact(dates[0].Trim(), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var startDate) ||
                !DateTime.TryParseExact(dates[1].Trim(), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var endDate))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }

            parsedAvailabilities.Add((hotelId, (startDate, endDate), roomType, overbooking));
        }

        var result = new List<string>();

        foreach (var parsedAvailability in parsedAvailabilities)
        {
            var hotel = Hotels!.FirstOrDefault(h => h.Id == parsedAvailability.HotelId);
            if (hotel is null)
            {
                Console.WriteLine("Hotel not found.");
                return;
            }

            var roomCount = hotel?.Rooms?.Count(r => r.RoomType == parsedAvailability.RoomType);

            var bookedRooms = Bookings!
                .Where(b => b.HotelId == parsedAvailability.HotelId
                    && b.RoomType == parsedAvailability.RoomType
                    && b.Arrival < parsedAvailability.DateRange.EndDate && b.Departure > parsedAvailability.DateRange.StartDate)
                .Count();

            var resultDateRange = parsedAvailability.DateRange.StartDate == parsedAvailability.DateRange.EndDate
                ? parsedAvailability.DateRange.StartDate.ToString("yyyyMMdd")
                : $"{parsedAvailability.DateRange.StartDate:yyyyMMdd}-{parsedAvailability.DateRange.EndDate:yyyyMMdd}";

            if (roomCount - bookedRooms > 0)
            {
                result.Add($"({resultDateRange},{roomCount - bookedRooms})");
            }
            else if (hotel?.RoomTypes?.Any(rt => rt.Code == parsedAvailability.RoomType && rt.Overbooking && parsedAvailability.AllowOverbooking) == true)
            {
                result.Add($"({resultDateRange},{-1})");
            }
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendJoin(", ", result);

        Console.WriteLine(stringBuilder.ToString());
    }

    private static void HandleSearch(string input)
    {
        var parts = input.Split([ '(', ',', ')' ], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4)
        {
            Console.WriteLine("Invalid command format.");
            return;
        }

        string hotelId = parts[1].Trim();
        if (!int.TryParse(parts[2].Trim(), out var daysAhead))
        {
            Console.WriteLine("Invalid number of days.");
            return;
        }

        string roomType = parts[3].Trim();

        var hotel = Hotels?.FirstOrDefault(h => h.Id == hotelId);
        if (hotel is null)
        {
            Console.WriteLine("Hotel not found.");
            return;
        }

        DateTime today = DateTime.Now;

        var availableRooms = hotel.Rooms?
            .Where(r => r.RoomType == roomType)
            .Select(r => new { r.RoomId, Availability = new List<(DateTime? AvailableFrom, DateTime? AvailableTo)> { (today, today.AddDays(daysAhead)) } })
            .ToList() ?? [];

        var bookedRooms = Bookings!
            .Where(b => b.HotelId == hotelId && b.RoomType == roomType && b.Departure > today);

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

        var totalAvailability = availableRooms?
            .SelectMany(x => x.Availability)
            .GroupBy(pair => pair)
            .Select(group => new { 
                StartDate = group.Key.AvailableFrom, 
                EndDate = group.Key.AvailableTo, 
                Count = group.Count() })
            .OrderBy(g => g.StartDate)
            .ThenBy(g => g.EndDate)
            .Select(g => $"({g.StartDate:yyyyMMdd}-{g.EndDate:yyyyMMdd},{g.Count})")
            .ToList() ?? [];

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendJoin(", ", totalAvailability);

        Console.WriteLine(stringBuilder.ToString());
    }
}
