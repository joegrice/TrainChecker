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
        if (huxleyResponse?.TrainServices != null && huxleyResponse.TrainServices.Length != 0)
        {
            var message = new StringBuilder();
            message.AppendLine($"*Train Status Update for {huxleyResponse.OriginName} to {huxleyResponse.DestinationName}*");
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
                
                var platform = !string.IsNullOrEmpty(trainService.Platform)
                    ? $" (Platform {trainService.Platform})"
                    : "";
                
                var trainLength = trainService.Length is not null
                    ? $" ({trainService.Length} Coaches)"
                    : "";

                message.AppendLine($"- *{trainService.ScheduledTimeOfDeparture}* from *{origin}*{platform} to *{destination}{trainLength}*: {statusText}");
            }
            await telegramService.SendMessageAsync(message.ToString());
        }
        else
        {
            var message = $"No train services found for {departureStation} to {arrivalStation}.";
            await telegramService.SendMessageAsync(message);
        }
        return huxleyResponse;
    }
}
