using System.Text;
using TrainChecker.Services.NationalRail;
using TrainChecker.Services.Telegram;
using TrainChecker.Models;

namespace TrainChecker.Services.Train;

public class TrainService(INationalRailService nationalRailService, ITelegramService telegramService)
    : ITrainService
{
    public async Task<HuxleyResponse?> GetAndSendTrainStatusAsync(string departureStation, string arrivalStation)
    {
        var huxleyResponse = await nationalRailService.GetTrainStatusAsync(DateTime.Now.ToString("HH:mm"), departureStation, arrivalStation);
        if (huxleyResponse?.TrainServices != null)
        {
            var message = new StringBuilder();
            message.AppendLine($"*Train Status Update for {huxleyResponse.LocationName}*");
            foreach (var trainService in huxleyResponse.TrainServices)
            {
                var origin = trainService.Origin?.FirstOrDefault()?.LocationName ?? "Unknown";
                var destination = trainService.Destination?.FirstOrDefault()?.LocationName ?? "Unknown";
                var statusText = trainService.EstimatedTimeOfDeparture?.ToLowerInvariant() switch
                {
                    "on time" => "🟢 *On time*",
                    "cancelled" => "🔴 *Cancelled*",
                    _ => $"🟠 *{trainService.EstimatedTimeOfDeparture}*",
                };

                if (trainService.EstimatedTimeOfDeparture != "On time" && trainService.EstimatedTimeOfDeparture != "Cancelled" &&
                    TimeSpan.TryParse(trainService.ScheduledTimeOfDeparture, out var scheduledTime) &&
                    TimeSpan.TryParse(trainService.EstimatedTimeOfDeparture, out var estimatedTime))
                {
                    var delay = estimatedTime - scheduledTime;
                    if (delay.TotalMinutes > 0)
                    {
                        statusText += $" (delayed by {delay.TotalMinutes} min)";
                    }
                }

                message.AppendLine($"- *{trainService.ScheduledTimeOfDeparture}* from *{origin}* (Platform {trainService.Platform}) to *{destination}*: {statusText}");
            }
            await telegramService.SendMessageAsync(message.ToString());
        }
        return huxleyResponse;
    }
}
