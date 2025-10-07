using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TrainChecker.Configuration;
using TrainChecker.Models;

namespace TrainChecker.Services.NationalRail;

public class NationalRailService : INationalRailService
{
    private readonly HttpClient _httpClient;
    private readonly TrainCheckerOptions _options;
    private readonly ILogger<NationalRailService> _logger;

    public NationalRailService(HttpClient httpClient, IOptions<TrainCheckerOptions> options, ILogger<NationalRailService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<HuxleyResponse?> GetTrainStatusAsync(string time, string departureStation, string arrivalStation)
    {
        var requestUri = $"/departures/{departureStation}/to/{arrivalStation}?accessToken={_options.ApiKey}&time={time}&timeWindow=60&expand=true";
        var redactedRequestUri = System.Text.RegularExpressions.Regex.Replace(requestUri, "accessToken=[^&]*", "accessToken=********");
        _logger.LogInformation("Requesting train status from: {BaseAddress}{RequestUri}", _httpClient.BaseAddress, redactedRequestUri);
        
        var response = await _httpClient.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error from National Rail API: {StatusCode} - {ReasonPhrase}. Content: {ErrorContent}", response.StatusCode, response.ReasonPhrase, errorContent);
        }
        
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<HuxleyResponse>(content);
    }
}
