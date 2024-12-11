// Model that establishes the fields of a DTO for use by the combined events controller
namespace ventaura_backend.Models
{
    public class CombinedEvent
    {
        // The title or name of the event
        public string Title { get; set; }

        // A detailed description of the event
        public string Description { get; set; }

        // The location of the event, either as a human-readable address or coordinates
        public string Location { get; set; }

        // The start date and time of the event; nullable to allow events without a specified start time
        public DateTime? Start { get; set; }

        // Indicates the source of the event data, such as "API" or "Host"
        public string Source { get; set; }

        // The category or type of the event, standardized for consistency (e.g., "Music", "Theater")
        public string Type { get; set; }

        // The currency code for any associated costs, using standard codes like "USD" or "EUR"
        public string CurrencyCode { get; set; }

        // The monetary amount related to the event, such as ticket price; nullable to accommodate free events
        public decimal? Amount { get; set; }

        // A URL linking to more information about the event or where tickets can be purchased
        public string URL { get; set; }

        // The distance from the user's search location to the event location, measured in kilometers
        public double Distance { get; set; }
    }
}
