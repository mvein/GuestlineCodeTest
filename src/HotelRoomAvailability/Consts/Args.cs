namespace HotelRoomAvailability.Consts;

public static class Args
{
    public const string Bookings = "--bookings";

    public const string Hotels = "--hotels";

    public static class AvailabilityCommand
    {
        public const string Name = "Availability";
        public const string Overbooking = "ovb";
    }

    public static class SearchCommand
    {
        public const string Name = "Search";
    }
}