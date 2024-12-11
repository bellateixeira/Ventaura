using Microsoft.AspNetCore.Mvc;
using ventaura_backend.Services;
using ventaura_backend.Data;
using ventaura_backend.Models;
using ventaura_backend.Utils; // Import DistanceCalculator utility
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/global-events")]
public class GlobalEventsController : ControllerBase
{
    // Services and database context injected via dependency injection
    private readonly GoogleGeocodingService _googleGeocodingService;
    private readonly CombinedAPIService _combinedApiService;
    private readonly DatabaseContext _dbContext;

    // Constructor to initialize the injected services and database context
    public GlobalEventsController(GoogleGeocodingService googleGeocodingService, CombinedAPIService combinedApiService, DatabaseContext dbContext)
    {
        _googleGeocodingService = googleGeocodingService;
        _combinedApiService = combinedApiService;
        _dbContext = dbContext;
    }

    // HTTP GET endpoint for searching events with various query parameters
    [HttpGet("search")]
    public async Task<IActionResult> SearchEvents(
        [FromQuery] string city, 
        [FromQuery] int userId, 
        [FromQuery] string eventType = null,
        [FromQuery] double? maxDistance = 100, // Default maximum distance set to 100 km
        [FromQuery] double? maxPrice = null,
        [FromQuery] DateTime? startDateTime = null,
        [FromQuery] DateTime? endDateTime = null
    )
    {
        try
        {
            // **1. Type Mapping Implementation**
            // Dictionary to map various event type strings to standardized types
            var typeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "festivals-fairs", "Festivals" },
                { "sports-active-life", "Outdoors" },
                { "visual-arts", "Exhibitions" },
                { "charities", "Community" },
                { "performing-arts", "Theater" },
                { "kids-family", "Family" },
                { "film", "Film" },
                { "food-and-drink", "Food and Drink" },
                { "music", "Music" },
                { "Holiday", "Holiday" },
                { "Networking", "Networking" },
                { "Gaming", "Gaming" },
                { "Pets", "Pets" },
                { "Virtual", "Virtual" },
                { "Science", "Science" },
                { "Basketball", "Basketball" },
                { "Pottery", "Pottery" },
                { "Tennis", "Tennis" },
                { "Soccer", "Soccer" },
                { "Football", "Football" },
                { "Fishing", "Fishing" },
                { "Hiking", "Hiking" },
                { "Wellness", "Wellness" },
                { "nightlife", "Nightlife" },
                { "Workshops", "Workshops" },
                { "Conferences", "Conferences" },
                { "Hockey", "Hockey"},
                { "Baseball", "Baseball"},
                { "lectures-books", "Lectures"},
                { "fashion", "Fashion"},
                { "Motorsports/Racing", "Motorsports"},
                { "Dance", "Dance"},
                { "Comedy", "Comedy"},
                { "Pop", "Music"},
                { "Country", "Music"},
                { "Hip-Hop/Rap", "Music"},
                { "Rock", "Music"},
                { "other", "Other" }
            };

            // Retrieve the user from the database using the provided userId
            var user = await _dbContext.Users.FindAsync(userId);
            // Check if the user exists and has valid latitude and longitude
            if (user == null || user.Latitude == null || user.Longitude == null)
            {
                return BadRequest("User not found or location is missing.");
            }

            // Log the received start and end date-time parameters for debugging
            Console.WriteLine($"Received startDateTime: {startDateTime}");
            Console.WriteLine($"Received endDateTime: {endDateTime}");

            // Get the geographic coordinates for the specified city using the geocoding service
            var coordinates = await _googleGeocodingService.GetCoordinatesAsync(city);
            if (coordinates == null) 
            {
                return NotFound(new { Message = "Could not find coordinates for the specified city." });
            }

            // Extract latitude and longitude from the retrieved coordinates
            var searchLatitude = coordinates.Value.latitude;
            var searchLongitude = coordinates.Value.longitude;

            // **3. Fetch Events**
            // Fetch events from the combined API service based on the search coordinates and userId
            var apiEvents = await _combinedApiService.FetchEventsAsync(searchLatitude, searchLongitude, userId);
            // Fetch host-managed events from the database
            var hostEvents = await _dbContext.HostEvents.ToListAsync();

            // List to store processed events fetched from the API
            var processedApiEvents = new List<CombinedEvent>();

