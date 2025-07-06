using Microsoft.Extensions.Options;

namespace TrainChecker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly NationalRailService _nationalRailService;
    private readonly TrainCheckerOptions _options;

    public Worker(ILogger<Worker> logger, NationalRailService nationalRailService, IOptions<TrainCheckerOptions> options)
    {
        _logger = logger;
        _nationalRailService = nationalRailService;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (IsWeekday() && IsTimeToCheck())
            {
                try
                {
                    var huxleyResponse = await _nationalRailService.GetTrainStatusAsync(_options.Time);
                    if (huxleyResponse?.TrainServices != null)
                    {
                        foreach (var trainService in huxleyResponse.TrainServices)
                        {
                            var origin = trainService.Origin?.FirstOrDefault()?.LocationName ?? "Unknown";
                            var destination = trainService.Destination?.FirstOrDefault()?.LocationName ?? "Unknown";
                            _logger.LogInformation("Train from {origin} to {destination} at {std} is {etd}", origin, destination, trainService.ScheduledTimeOfDeparture, trainService.EstimatedTimeOfDeparture);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking train status.");
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private bool IsWeekday()
    {
        var today = DateTime.Now.DayOfWeek;
        return today >= DayOfWeek.Monday && today <= DayOfWeek.Friday;
    }

    private bool IsTimeToCheck()
    {
        var now = DateTime.Now;
        return now.Hour == 7 && now.Minute == 30;
    }
}
