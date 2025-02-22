﻿using FluentAssertions;
using HotelRoomAvailability.Extensions;
using HotelRoomAvailability.Models;
using HotelRoomAvailability.Repositories.Abstractions;
using HotelRoomAvailability.Services;
using NSubstitute;

namespace HotelRoomAvailability.UnitTests.Services;

[TestFixture]
public class AvailabilityServiceTests
{
    private IHotelsRepository _hotelsRepository;
    private IBookingsRepository _bookingsRepository;

    private AvailabilityService _sut;

    [SetUp]
    public void SetUp()
    {
        _hotelsRepository = Substitute.For<IHotelsRepository>();
        _bookingsRepository = Substitute.For<IBookingsRepository>();
        _sut = new AvailabilityService(_hotelsRepository, _bookingsRepository);
    }

    [Test]
    public async Task Should_ReturnEmpty_When_HotelNotFoundInAvailabilityAsync()
    {
        // Arrange
        _hotelsRepository.Get(Arg.Any<string>()).Returns((Hotel?)null);

        var commands = new RoomAvailabilityCommand[] { new() { HotelId = "H1" } };

        // Act
        var result = await _sut.Availability(commands);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task Should_ReturnCorrectAvailabilityCount_When_HotelAndBookingsExistAsync()
    {
        // Arrange
        var hotel = new Hotel
        {
            Id = "H1",
            Rooms =
            [
                new() { RoomType = "SGL" },
            new() { RoomType = "SGL" }
            ],
            RoomTypes =
            [
                new() { Code = "SGL", Overbooking = false }
            ]
        };

        var bookings = new List<Booking>
        {
            new() { HotelId = "H1", RoomType = "SGL", Arrival = DateTime.Now, Departure = DateTime.Now.AddDays(1) }
        };

        _hotelsRepository.Get("H1").Returns(hotel);
        _bookingsRepository.Get("H1", "SGL", Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(bookings.ToAsyncEnumerable());

        var commands = new RoomAvailabilityCommand[]
        {
        new() { HotelId = "H1", RoomType = "SGL", StartDate = DateTime.Now, EndDate = DateTime.Now }
        };

        // Act
        var result = await _sut.Availability(commands);

        // Assert
        result.Should().HaveCount(1);
        result.First().Count.Should().Be(1); // 2 rooms - 1 booked room = 1 available room
    }

    [Test]
    public async Task Should_AllowOverbooking_When_OverbookingIsAllowedAsync()
    {
        // Arrange
        var hotel = new Hotel
        {
            Id = "H1",
            Rooms =
            [
                new() { RoomType = "DBL" }
            ],
            RoomTypes =
            [
                new() { Code = "DBL", Overbooking = true }
            ]
        };

        var bookings = new List<Booking>
        {
            new() { HotelId = "H1", RoomType = "DBL", Arrival = DateTime.Now, Departure = DateTime.Now.AddDays(1) }
        };

        _hotelsRepository.Get("H1").Returns(hotel);
        _bookingsRepository.Get("H1", "DBL", Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(bookings.ToAsyncEnumerable());

        var commands = new RoomAvailabilityCommand[]
        {
        new() { HotelId = "H1", RoomType = "DBL", StartDate = DateTime.Now, EndDate = DateTime.Now, AllowOverbooking = true }
        };

        // Act
        var result = await _sut.Availability(commands);

        // Assert
        result.Should().HaveCount(1);
        result.First().Count.Should().Be(-1); // Overbooking allowed, thus count is -1
    }

    [Test]
    public async Task Should_ReturnEmpty_When_HotelNotFoundInSearchAsync()
    {
        // Arrange
        _hotelsRepository.Get(Arg.Any<string>()).Returns((Hotel?)null);

        // Act
        var result = await _sut.Search("SGL", "H1", 10);

        // Assert
        result.Should().BeEmpty();
    }
}