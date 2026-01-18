using Microsoft.Extensions.Options;
using TrainChecker.Configuration;
using TrainChecker.Models;

namespace TrainChecker.Services.Tfl;

public interface ITflService
{
    Task<List<TubeStatus>> GetTubeStatusesAsync();
}

public class TflService(ILogger<TflService> logger, IOptions<TubeCheckerOptions> options, HttpClient httpClient)
    : ITflService
{
    private readonly TubeCheckerOptions _options = options.Value;

    public async Task<List<TubeStatus>> GetTubeStatusesAsync()
    {
        try
        {
            var response = await httpClient.GetAsync($"/Line/Mode/tube/Status?app_key={_options.AppKey}&app_id={_options.AppId}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("Error from TFL API: {StatusCode} - {ReasonPhrase}. Content: {ErrorContent}", response.StatusCode, response.ReasonPhrase, errorContent);
            }
            
            response.EnsureSuccessStatusCode();
            
            var statuses = await response.Content.ReadFromJsonAsync<List<TubeStatus>>();
            
            return statuses ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tube statuses");
            return [];
        }
    }
}