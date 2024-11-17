// using Microsoft.AspNetCore.Mvc;
// using combinedapi.Data;
// using combinedapi.Services;
// using Microsoft.EntityFrameworkCore;     
// using Npgsql;  

/* NB! The portions of code that are commented out are because they are not needed for the FOR-YOU view since
we are working with the coordinates directly.
This controller becomes useful in the GLOBAL view if necessary */
/* namespace combinedapi.Controllers
{
    [ApiController]
    [Route("api/amadeus")]
    public class AmadeusController : ControllerBase
    {
        private readonly AmadeusService _amadeusService;
        private readonly GoogleGeocodingService _googleGeocodingService;
        private readonly DatabaseContext _dbContext;

        public AmadeusController(AmadeusService amadeusService, GoogleGeocodingService googleGeocodingService, DatabaseContext dbContext)
        {
            _amadeusService = amadeusService;
            _googleGeocodingService = googleGeocodingService;
            _dbContext = dbContext;
        }

        // Endpoint to fetch and store Amadeus events based on user location
        [HttpGet("fetch")]
        public async Task<IActionResult> FetchAndStoreAmadeusEvents([FromQuery] int userId)
        {

            // Retrieve the user from the database
            var user = await _dbContext.Users.FindAsync(userId);
            // if (user == null || string.isNullorEmpty(user.location)) --> previously when storing a location string instead of coordinates directly
            if (user == null || user.Latitude == null || user.Longitude == null)
                return BadRequest("User not found or location is missing.");

            // Convert the user's location to coordinates using Google Geocoding
            // var coordinates = await _googleGeocodingService.GetCoordinatesAsync(user.Location);
            /* if (!coordinates.HasValue)
                return BadRequest("Could not find coordinates for the user's location."); */
            
            // Console.WriteLine($"Coordinates for location {user.Location}: Latitude={coordinates.Value.latitude}, Longitude={coordinates.Value.longitude}");

            // Fetch events from Amadeus based on coordinates
            // var events = await _amadeusService.FetchAmadeusEventsAsync(coordinates.Value.latitude, coordinates.Value.longitude); --> previous 
            // var events = await _amadeusService.FetchAmadeusEventsAsync(user.Latitude, user.Longitude);

            // Only add events that don't already exist in the database based on Title, Location, and Source
            // This checks that we don't add duplicate events so all events are unique in the database table per user
            /* var newEvents = events.Where(e => !_dbContext.Content
                .Any(c => c.Title == e.Title && c.Location == e.Location && c.Source == e.Source))
                .ToList();

            int addedCount = 0;

            foreach (var newEvent in newEvents)
            {
                try
                {
                    _dbContext.Content.Add(newEvent);
                    await _dbContext.SaveChangesAsync();
                    addedCount++;
                }
                catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    // Log the duplicate error and skip this entry
                    Console.WriteLine($"Duplicate entry found for event {newEvent.Title} at {newEvent.Location}. Skipping...");
                }
            }

            Console.WriteLine($"{addedCount} new Amadeus events added to the content table.");
            return Ok(new { Message = "Amadeus events processed.", AddedCount = addedCount });
        }
    }
} */