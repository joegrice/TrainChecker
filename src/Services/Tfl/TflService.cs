using System.Text.Json;
using Microsoft.Extensions.Options;
using TrainChecker.Configuration;
using TrainChecker.Models;

namespace TrainChecker.Services.Tfl;

public class TflService : ITflService
{
    private readonly HttpClient _httpClient;
    private readonly TubeCheckerOptions _options;
    private readonly ILogger<TflService> _logger;

    public TflService(HttpClient httpClient, IOptions<TubeCheckerOptions> options, ILogger<TflService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_options.BaseAddress);
    }

    public async Task<List<TubeStatus>> GetTubeStatusesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"/Line/Mode/tube/Status?app_key={_options.AppKey}&app_id={_options.AppId}");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var statuses = JsonSerializer.Deserialize<List<TubeStatus>>(content);
            
            return statuses ?? new List<TubeStatus>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tube statuses");
            return new List<TubeStatus>();
        }
    }
}