using System.Text.Json.Serialization;

namespace TrainChecker.Models;

public class TubeStatus
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("lineStatuses")]
    public List<LineStatus> LineStatuses { get; init; } = [];
}

public class LineStatus
{
    [JsonPropertyName("statusSeverityDescription")]
    public string StatusSeverityDescription { get; init; } = string.Empty;
}