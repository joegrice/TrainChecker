using Microsoft.Extensions.Options;
using Quartz;
using TrainChecker.Configuration;
using TrainChecker.Services.Train;

namespace TrainChecker.Jobs;

[DisallowConcurrentExecution]
public class TrainCheckJob(
    ILogger<TrainCheckJob> logger,
    ITrainService trainService,
    IOptions<TrainCheckerOptions> options)
    : IJob
{
    private readonly TrainCheckerOptions _options = options.Value;

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var departureStation = context.JobDetail.JobDataMap.GetString("DepartureStation");
            var arrivalStation = context.JobDetail.JobDataMap.GetString("ArrivalStation");

            if (string.IsNullOrEmpty(departureStation) || string.IsNullOrEmpty(arrivalStation))
            {
                logger.LogError("Departure or arrival station not provided for TrainCheckJob.");
                return;
            }

            await trainService.GetAndSendTrainStatusAsync(departureStation, arrivalStation);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking train status.");
        }
    }
}