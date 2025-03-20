/* Retrieves and stores user-specific content in memory, automatically cleared on logout. 
This file implements the CombinedEventsController for the Ventaura application, enabling 
user-specific content management through session-based database tables. It handles fetching 
events from integrated APIs (Amadeus and Ticketmaster), dynamically creating and 
populating temporary tables for user sessions, and ensuring cleanup upon logout. This approach 
optimizes resource usage, ensures privacy, and provides a seamless experience for users accessing 
personalized event recommendations. */

using Microsoft.AspNetCore.Mvc;
using ventaura_backend.Data;
using Microsoft.EntityFrameworkCore;
using ventaura_backend.Utils;
using ventaura_backend.Models;
using ventaura_backend.Services;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using Npgsql;
using System.Collections.Generic;

namespace ventaura_backend.Controllers
{
    // Indicates that this class is an API controller and 
    // the route for its endpoints starts with "api/combined-events".
    [ApiController]
    [Route("api/combined-events")]
    public class CombinedEventsController : ControllerBase
    {
        // Fields for the combined API service, database context, and configuration.
        private readonly CombinedAPIService _combinedApiService;
        private readonly DatabaseContext _dbContext;
        private readonly IConfiguration _configuration;

        // Constructor that receives and stores the required services and configuration.
        // _combinedApiService: used for fetching external and internal event data.
        // _dbContext: used for accessing and manipulating database entities.
        // _configuration: used to retrieve application settings (e.g., API keys).
        public CombinedEventsController(CombinedAPIService combinedApiService, DatabaseContext dbContext, IConfiguration configuration)
        {
            _combinedApiService = combinedApiService;
            _dbContext = dbContext;
            _configuration = configuration;
        }

