using Microsoft.Extensions.Options;
using Quartz;
using TrainChecker.Options;
using TrainChecker.Services.Train;

namespace TrainChecker.Jobs;

[DisallowConcurrentExecution]
public class TrainCheckJob : IJob
{
    private readonly ILogger<TrainCheckJob> _logger;
    private readonly ITrainService _trainService;
    private readonly TrainCheckerOptions _options;

    public TrainCheckJob(ILogger<TrainCheckJob> logger, ITrainService trainService, IOptions<TrainCheckerOptions> options)
    {
        _logger = logger;
        _trainService = trainService;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await _trainService.GetAndSendTrainStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking train status.");
        }
    }
}