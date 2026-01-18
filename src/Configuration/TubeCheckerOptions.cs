namespace TrainChecker.Configuration;

public class TubeCheckerOptions
{
    public const string TubeChecker = "TubeChecker";

    public string AppKey { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;
    public string BaseAddress { get; set; } = string.Empty;
}