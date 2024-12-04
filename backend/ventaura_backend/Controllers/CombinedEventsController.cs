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
        
    // Marks this class as an API controller and sets the route prefix for all endpoints.
    [ApiController]
    [Route("api/combined-events")]
    public class CombinedEventsController : ControllerBase
    {
        // Service for fetching events from combined APIs and the database context for accessing the database.
        private readonly CombinedAPIService _combinedApiService;
        private readonly DatabaseContext _dbContext;
        private readonly IConfiguration _configuration;


        // Constructor to inject dependencies for the API service and database context.
        public CombinedEventsController(CombinedAPIService combinedApiService, DatabaseContext dbContext, IConfiguration configuration)
        {
            _combinedApiService = combinedApiService;
            _dbContext = dbContext;
            _configuration = configuration;

        }

        // Endpoint to fetch and store user-specific event data in a temporary table.
        [HttpGet("fetch")]
        public async Task<IActionResult> FetchCombinedEvents([FromQuery] int userId)
        {
            try
            {
                // Retrieve the user and validate their location data
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null || user.Latitude == null || user.Longitude == null)
                {
                    Console.WriteLine($"User with ID {userId} not found or location data is missing.");
                    return BadRequest("User not found or location is missing.");
                }

                Console.WriteLine($"Location successfully extracted for userId: {userId}.");

                // Fetch events from the combined API service
                Console.WriteLine($"Fetching events for userId {userId}...");
                var apiEvents = await _combinedApiService.FetchEventsAsync(user.Latitude.Value, user.Longitude.Value, userId);

                // Fetch host events from the database
                Console.WriteLine($"Fetching host events for userId {userId}...");
                var hostEvents = await _dbContext.HostEvents.ToListAsync();

                var geocodingService = new GoogleGeocodingService(new HttpClient(), _configuration);

                var processedHostEvents = new List<CombinedEvent>();

                foreach (var he in hostEvents)
                {
                    double latitude, longitude;

                    // Try parsing the location as coordinates or geocode it
                    if (!TryParseLocation(he.Location, out latitude, out longitude))
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
                            continue; // Skip this event if location can't be resolved
                        }
                    }

                    var distance = DistanceCalculator.CalculateDistance(
                        user.Latitude.Value,
                        user.Longitude.Value,
                        latitude,
                        longitude
                    );

                    processedHostEvents.Add(new CombinedEvent
                    {
                        Title = he.Title,
                        Description = he.Description,
                        Location = he.Location,
                        Start = he.Start, // Ensure this is the property
                        Source = he.Source,
                        Type = he.Type,
                        CurrencyCode = he.CurrencyCode,
                        Amount = (decimal?)he.Amount, // Explicit conversion
                        URL = he.URL,
                        Distance = distance // Correct usage
                    });
                }

                // Combine API and processed host events
                var allEvents = apiEvents.Select(e => new CombinedEvent
                {
                    Title = e.Title,
                    Description = e.Description,
                    Location = e.Location,
                    Start = e.Start,
                    Source = e.Source,
                    Type = e.Type,
                    CurrencyCode = e.CurrencyCode,
                    Amount = (decimal?)e.Amount, // Explicit conversion
                    URL = e.URL,
                    Distance = e.Distance
                }).ToList();

                allEvents.AddRange(processedHostEvents);

                // Generate CSV
                if (allEvents.Any())
                {
                    Console.WriteLine($"Preparing data for {allEvents.Count} events to generate CSV.");

                    var csvFilePath = Path.Combine("CsvFiles", $"{userId}.csv");
                    var directory = Path.GetDirectoryName(csvFilePath);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    using (var writer = new StreamWriter(csvFilePath, false, Encoding.UTF8))
                    {
                        await writer.WriteLineAsync("contentId,title,description,location,start,source,type,currencyCode,amount,url,distance");
                        int contentIdCounter = 1;

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

                    return Ok(new
                    {
                        Message = "Events processed successfully and CSV created.",
                        CsvPath = csvFilePath,
                        TotalEvents = allEvents.Count
                    });
                }
                else
                {
                    Console.WriteLine("No events to process.");
                    return Ok(new { Message = "No events available to process.", TotalEvents = 0 });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FetchCombinedEvents: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching events.");
            }
        }

        private bool TryParseLocation(string location, out double latitude, out double longitude)
        {
            latitude = 0;
            longitude = 0;
            if (string.IsNullOrEmpty(location) || !location.Contains(","))
                return false;

            var parts = location.Split(',');
            return double.TryParse(parts[0].Trim(), out latitude) && double.TryParse(parts[1].Trim(), out longitude);
        }

        // Endpoint to log out a user and delete their associated CSV file.
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] int userId)
        {
            try
            {
                // Check if the user exists in the database.
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    Console.WriteLine($"User with ID {userId} does not exist.");
                    return BadRequest(new { Message = "User not found or not logged in." });
                }

                // File path for the user's CSV file.
                var csvFilePath = Path.Combine("CsvFiles", $"{userId}.csv");

                // Attempt to delete the CSV file.
                if (System.IO.File.Exists(csvFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(csvFilePath);
                        Console.WriteLine($"Deleted CSV file: {csvFilePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting CSV file {csvFilePath}: {ex.Message}");
                        return StatusCode(500, new { Message = "Error deleting CSV file.", Details = ex.Message });
                    }
                }
                else
                {
                    Console.WriteLine($"CSV file {csvFilePath} does not exist.");
                }

                // Update the user's login status in the database.
                user.IsLoggedIn = false;
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"User {user.Email} logged out successfully.");

                return Ok(new { Message = "User logged out successfully and CSV file deleted." });
            }
            catch (Exception ex)
            {
                // Log and return general errors.
                Console.WriteLine($"Error while logging out user {userId}: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred while logging out.", Details = ex.Message });
            }
        }

        // Endpoint to add a host event to the database
        [HttpPost("create-host-event")]
        public async Task<IActionResult> CreateHostEvent([FromBody] HostEvent newEvent)
        {
            try
            {
                Console.WriteLine("Start CreateHostEvent process...");

                // Validate the input model
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("Validation failed: Missing required fields.");
                    return BadRequest(ModelState);
                }

                // Retry mechanism for transient failures
                var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
                await executionStrategy.ExecuteAsync(async () =>
                {
                    using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                    {
                        // Check for duplicate event title by the same host
                        Console.WriteLine($"Checking if event '{newEvent.Title}' already exists for host {newEvent.HostUserId}...");
                        var existingEvent = await _dbContext.HostEvents.AsNoTracking()
                            .FirstOrDefaultAsync(e => e.Title == newEvent.Title && e.HostUserId == newEvent.HostUserId);

                        if (existingEvent != null)
                        {
                            throw new InvalidOperationException($"An event with the title '{newEvent.Title}' already exists for this host.");
                        }

                        // Prepare new host event object
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
                        await transaction.CommitAsync();

                        Console.WriteLine($"Host event '{hostEvent.Title}' successfully created in database.");
                    }
                });

                return Ok(new
                {
                    Message = "Host event created successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Validation error: {ex.Message}");
                return Conflict(ex.Message);
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                Console.WriteLine($"Duplicate event error: {pgEx.MessageText}");
                return Conflict("An event with this title already exists for this host.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }


        // OLD Endpoint to fetch and store user-specific event data in a temporary table.
        /* [HttpGet("fetch")]
        public async Task<IActionResult> FetchCombinedEvents([FromQuery] int userId)
        {
            try
            {
                // Retrieve the user and validate their location data
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null || user.Latitude == null || user.Longitude == null)
                {
                    Console.WriteLine($"User with ID {userId} not found or location data is missing.");
                    return BadRequest("User not found or location is missing.");
                }

                Console.WriteLine($"Location successfully extracted for userId: {userId}.");

                // Use a single database connection for the operation
                using (var connection = _dbContext.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    Console.WriteLine($"Database connection opened successfully for userId: {userId}");
                    Console.WriteLine($"Connection state before table existence check: {connection.State}");

                    string tableExistsQuery = $@"
                        SELECT EXISTS (
                            SELECT 1
                            FROM pg_catalog.pg_tables
                            WHERE schemaname = 'public'
                            AND tablename = 'usercontent_{userId}'
                        );";

                    bool tableExists = false;

                    // Retry logic to handle connection or query issues
                    for (int attempt = 0; attempt < 3; attempt++)
                    {
                        try
                        {
                            // Check if the user-specific table exists
                            Console.WriteLine($"Checking for existing UserContent_{userId} table...");
                            using (var checkTableCommand = connection.CreateCommand())
                            {
                                checkTableCommand.CommandText = tableExistsQuery;
                                Console.WriteLine($"SQL Command: {checkTableCommand.CommandText}");

                                var result = await checkTableCommand.ExecuteScalarAsync();
                                Console.WriteLine($"Query executed successfully. Result: {result}");

                                tableExists = result != null && (bool)result;
                                Console.WriteLine($"Table existence check result: {tableExists}");
                                break; // Exit the loop if successful
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}");

                            // Close and reopen the connection if it's invalid
                            if (connection.State != System.Data.ConnectionState.Open)
                            {
                                Console.WriteLine("Reopening database connection...");
                                await connection.OpenAsync();
                                Console.WriteLine($"Connection reopened successfully. State: {connection.State}");
                            }

                            if (attempt == 2) throw; // Re-throw after the 3rd attempt
                        }
                    }

                    // Drop the table if it exists
                    if (tableExists)
                    {
                        Console.WriteLine($"UserContent_{userId} table exists. Dropping...");
                        using var dropTableCommand = connection.CreateCommand();
                        dropTableCommand.CommandText = $@"DROP TABLE usercontent_{userId};";
                        await dropTableCommand.ExecuteNonQueryAsync();
                        Console.WriteLine($"UserContent_{userId} table dropped.");
                    }

                    // Create the user-specific table
                    Console.WriteLine($"Creating UserContent_{userId} table...");
                    using (var createTableCommand = connection.CreateCommand())
                    {
                        createTableCommand.CommandText = $@"
                            CREATE TABLE usercontent_{userId} (
                                ContentId SERIAL PRIMARY KEY,
                                Title VARCHAR(255),
                                Description TEXT,
                                Location VARCHAR(255),
                                Start TIMESTAMPTZ,
                                Source VARCHAR(50),
                                Type VARCHAR(50),
                                CurrencyCode VARCHAR(10),
                                Amount VARCHAR(20),
                                URL TEXT,
                                Distance FLOAT
                            );";
                        Console.WriteLine($"SQL Command: {createTableCommand.CommandText}");
                        await createTableCommand.ExecuteNonQueryAsync();
                        Console.WriteLine($"UserContent_{userId} table created.");
                    }

                    // Fetch events from the combined API service
                    Console.WriteLine($"Fetching events for userId {userId}...");
                    var events = await _combinedApiService.FetchEventsAsync(user.Latitude.Value, user.Longitude.Value, userId);

                    // Batch insert events into the user-specific table
                    if (events.Any())
                    {
                        Console.WriteLine($"Preparing batch insert for {events.Count} events.");

                        var insertValues = events.Select(e =>
                        {
                            // Handle invalid or missing event locations
                            double eventLatitude, eventLongitude;
                            if (string.IsNullOrEmpty(e.Location) ||
                                !e.Location.Contains(",") ||
                                !double.TryParse(e.Location.Split(',')[0], out eventLatitude) ||
                                !double.TryParse(e.Location.Split(',')[1], out eventLongitude))
                            {
                                eventLatitude = user.Latitude.Value;
                                eventLongitude = user.Longitude.Value;
                            }

                            var distance = DistanceCalculator.CalculateDistance(
                                user.Latitude.Value,
                                user.Longitude.Value,
                                eventLatitude,
                                eventLongitude
                            );
                            e.Distance = (float)distance;

                            return $@"
                                ('{e.Title?.Replace("'", "''")}', 
                                '{e.Description?.Replace("'", "''")}', 
                                '{e.Location?.Replace("'", "''")}', 
                                '{e.Start?.ToString("yyyy-MM-dd HH:mm:ss") ?? "NULL"}', 
                                '{e.Source?.Replace("'", "''")}', 
                                '{e.Type?.Replace("'", "''")}', 
                                '{e.CurrencyCode?.Replace("'", "''")}', 
                                '{e.Amount?.ToString() ?? "NULL"}', 
                                '{e.URL?.Replace("'", "''")}', 
                                {e.Distance ?? 0})";
                        }).ToList();

                        using var batchInsertCommand = connection.CreateCommand();
                        batchInsertCommand.CommandText = $@"
                            INSERT INTO usercontent_{userId} 
                            (Title, Description, Location, Start, Source, Type, CurrencyCode, Amount, URL, Distance) 
                            VALUES {string.Join(",", insertValues)};";
                        await batchInsertCommand.ExecuteNonQueryAsync();
                        Console.WriteLine("Batch insert completed successfully.");
                    }
                    else
                    {
                        Console.WriteLine("No events to insert.");
                    }

                    return Ok(new
                    {
                        Message = "Events processed successfully.",
                        Table = $"usercontent_{userId}",
                        TotalEvents = events.Count,
                        ValidEvents = events.Count(e => !string.IsNullOrEmpty(e.Location)),
                        InvalidEvents = events.Count(e => string.IsNullOrEmpty(e.Location))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FetchCombinedEvents: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching events.");
            }
        }*/

        // OLD Endpoint to log out a user and clear their session-specific content.
        /* [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] int userId)
        {
            try
            {
                // Check if the user exists in the database.
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    Console.WriteLine($"User with ID {userId} does not exist.");
                    return BadRequest(new { Message = "User not found or not logged in." });
                }

                // Use a single DbConnection for direct SQL execution.
                using (var connection = _dbContext.Database.GetDbConnection())
                {
                    for (int attempt = 0; attempt < 3; attempt++)
                    {
                        try
                        {
                            if (connection.State != System.Data.ConnectionState.Open)
                            {
                                Console.WriteLine("Opening database connection...");
                                await connection.OpenAsync();
                                Console.WriteLine($"Connection opened successfully. State: {connection.State}");
                            }

                            // Drop the user-specific table if it exists.
                            using (var dropTableCommand = connection.CreateCommand())
                            {
                                dropTableCommand.CommandText = $@"DROP TABLE IF EXISTS usercontent_{userId};";
                                Console.WriteLine($"Executing: {dropTableCommand.CommandText}");
                                await dropTableCommand.ExecuteNonQueryAsync();
                                Console.WriteLine($"UserContent_{userId} table dropped if it existed.");
                            }

                            break; // Exit the retry loop if successful
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Attempt {attempt + 1} failed: {ex.Message}");

                            if (connection.State != System.Data.ConnectionState.Open)
                            {
                                Console.WriteLine("Reopening database connection...");
                                await connection.CloseAsync();
                                await connection.OpenAsync();
                                Console.WriteLine($"Connection reopened successfully. State: {connection.State}");
                            }

                            if (attempt == 2) throw; // Re-throw after 3rd attempt
                        }
                    }
                }

                // Update the user's login status in the database.
                user.IsLoggedIn = false;
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"User {user.Email} logged out successfully.");

                return Ok(new { Message = "User logged out successfully." });
            }
            catch (Npgsql.PostgresException ex)
            {
                // Log and return database-specific errors.
                Console.WriteLine($"Postgres error while logging out user {userId}: {ex.Message}");
                return StatusCode(500, new { Message = "Database error.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                // Log and return general errors.
                Console.WriteLine($"Error while logging out user {userId}: {ex.Message}");
                return StatusCode(500, new { Message = "An error occurred while logging out.", Details = ex.Message });
            }
        }*/

    }
}
