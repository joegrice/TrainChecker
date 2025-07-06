using Microsoft.Extensions.Options;
using Quartz;
using TrainChecker.Configuration;
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
            var departureStation = context.JobDetail.JobDataMap.GetString("DepartureStation");
            var arrivalStation = context.JobDetail.JobDataMap.GetString("ArrivalStation");

            if (string.IsNullOrEmpty(departureStation) || string.IsNullOrEmpty(arrivalStation))
            {
                _logger.LogError("Departure or arrival station not provided for TrainCheckJob.");
                return;
            }

            await _trainService.GetAndSendTrainStatusAsync(departureStation, arrivalStation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking train status.");
        }
    }
}