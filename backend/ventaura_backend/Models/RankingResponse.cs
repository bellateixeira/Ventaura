// Models/RankingResponse.cs
namespace ventaura_backend.Models
{
    public class RankingResponse
    {
        // Indicates whether the ranking operation was successful
        public bool Success { get; set; }

        // Provides a message related to the ranking operation, such as error details or success confirmation
        public string Message { get; set; }

        // The number of events that were processed during the ranking operation
        public int EventsProcessed { get; set; }

        // The number of events that were removed or excluded during the ranking operation
        public int EventsRemoved { get; set; }
    }
}