        // GET endpoint: Fetches combined events (external and host events),
        // stores them in a CSV file, and returns information about the processed events.
        [HttpGet("fetch")]
        public async Task<IActionResult> FetchCombinedEvents([FromQuery] int userId)
        {
            try
            {
                // Attempt to find the user by ID in the database.
                // The user must have valid location data (latitude & longitude).
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null || user.Latitude == null || user.Longitude == null)
                {
                    Console.WriteLine($"User with ID {userId} not found or location data is missing.");
                    return BadRequest("User not found or location is missing.");
                }

                Console.WriteLine($"Location successfully extracted for userId: {userId}.");

                // Fetch events from external APIs based on the user's location via the CombinedAPIService.
                Console.WriteLine($"Fetching events for userId {userId}...");
                var apiEvents = await _combinedApiService.FetchEventsAsync(user.Latitude.Value, user.Longitude.Value, userId);

                // Retrieve all host events from the database for processing.
                Console.WriteLine($"Fetching host events for userId {userId}...");
                var hostEvents = await _dbContext.HostEvents.ToListAsync();

                // Initialize a geocoding service to resolve event locations if needed.
                var geocodingService = new GoogleGeocodingService(new HttpClient(), _configuration);

                // This list will hold host events processed with proper coordinates and distances.
                var processedHostEvents = new List<CombinedEvent>();

                // Process each host event: If it doesn't have coordinates, attempt geocoding.
                foreach (var he in hostEvents)
                {
                    double latitude, longitude;
                    
                    // First, check if we already have coordinates in the database
                    if (he.Latitude.HasValue && he.Longitude.HasValue)
                    {
                        // Use the coordinates stored in the database
                        latitude = (double)he.Latitude.Value;
                        longitude = (double)he.Longitude.Value;
                    }
                    // Only try to parse or geocode if we don't have coordinates
                    else if (!TryParseLocation(he.Location, out latitude, out longitude))
                    {
                        var geocodeResult = await geocodingService.GetCoordinatesAsync(he.Location);
                        if (geocodeResult != null)
                        {
                            latitude = geocodeResult.Value.latitude;
                            longitude = geocodeResult.Value.longitude;
                        }
                        else
                        {
                            Console.WriteLine($"Skipping host event '{he.Title}' due to invalid location.");
                            continue;
                        }
                    }

                    // Calculate the distance from the user's location to the event.
                    var distance = DistanceCalculator.CalculateDistance(
                        user.Latitude.Value,
                        user.Longitude.Value,
                        latitude,
                        longitude
                    );

                    // Create a CombinedEvent object for the host event, including calculated distance.
                    processedHostEvents.Add(new CombinedEvent
                    {
                        Title = he.Title,
                        Description = he.Description,
                        Location = he.Location,
                        Start = he.Start,
                        Source = he.Source,
                        Type = he.Type,
                        CurrencyCode = he.CurrencyCode,
                        Amount = (decimal?)he.Amount,
                        URL = he.URL,
                        Distance = distance
                    });
                }

                // Convert the external API events into CombinedEvent objects for uniformity.
                var allEvents = apiEvents.Select(e => new CombinedEvent
                {
                    Title = e.Title,
                    Description = e.Description,
                    Location = e.Location,
                    Start = e.Start,
                    Source = e.Source,
                    Type = e.Type,
                    CurrencyCode = e.CurrencyCode,
                    Amount = (decimal?)e.Amount,
                    URL = e.URL,
                    Distance = e.Distance
                }).ToList();

                // Combine the external and host events into a single list.
                allEvents.AddRange(processedHostEvents);

                // If there are events, write them to a CSV file.
                if (allEvents.Any())
                {
                    Console.WriteLine($"Preparing data for {allEvents.Count} events to generate CSV.");

                    // Construct the CSV file path for this user.
                    var csvFilePath = Path.Combine("CsvFiles", $"{userId}.csv");
                    var directory = Path.GetDirectoryName(csvFilePath);

                    // Ensure the directory exists, create if necessary.
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Write the events to the CSV file.
                    using (var writer = new StreamWriter(csvFilePath, false, Encoding.UTF8))
                    {
                        // Write the header line.
                        await writer.WriteLineAsync("contentId,title,description,location,start,source,type,currencyCode,amount,url,distance");
                        
                        int contentIdCounter = 1;
                        // Write each event as a line in the CSV.
                        foreach (var e in allEvents)
                        {
                            await writer.WriteLineAsync($"{contentIdCounter}," +
                                                        $"{e.Title}," +
                                                        $"{e.Description}," +
                                                        $"{e.Location}," +
                                                        $"{e.Start?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""}," +
                                                        $"{e.Source}," +
                                                        $"{e.Type}," +
                                                        $"{e.CurrencyCode}," +
                                                        $"{e.Amount?.ToString() ?? ""}," +
                                                        $"{e.URL}," +
                                                        $"{e.Distance}");
                            contentIdCounter++;
                        }
                    }

                    Console.WriteLine($"CSV file created at {csvFilePath}.");

                    // Return a response including the file path and total events processed.
                    return Ok(new
                    {
                        Message = "Events processed successfully and CSV created.",
                        CsvPath = csvFilePath,
                        TotalEvents = allEvents.Count
                    });
                }
                else
                {
                    // If no events were found or processed, return a message indicating so.
                    Console.WriteLine("No events to process.");
                    return Ok(new { Message = "No events available to process.", TotalEvents = 0 });
                }
            }
            catch (Exception ex)
            {
                // Log any unexpected errors and return a 500 status code.
                Console.WriteLine($"Error in FetchCombinedEvents: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching events.");
            }
        }

        // Attempts to parse a location string into latitude and longitude.
        // Returns true if successful; otherwise false.
        private bool TryParseLocation(string location, out double latitude, out double longitude)
        {
            latitude = 0;
            longitude = 0;

            // Check if the location string is valid and contains a comma.
            if (string.IsNullOrEmpty(location) || !location.Contains(","))
                return false;

            // Split by comma and try to parse the parts as doubles.
            var parts = location.Split(',');
            return double.TryParse(parts[0].Trim(), out latitude) && double.TryParse(parts[1].Trim(), out longitude);
        }

