# Hotel Room Availability Application

This application reads from JSON files containing hotel and booking data, then allows users to check room availability for a specified hotel, date range, and room type. It supports overbooking, meaning the availability value can be negative to indicate that the hotel is over capacity for that room type.

## Features

- **Room Availability Check**: Determine the availability count for a specified room type and date range.
- **Search Room Availability**: Retrieve a list of date ranges and availability where the room is available for a specified number of days ahead.
- **Overbooking Support**: Handle overbookings by allowing availability values to be negative.

## Getting Started

### Prerequisites

- .NET Core SDK installed on your machine.
- JSON files containing hotel and booking data.

### Getting the Application

To clone the application from the GitHub repository, use the following command: 
```sh
# git clone https://github.com/mvein/GuestlineCodeTest.git
```

### Running the Application

1. **Prepare JSON Data Files**:
   - Create a JSON file with hotel data (e.g., `hotels.json`).
   - Create a JSON file with booking data (e.g., `bookings.json`).

2. **Navigate to the project directory:**

```sh
# cd GuestlineCodeTest
```

3. **Building the application**

```sh
# dotnet build .\src\HotelRoomAvailability\HotelRoomAvailability.csproj
```

4. **Command to Run the Program**:
```sh
# dotnet run --project .\src\HotelRoomAvailability\HotelRoomAvailability.csproj --hotels path_to_hotels.json --bookings path_to_bookings.json
```

### Commands supported
**Availability**

The program gives the availability count for the specified room type and date range.

Usage:

``Availability({hotel_name}, {date_in_format_yyyyMMdd}, {room_type})``

``Availability({hotel_name}, {date_in_format_yyyyMMdd}, {room_type}, ovb)``

``Availability({hotel_name}, {start_date_in_format_yyyyMMdd}-{start_date_in_format_yyyyMMdd}, {room_type})``

``Availability({hotel_name}, {start_date_in_format_yyyyMMdd}-{start_date_in_format_yyyyMMdd}, {room_type}, ovb)``

Examples:
```sh
Availability(H1, 20240901, SGL)
output: (20240901, 2)

Availability(H1, 20240901-20240903, DBL)
output: (20240901-20240903, -1) -- when hotel allows overbooking and 'ovb' param was used
```

**Search**

The program returns a comma-separated list of date ranges and availability where the room is available. If there is no availability, the program returns an empty line.

Usage:

``Search(H1, 365, SGL)``

Examples:
```sh
Search(H1, 365, SGL)
output: (20241101-20241103,2), (20241203-20241210, 1)
```

### Data Requirements
#### Hotel Data
**File Format:** JSON

**Structure:**
```json
[
  {
    "id": "H1",
    "name": "Hotel Name",
    "roomTypes": [
      {
        "code": "SGL",
        "description": "Single Room",
        "amenities": ["WiFi", "TV"],
        "features": ["Sea View"],
        "overbooking": true
      }
    ],
    "rooms": [
      {
        "roomType": "SGL",
        "roomId": "101"
      }
    ]
  }
]
```

#### Booking data
**File Format** JSON

**Structure:**
```json
[
  {
    "hotelId": "H1",
    "arrival": "20240901",
    "departure": "20240903",
    "roomType": "SGL",
    "roomRate": "Standard"
  }
]
```