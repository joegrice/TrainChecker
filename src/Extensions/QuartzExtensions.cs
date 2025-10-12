using Quartz;

namespace TrainChecker.Extensions;

public static class QuartzExtensions
{
    public static void AddJobAndTriggers<T>(this IServiceCollectionQuartzConfigurator q, string jobName, string departureStation, string arrivalStation, string[] cronSchedules) where T : IJob
    {
        var jobKey = new JobKey(jobName);
        q.AddJob<T>(opts => opts
            .WithIdentity(jobKey)
            .UsingJobData("DepartureStation", departureStation)
            .UsingJobData("ArrivalStation", arrivalStation));

        foreach (var cronSchedule in cronSchedules)
        {
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{jobName}-{cronSchedule}-trigger")
                .WithCronSchedule(cronSchedule));
        }
    }
}