            // Iterate through each event fetched from the API
            foreach (var e in apiEvents)
            {
                // Log the event title and start time for debugging
                Console.WriteLine($"Event '{e.Title}' Start Time: {e.Start}");

                // Initialize location with event's location or set to "Unknown Location" if null
                var location = e.Location ?? "Unknown Location";
                double eventLatitude = 0, eventLongitude = 0;

                // Attempt to parse the location string into latitude and longitude
                if (TryParseLocation(location, out eventLatitude, out eventLongitude))
                {
                    // If parsing is successful, get the formatted address from coordinates
                    location = await _googleGeocodingService.GetAddressFromCoordinates(eventLatitude, eventLongitude);
                }
                else
                {
                    // If parsing fails, attempt to get coordinates using the geocoding service
                    var geoCoordinates = await _googleGeocodingService.GetCoordinatesAsync(location);
                    if (geoCoordinates.HasValue)
                    {
                        eventLatitude = geoCoordinates.Value.latitude;
                        eventLongitude = geoCoordinates.Value.longitude;
                    }
                }

                // Calculate the distance between the search location and the event location
                var distance = DistanceCalculator.CalculateDistance(searchLatitude, searchLongitude, eventLatitude, eventLongitude);

                // If the event is beyond the maximum allowed distance, skip it
                if (distance > (maxDistance ?? 100))
                {
                    continue; // Skip events beyond maxDistance
                }

                // Create a CombinedEvent object with the processed event details and add to the list
                processedApiEvents.Add(new CombinedEvent
                {
                    Title = e.Title ?? "Unknown Title",
                    Description = e.Description ?? "No description",
                    Location = location,
                    Start = e.Start,
                    Source = e.Source ?? "API",
                    Type = typeMapping.ContainsKey(e.Type?.ToLower()) ? typeMapping[e.Type.ToLower()] : e.Type,
                    CurrencyCode = e.CurrencyCode ?? "N/A",
                    Amount = e.Amount.HasValue ? (decimal)e.Amount.Value : 0,
                    URL = e.URL ?? "N/A",
                    Distance = distance
                });
            }

            // List to store processed host-managed events
            var processedHostEvents = new List<CombinedEvent>();

            // Iterate through each host-managed event fetched from the database
            foreach (var he in hostEvents)
            {
                double hostLatitude = 0, hostLongitude = 0;

                // Attempt to parse the host event's location into latitude and longitude
                if (!TryParseLocation(he.Location, out hostLatitude, out hostLongitude))
                {
                    // If parsing fails, get coordinates using the geocoding service
                    var geoCoordinates = await _googleGeocodingService.GetCoordinatesAsync(he.Location);
                    if (geoCoordinates.HasValue)
                    {
                        hostLatitude = geoCoordinates.Value.latitude;
                        hostLongitude = geoCoordinates.Value.longitude;
                    }
                }

                // Calculate the distance between the search location and the host event location
                var distance = DistanceCalculator.CalculateDistance(searchLatitude, searchLongitude, hostLatitude, hostLongitude);

                // If the event is beyond the maximum allowed distance, skip it
                if (distance > (maxDistance ?? 100))
                {
                    continue; // Skip events beyond maxDistance
                }

                // Create a CombinedEvent object with the processed host event details and add to the list
                processedHostEvents.Add(new CombinedEvent
                {
                    Title = he.Title ?? "Unknown Title",
                    Description = he.Description ?? "No description",
                    Location = he.Location ?? "Unknown Location",
                    Start = he.Start,
                    Source = "Host",
                    Type = typeMapping.ContainsKey(he.Type?.ToLower()) ? typeMapping[he.Type.ToLower()] : he.Type,
                    CurrencyCode = he.CurrencyCode ?? "N/A",
                    Amount = he.Amount ?? 0,
                    URL = he.URL ?? "N/A",
                    Distance = distance
                });
            }

            // Combine API-fetched events and host-managed events into a single list
            var combinedEvents = processedApiEvents.Concat(processedHostEvents).ToList();

            // Filter events by event type if provided
            if (!string.IsNullOrEmpty(eventType))
            {
                combinedEvents = combinedEvents
                    .Where(e => e.Type.Equals(eventType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filter events by maximum price if provided
            if (maxPrice.HasValue)
            {
                combinedEvents = combinedEvents
                    .Where(e => e.Amount.HasValue && e.Amount.Value <= (decimal)maxPrice.Value)
                    .ToList();
            }

            // Filter events that start after the specified start date-time if provided
            if (startDateTime.HasValue)
            {
                combinedEvents = combinedEvents
                    .Where(e => e.Start.HasValue && e.Start.Value >= startDateTime.Value)
                    .ToList();
            }

            // Filter events that start before the specified end date-time if provided
            if (endDateTime.HasValue)
            {
                combinedEvents = combinedEvents
                    .Where(e => e.Start.HasValue && e.Start.Value <= endDateTime.Value)
                    .ToList();
            }

            // Validate that the end date-time is not earlier than the start date-time
            if (startDateTime.HasValue && endDateTime.HasValue && endDateTime.Value < startDateTime.Value)
            {
                return BadRequest("End DateTime cannot be earlier than Start DateTime.");
            }

            // Return the combined and filtered list of events with a success message
            return Ok(new { Message = "Events fetched successfully.", Events = combinedEvents });
        }
        catch (Exception ex)
        {
            // Handle any unexpected errors and return a 500 Internal Server Error with the exception message
            return StatusCode(500, $"An error occurred while fetching events: {ex.Message}");
        }
    }

    // Helper method to attempt parsing a location string into latitude and longitude
    private bool TryParseLocation(string location, out double latitude, out double longitude)
    {
        latitude = 0;
        longitude = 0;

        // Check if the location string is null, empty, or does not contain a comma
        if (string.IsNullOrEmpty(location) || !location.Contains(",")) return false;

        // Split the location string by comma to extract latitude and longitude
        var parts = location.Split(',');
        // Try to parse the first part as latitude and the second part as longitude
        return double.TryParse(parts[0].Trim(), out latitude) && double.TryParse(parts[1].Trim(), out longitude);
    }
}
