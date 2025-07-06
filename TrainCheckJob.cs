using Microsoft.Extensions.Options;
using Quartz;

namespace TrainChecker;

[DisallowConcurrentExecution]
public class TrainCheckJob : IJob
{
    private readonly ILogger<TrainCheckJob> _logger;
    private readonly NationalRailService _nationalRailService;
    private readonly TrainCheckerOptions _options;

    public TrainCheckJob(ILogger<TrainCheckJob> logger, NationalRailService nationalRailService, IOptions<TrainCheckerOptions> options)
    {
        _logger = logger;
        _nationalRailService = nationalRailService;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
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
}
