using FluentAssertions;
using Program = HotelRoomAvailability.Program;

namespace HotelRoomAvailability.IntegrationTests;

public class ProgramTests
{
	[SetUp]
	public void SetUp()
	{
        File.WriteAllText("hotels.json", GetHotelsJson());
        File.WriteAllText("bookings.json", GetBookingsJson());
    }

    [TearDown]
    public void TearDown()
    {
        RemoveFileIfExists("hotels.json");
        RemoveFileIfExists("bookings.json");
    }

    [Test]
    public void Should_PrintUsage_When_ArgsLengthIsLessThan4()
    {
        // Arrange
        var args = new string[] { "--hotels", "hotels.json" };

        using var output = new StringWriter();
        Console.SetOut(output);

        // Act
        Program.Main(args);

        // Assert
        var expectedOutput = "Usage: dotnet run --hotels [path_to_hotels_json_file] --bookings [path_to_bookings_json_file]";
        output.ToString().Should().Contain(expectedOutput);
    }

    [Test]
    public void Should_PrintError_When_HotelsParamIsMissing()
    {
        // Arrange
        var args = new string[] { "--invalid", "hotels.json", "--bookings", "bookings.json" };

        using var output = new StringWriter();
        Console.SetOut(output);

        // Act
        Program.Main(args);

        // Assert
        var expectedOutput = "Cannot find param '--hotels'";
        output.ToString().Should().Contain(expectedOutput);
        output.ToString().Should().Contain("Usage: dotnet run --hotels [path_to_hotels_json_file] --bookings [path_to_bookings_json_file]");
    }

    [Test]
    public void Should_PrintError_When_HotelsFileDoesNotExist()
    {
        // Arrange
        var args = new string[] { "--hotels", "non_existing_hotels.json", "--bookings", "bookings.json" };

        using var output = new StringWriter();
        Console.SetOut(output);

        // Act
        Program.Main(args);

        // Assert
        var expectedOutput = "Cannot load 'non_existing_hotels.json' as it does not exist";
        output.ToString().Should().Contain(expectedOutput);
    }

    [Test]
    public void Should_PrintError_When_BookingsParamIsMissing()
    {
        // Arrange
        var args = new string[] { "--hotels", "hotels.json", "--invalid", "bookings.json" };

        using var output = new StringWriter();
        Console.SetOut(output);

        // Act
        Program.Main(args);

        // Assert
        var expectedOutput = "Cannot find param '--bookings'";
        output.ToString().Should().Contain(expectedOutput);
        output.ToString().Should().Contain("Usage: dotnet run --hotels [path_to_hotels_json_file] --bookings [path_to_bookings_json_file]");
    }

    [Test]
    public void Should_PrintError_When_BookingsFileDoesNotExist()
    {
        // Arrange
        var args = new string[] { "--hotels", "hotels.json", "--bookings", "non_existing_bookings.json" };

        using var output = new StringWriter();
        Console.SetOut(output);

        // Act
        Program.Main(args);

        // Assert
        var expectedOutput = "Cannot load 'non_existing_bookings.json' as it does not exist";
        output.ToString().Should().Contain(expectedOutput);
    }

    [TestCase("Availability(H1, 20240901, SGL)", "(20240901,2)")]
	[TestCase("Availability(H1, 20240901-20240903, DBL)", "(20240901-20240903,1)")]
	[TestCase("Availability(H1, 20240901, SGL)  Availability(H1, 20240901-20240903, DBL)", "(20240901,2), (20240901-20240903,1)")]
	[TestCase("Availability(H1, 20250202-20250205, SGL)", "\r\n")]
	[TestCase("Availability(H1, 20250201-20250203, DBL, ovb)", "(20250201-20250203,-1)")]
	public void Should_ProcessAvailabilityCommand_Correctly(string availability, string expectedOutput)
    {
        // Arrange
        var inputCommands = new StringReader(availability);
        Console.SetIn(inputCommands);

        using var output = new StringWriter();
        Console.SetOut(output);

        // Act
        Program.Main(["--hotels", "hotels.json", "--bookings", "bookings.json"]);

        // Assert
        output.ToString().Should().Contain(expectedOutput);
    }

    [Test]
    public void Should_ProcessSearchCommandAndPrintAvailability_WhenAvailabilityExists()
    {
        // Arrange
		var arrival1 = DateTime.Now.AddDays(10);
        var departure1 = arrival1.AddDays(5);
        var arrival2 = DateTime.Now.AddDays(8);
        var departure2 = arrival2.AddDays(7);
        var bookingsJson = $@"[
			{{
				""hotelId"": ""H1"",
				""arrival"": ""{arrival1:yyyyMMdd}"",
				""departure"": ""{departure1:yyyyMMdd}"",
				""roomType"": ""DBL"",
				""roomRate"": ""Prepaid""
			}},
			{{
				""hotelId"": ""H1"",
				""arrival"": ""{arrival2:yyyyMMdd}"",
				""departure"": ""{departure2:yyyyMMdd}"",
				""roomType"": ""DBL"",
				""roomRate"": ""Prepaid""
			}}				
		]";

