using System.Text;
using Microsoft.Extensions.Options;
using Quartz;

namespace TrainChecker;

[DisallowConcurrentExecution]
public class TrainCheckJob : IJob
{
    private readonly ILogger<TrainCheckJob> _logger;
    private readonly NationalRailService _nationalRailService;
    private readonly TelegramService _telegramService;
    private readonly TrainCheckerOptions _options;

    public TrainCheckJob(ILogger<TrainCheckJob> logger, NationalRailService nationalRailService, TelegramService telegramService, IOptions<TrainCheckerOptions> options)
    {
        _logger = logger;
        _nationalRailService = nationalRailService;
        _telegramService = telegramService;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var huxleyResponse = await _nationalRailService.GetTrainStatusAsync(_options.Time);
            if (huxleyResponse?.TrainServices != null)
            {
                var message = new StringBuilder();
                message.AppendLine("*Train Status Update*");
                foreach (var trainService in huxleyResponse.TrainServices)
                {
                    var origin = trainService.Origin?.FirstOrDefault()?.LocationName ?? "Unknown";
                    var destination = trainService.Destination?.FirstOrDefault()?.LocationName ?? "Unknown";
                    message.AppendLine($"- *{trainService.ScheduledTimeOfDeparture}* from {origin} to {destination} is {trainService.EstimatedTimeOfDeparture}");
                }
                await _telegramService.SendMessageAsync(message.ToString());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking train status.");
        }
    }
}