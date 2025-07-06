namespace TrainChecker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly NationalRailService _nationalRailService;

    public Worker(ILogger<Worker> logger, NationalRailService nationalRailService)
    {
        _logger = logger;
        _nationalRailService = nationalRailService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (IsWeekday() && IsTimeToCheck())
            {
                try
                {
                    var trainServices = await _nationalRailService.GetTrainStatusAsync();
                    foreach (var trainService in trainServices)
                    {
                        _logger.LogInformation("Train at {std} is {etd}", trainService.ScheduledTimeOfDeparture, trainService.EstimatedTimeOfDeparture);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking train status.");
                }
            }
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
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