        File.WriteAllText("bookings.json", bookingsJson);

		var daysAhead = 100;
        var inputCommands = new StringReader($"Search (H1, {daysAhead}, DBL)");
        Console.SetIn(inputCommands);

        using var output = new StringWriter();
        Console.SetOut(output);

        // Act
        Program.Main(["--hotels", "hotels.json", "--bookings", "bookings.json"]);

        // Assert
        DateTime today = DateTime.Now;
        string[] expectedOutput = [
            $"({today:yyyyMMdd}-{arrival1:yyyyMMdd},1)",
            $"({today:yyyyMMdd}-{arrival2:yyyyMMdd},1)",
            $"({departure1:yyyyMMdd}-{today.AddDays(daysAhead):yyyyMMdd},2)"];        
        expectedOutput.All(expected => output.ToString().Contains(expected)).Should().BeTrue();        
    }

    [Test]
    public void Should_ProcessSearchCommandAndReturnsEmptyLine_WhenAvailabilityDoesNotExist()
    {
        // Arrange
		var today = DateTime.Now;
        var departure = today.AddDays(15);
        var bookingsJson = $@"[
			{{
				""hotelId"": ""H1"",
				""arrival"": ""{today:yyyyMMdd}"",
				""departure"": ""{departure:yyyyMMdd}"",
				""roomType"": ""DBL"",
				""roomRate"": ""Prepaid""
			}},
			{{
				""hotelId"": ""H1"",
				""arrival"": ""{today:yyyyMMdd}"",
				""departure"": ""{departure:yyyyMMdd}"",
				""roomType"": ""DBL"",
				""roomRate"": ""Prepaid""
			}}				
		]";

        File.WriteAllText("bookings.json", bookingsJson);

        var daysAhead = 15;
        var inputCommands = new StringReader($"Search (H1, {daysAhead}, DBL)");
        Console.SetIn(inputCommands);

        using var output = new StringWriter();
        Console.SetOut(output);

        // Act
        Program.Main(["--hotels", "hotels.json", "--bookings", "bookings.json"]);

        // Assert
		output.ToString().Should().Be("\r\n");
    }

    private static string GetHotelsJson()
		=> @"[
				{
					""id"": ""H1"",
					""name"": ""Hotel California"",
					""roomTypes"": [
						{
							""code"": ""SGL"",
							""description"": ""Single Room"",
							""amenities"": [
								""WiFi"",
								""TV""
							],
							""features"": [
								""Non-smoking""
							]
						},
						{
							""code"": ""DBL"",
							""description"": ""Double Room"",
							""overbooking"": true,
							""amenities"": [
								""WiFi"",
								""TV"",
								""Minibar""
							],
							""features"": [
								""Non-smoking"",
								""Sea View""
							]
						}
					],
					""rooms"": [
						{
							""roomType"": ""SGL"",
							""roomId"": ""101""
						},
						{
							""roomType"": ""SGL"",
							""roomId"": ""102""
						},
						{
							""roomType"": ""DBL"",
							""roomId"": ""201""
						},
						{
							""roomType"": ""DBL"",
							""roomId"": ""202""
						}
					]
				}
			]";

	private static string GetBookingsJson()
		=> @"[
				{
					""hotelId"": ""H1"",
					""arrival"": ""20240901"",
					""departure"": ""20240903"",
					""roomType"": ""DBL"",
					""roomRate"": ""Prepaid""
				},
				{
					""hotelId"": ""H1"",
					""arrival"": ""20240904"",
					""departure"": ""20240905"",
					""roomType"": ""DBL"",
					""roomRate"": ""Prepaid""
				},
				{
					""hotelId"": ""H1"",
					""arrival"": ""20240902"",
					""departure"": ""20240905"",
					""roomType"": ""SGL"",
					""roomRate"": ""Standard""
				},
				{
					""hotelId"": ""H1"",
					""arrival"": ""20250201"",
					""departure"": ""20250203"",
					""roomType"": ""DBL"",
					""roomRate"": ""Prepaid""
				},
				{
					""hotelId"": ""H1"",
					""arrival"": ""20250201"",
					""departure"": ""20250203"",
					""roomType"": ""DBL"",
					""roomRate"": ""Prepaid""
				},
				{
					""hotelId"": ""H1"",
					""arrival"": ""20250202"",
					""departure"": ""20250205"",
					""roomType"": ""SGL"",
					""roomRate"": ""Standard""
				},
				{
					""hotelId"": ""H1"",
					""arrival"": ""20250202"",
					""departure"": ""20250205"",
					""roomType"": ""SGL"",
					""roomRate"": ""Standard""
				}
			]";

	private static void RemoveFileIfExists(string filePath)
	{
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}