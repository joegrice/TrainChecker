using Quartz;
using TrainChecker.Services.Train;

namespace TrainChecker.Jobs;

public class TrainCheckJob(
    ILogger<TrainCheckJob> logger,
    ITrainService trainService)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("TrainCheckJob {JobName} started.", context.JobDetail.Key.Name);
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