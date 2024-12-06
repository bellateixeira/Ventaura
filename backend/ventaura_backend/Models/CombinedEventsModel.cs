// This file defines the CombinedEvent class, representing a unified event object that 
// consolidates both external API events and locally hosted events into a common model. 
// Each CombinedEvent instance includes essential event details, a potential start time, 
// an event source (internal or external), financial information (currency and amount), 
// as well as a URL for more information. Additionally, it can store a calculated distance 
// from a user's location, enabling location-based sorting or filtering of events.

public class CombinedEvent
{
    // The title or name of the event.
    public string Title { get; set; }

    // A descriptive summary or details about the event.
    public string Description { get; set; }

    // The event’s location. This may be a formatted address or latitude/longitude coordinates.
    public string Location { get; set; }

    // The start date and time of the event, if available.
    public DateTime? Start { get; set; }

    // Indicates the source of the event. For example, could be "Host" or "External API".
    public string Source { get; set; }

    // The type or category of the event (e.g., "Music", "Conference", "Meetup").
    public string Type { get; set; }

    // The three-letter ISO currency code associated with ticket prices or event costs.
    public string CurrencyCode { get; set; }

    // The amount associated with the event’s price or cost, if applicable.
    public decimal? Amount { get; set; }

    // A URL for more information about the event (e.g., a ticket purchase link or event website).
    public string URL { get; set; }

    // The distance from a given reference point (e.g., a user's location) to the event.
    // Typically measured in kilometers or miles, depending on implementation.
    public double? Distance { get; set; }
}
