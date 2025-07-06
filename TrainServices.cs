
using System.Text.Json.Serialization;

namespace TrainChecker;

public class HuxleyResponse
{
    [JsonPropertyName("trainServices")]
    public TrainService[]? TrainServices { get; set; }

    [JsonPropertyName("crs")]
    public string? Crs { get; set; }
}

public class TrainService
{
    [JsonPropertyName("std")]
    public string? ScheduledTimeOfDeparture { get; set; }

    [JsonPropertyName("etd")]
    public string? EstimatedTimeOfDeparture { get; set; }

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }
}
