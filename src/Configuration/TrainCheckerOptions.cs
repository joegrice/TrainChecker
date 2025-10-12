namespace TrainChecker.Configuration;

public class TrainCheckerOptions
{
    public const string TrainChecker = "TrainChecker";

    public string DepartureStation { get; set; } = string.Empty;
    public string ArrivalStation { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string BaseAddress { get; set; } = "";
    public string Version { get; set; } = string.Empty;
}
