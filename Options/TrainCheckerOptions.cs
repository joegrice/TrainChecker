namespace TrainChecker.Options;

public class TrainCheckerOptions
{
    public const string TrainChecker = "TrainChecker";

    public string DepartureStation { get; set; } = string.Empty;
    public string ArrivalStation { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}