        // POST endpoint: Logs the user out and deletes their associated CSV file.
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] int userId)
        {
            try
            {
                // Retrieve the user by ID.
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    Console.WriteLine($"User with ID {userId} does not exist.");
                    return BadRequest(new { Message = "User not found or not logged in." });
                }

                // Construct the CSV file path for this user.
                var csvFilePath = Path.Combine("CsvFiles", $"{userId}.csv");

                // If the CSV file exists, attempt to delete it.
                if (System.IO.File.Exists(csvFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(csvFilePath);
                        Console.WriteLine($"Deleted CSV file: {csvFilePath}");
                    }
                    catch (Exception ex)
                    {
                        // If file deletion fails, log the error and return a server error response.
                        Console.WriteLine($"Error deleting CSV file {csvFilePath}: {ex.Message}");
                        return StatusCode(500, new { Message = "Error deleting CSV file.", Details = ex.Message });
                    }
                }
                else
                {
                    Console.WriteLine($"CSV file {csvFilePath} does not exist.");
                }

                // Mark the user as logged out in the database and save changes.
                user.IsLoggedIn = false;
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"User {user.Email} logged out successfully.");

                // Return a success message.
                return Ok(new { Message = "User logged out successfully and CSV file deleted." });
            }
            catch (Exception ex)
            {
                // Log any unexpected errors and return a server error response.
                Console.WriteLine($"Error while logging out user {userId}: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred while logging out.", Details = ex.Message });
            }
        }

        // POST endpoint: Creates a new host event in the database for a specific host user.
        [HttpPost("create-host-event")]
        public async Task<IActionResult> CreateHostEvent([FromBody] HostEvent newEvent)
        {
            try
            {
                Console.WriteLine("Start CreateHostEvent process...");

                // Validate the input model. If invalid, return a BadRequest response.
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("Validation failed: Missing required fields.");
                    return BadRequest(ModelState);
                }

                // Use the execution strategy to handle transient failures and retries.
                var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
                await executionStrategy.ExecuteAsync(async () =>
                {
                    // Begin a database transaction.
                    using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                    {
                        // Check if an event with the same title already exists for the given host.
                        Console.WriteLine($"Checking if event '{newEvent.Title}' already exists for host {newEvent.HostUserId}...");
                        var existingEvent = await _dbContext.HostEvents.AsNoTracking()
                            .FirstOrDefaultAsync(e => e.Title == newEvent.Title && e.HostUserId == newEvent.HostUserId);

                        if (existingEvent != null)
                        {
                            // If a duplicate event exists, throw an exception to handle it gracefully.
                            throw new InvalidOperationException($"An event with the title '{newEvent.Title}' already exists for this host.");
                        }

                        // Create a new HostEvent entity to add to the database.
                        var hostEvent = new HostEvent
                        {
                            Title = newEvent.Title,
                            Description = newEvent.Description,
                            Location = newEvent.Location,
                            Start = newEvent.Start,
                            Source = "Host",
                            Type = newEvent.Type,
                            CurrencyCode = newEvent.CurrencyCode,
                            Amount = newEvent.Amount,
                            URL = newEvent.URL,
                            CreatedAt = DateTime.UtcNow,
                            HostUserId = newEvent.HostUserId
                        };

                        Console.WriteLine("Adding host event to database...");
                        await _dbContext.HostEvents.AddAsync(hostEvent);
                        await _dbContext.SaveChangesAsync();

                        // Commit the transaction after successful insertion.
                        await transaction.CommitAsync();

                        Console.WriteLine($"Host event '{hostEvent.Title}' successfully created in database.");
                    }
                });

                // Return a success message if everything was completed without exceptions.
                return Ok(new
                {
                    Message = "Host event created successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                // Handle validation errors (such as duplicate titles).
                Console.WriteLine($"Validation error: {ex.Message}");
                return Conflict(ex.Message);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                // Handle duplicate key errors at the database level.
                Console.WriteLine($"Duplicate event error: {pgEx.MessageText}");
                return Conflict("An event with this title already exists for this host.");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors.
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}