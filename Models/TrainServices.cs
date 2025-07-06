using System.Text.Json.Serialization;

namespace TrainChecker.Models;

public class HuxleyResponse
{
    [JsonPropertyName("locationName")]
    public string? LocationName { get; set; }

    [JsonPropertyName("trainServices")]
    public TrainService[]? TrainServices { get; set; }

    [JsonPropertyName("crs")]
    public string? Crs { get; set; }
}

public class TrainService
{
    [JsonPropertyName("origin")]
    public Location[]? Origin { get; set; }

    [JsonPropertyName("destination")]
    public Location[]? Destination { get; set; }

    [JsonPropertyName("std")]
    public string? ScheduledTimeOfDeparture { get; set; }

    [JsonPropertyName("etd")]
    public string? EstimatedTimeOfDeparture { get; set; }

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }
}

public class Location
{
    [JsonPropertyName("locationName")]
    public string? LocationName { get; set; }

    [JsonPropertyName("crs")]
    public string? Crs { get; set; }
}