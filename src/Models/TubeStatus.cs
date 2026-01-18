using System.Text.Json.Serialization;

namespace TrainChecker.Models;

public class TubeStatus
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("lineStatuses")]
    public List<LineStatus> LineStatuses { get; set; } = new();
}

public class LineStatus
{
    [JsonPropertyName("statusSeverity")]
    public int StatusSeverity { get; set; }

    [JsonPropertyName("statusSeverityDescription")]
    public string StatusSeverityDescription { get; set; } = string.Empty;
}