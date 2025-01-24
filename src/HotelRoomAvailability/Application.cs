using HotelRoomAvailability.Consts;
using HotelRoomAvailability.Models;
using HotelRoomAvailability.Serialization;
using HotelRoomAvailability.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text;

namespace HotelRoomAvailability;

public class Application(IOptions<CommandLineOptions> options, IMemoryCache cache, IAvailabilityService availabilityService) : IApplication
{
    private readonly string[]? _args = options.Value.Args;
    private readonly IMemoryCache _cache = cache;
    private readonly IAvailabilityService _availabilityService = availabilityService;

    public Task Run()
        => !ParseArgs()
        ? Task.CompletedTask
        : RunInternal();

    private async Task RunInternal()
    {
        string? input;
        while (!string.IsNullOrWhiteSpace(input = Console.ReadLine()))
        {
            if (input.StartsWith(Args.AvailabilityCommand.Name))
            {
                await HandleAvailabilityCommands(ParseAvailabilityCommands(input));
            }
            else if (input.StartsWith(Args.SearchCommand.Name))
            {
                var (roomType, hotelId, daysAhead) = ParseSearchCommand(input);

                if (roomType is null || hotelId is null || daysAhead is null)
                {
                    continue;
                }

                await HandleSearchCommand(roomType!, hotelId!, daysAhead!.Value);
            }
        }
    }

    private bool ParseArgs()
    {
        if (_args is null)
        {
            Console.WriteLine("Usage: dotnet run --hotels [path_to_hotels_json_file] --bookings [path_to_bookings_json_file]");

            return false;
        }

        if (_args.Length < 4)
        {
            Console.WriteLine("Usage: dotnet run --hotels [path_to_hotels_json_file] --bookings [path_to_bookings_json_file]");

            return false;
        }

        if (_args[0] != "--hotels")
        {
            Console.WriteLine("Cannot find param '--hotels'");
            Console.WriteLine("Usage: dotnet run --hotels [path_to_hotels_json_file] --bookings [path_to_bookings_json_file]");

            return false;
        }

        if (_args[0] == "--hotels")
        {
            var hotelsFile = _args[1];

            if (!File.Exists(hotelsFile))
            {
                Console.WriteLine($"Cannot load '{hotelsFile}' as it does not exist");

                return false;
            }

            _cache.Set("--hotels", hotelsFile);
        }

        if (_args[2] != "--bookings")
        {
            Console.WriteLine("Cannot find param '--bookings'");
            Console.WriteLine("Usage: dotnet run --hotels [path_to_hotels_json_file] --bookings [path_to_bookings_json_file]");

            return false;
        }

        if (_args[2] == "--bookings")
        {
            var bookingsFile = _args[3];

            if (!File.Exists(bookingsFile))
            {
                Console.WriteLine($"Cannot load '{bookingsFile}' as it does not exist");

                return false;
            }

            _cache.Set("--bookings", bookingsFile);
        }

        return true;
    }

    private static IEnumerable<RoomAvailabilityCommand> ParseAvailabilityCommands(string input)
    {
        var availabilities = input.Split(["  "], StringSplitOptions.RemoveEmptyEntries);

        var roomAvailabilityCommands = new List<RoomAvailabilityCommand>();

        foreach (var availability in availabilities)
        {
            var parts = availability.Split(['(', ',', ')'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4 || parts.Length > 5)
            {
                Console.WriteLine("Invalid command format.");
                return [];
            }

            var hotelId = parts[1].Trim();
            var roomType = parts[3].Trim();

            var overbooking = parts.Length == 5 && parts[4].Trim().Equals(Args.AvailabilityCommand.Overbooking, StringComparison.CurrentCultureIgnoreCase);

            var dates = parts[2].Contains('-') ? parts[2].Split('-') : [parts[2], parts[2]];

            if (!DateTime.TryParseExact(dates[0].Trim(), CustomDateFormatConverter.DateFormat, null, System.Globalization.DateTimeStyles.None, out var startDate) ||
                !DateTime.TryParseExact(dates[1].Trim(), CustomDateFormatConverter.DateFormat, null, System.Globalization.DateTimeStyles.None, out var endDate))
            {
                Console.WriteLine("Invalid date format.");
                return [];
            }

            roomAvailabilityCommands.Add(new RoomAvailabilityCommand
            {
                HotelId = hotelId,
                StartDate = startDate,
                EndDate = endDate,
                RoomType = roomType,
                AllowOverbooking = overbooking,
            });

        }

        return roomAvailabilityCommands;
    }

    private async Task HandleAvailabilityCommands(IEnumerable<RoomAvailabilityCommand> availabilities)
    {
        var roomsAvailabilities = await _availabilityService.Availability([.. availabilities]);

        var result = roomsAvailabilities.Select(r => r.StartDate == r.EndDate
            ? $"({r.StartDate.ToString(CustomDateFormatConverter.DateFormat)},{r.Count})"
            : $"({r.StartDate.ToString(CustomDateFormatConverter.DateFormat)}-{r.EndDate.ToString(CustomDateFormatConverter.DateFormat)},{r.Count})");

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendJoin(", ", result);

        Console.WriteLine(stringBuilder.ToString());
    }

    private (string? RoomType, string? HotelId, int? DaysAhead) ParseSearchCommand(string input)
    {
        var parts = input.Split(['(', ',', ')'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4)
        {
            Console.WriteLine("Invalid command format.");
            return (null, null, null);
        }

        string hotelId = parts[1].Trim();
        if (!int.TryParse(parts[2].Trim(), out var daysAhead))
        {
            Console.WriteLine("Invalid number of days.");
            return (null, null, null);
        }

        string roomType = parts[3].Trim();

        return (roomType, hotelId, daysAhead);
    }

    private async Task HandleSearchCommand(string roomType, string hotelId, int daysAhead)
    {
        var roomsAvailabilities = await _availabilityService.Search(roomType, hotelId, daysAhead);

        var totalAvailability = roomsAvailabilities.Select(static r => $"({r.StartDate.ToString(CustomDateFormatConverter.DateFormat)}-{r.EndDate.ToString(CustomDateFormatConverter.DateFormat)},{r.Count})");

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendJoin(", ", totalAvailability);

        Console.WriteLine(stringBuilder.ToString());
    }
}