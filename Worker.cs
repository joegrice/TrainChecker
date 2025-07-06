namespace TrainChecker;

public class Worker(ILogger<Worker> logger, NationalRailService nationalRailService)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (IsWeekday() && IsTimeToCheck())
            {
                try
                {
                    var status = await nationalRailService.GetTrainStatusAsync();
                    logger.LogInformation("Train status: {status}", status);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error checking train status.");
                }
            }
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private static bool IsWeekday()
    {
        var today = DateTime.Now.DayOfWeek;
        return today is >= DayOfWeek.Monday and <= DayOfWeek.Friday;
    }

    private static bool IsTimeToCheck()
    {
        var now = DateTime.Now;
        return now is { Hour: 7, Minute: 30 };
    }
}