namespace TrainChecker.Configuration;

public class CorsOptions
{
    public const string Cors = "Cors";
    
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}