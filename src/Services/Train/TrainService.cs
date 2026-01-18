using System.Text;
using TrainChecker.Services.NationalRail;
using TrainChecker.Services.Telegram;
using TrainChecker.Services.Tfl;
using TrainChecker.Models;

namespace TrainChecker.Services.Train;

public class TrainService(INationalRailService nationalRailService, ITelegramService telegramService, ITflService tflService)
    : ITrainService
{
    public async Task<HuxleyResponse?> GetAndSendTrainStatusAsync(string departureStation, string arrivalStation)
    {
        var message = new StringBuilder();
        var huxleyResponse = await nationalRailService.GetTrainStatusAsync(DateTime.Now.ToString("HH:mm"), departureStation, arrivalStation);
        
        if (huxleyResponse?.TrainServices != null && huxleyResponse.TrainServices.Length != 0)
        {
            var originName = huxleyResponse.OriginName?.ToUpper() ?? departureStation;
            var destinationName = huxleyResponse.DestinationName?.ToUpper() ?? arrivalStation;
            
            var rows = new List<string>();

            foreach (var trainService in huxleyResponse.TrainServices)
            {
                var statusText = trainService.EstimatedTimeOfDeparture?.ToLowerInvariant() switch
                {
                    "on time" => "🟢 On time",
                    "cancelled" => "🔴 Cancelled",
                    _ => $"🟠 {trainService.EstimatedTimeOfDeparture}",
                };

                if (trainService.EstimatedTimeOfDeparture != "On time" && trainService.EstimatedTimeOfDeparture != "Cancelled" &&
                    TimeSpan.TryParse(trainService.ScheduledTimeOfDeparture, out var scheduledTime) &&
                    TimeSpan.TryParse(trainService.EstimatedTimeOfDeparture, out var estimatedTime))
                {
                    var delay = estimatedTime - scheduledTime;
                    if (delay.TotalMinutes > 0)
                    {
                        statusText += $" (+{delay.TotalMinutes}m)";
                    }
                }
                
                var platform = !string.IsNullOrEmpty(trainService.Platform)
                    ? $"Plat {trainService.Platform}"
                    : "TBC";
                
                // Pad platform to align columns (approx 7 chars)
                platform = platform.PadRight(7);
                
                var trainLength = trainService.Length is not null
                    ? $"{trainService.Length} coaches"
                    : "";
                
                // Pad length to align columns (approx 10 chars)
                trainLength = trainLength.PadRight(10);

                rows.Add($"{trainService.ScheduledTimeOfDeparture}  {platform}  {trainLength}  {statusText}");
            }
            
            message.AppendLine($"*{originName} → {destinationName}*");
            
            foreach (var row in rows)
            {
                message.AppendLine(row);
            }
        }
        else
        {
            message.AppendLine($"No train services found for {departureStation} to {arrivalStation}.");
        }

        // Add Tube Status
        var tubeStatuses = await tflService.GetTubeStatusesAsync();
        if (tubeStatuses.Count != 0)
        {
            message.AppendLine();
            message.AppendLine("*Tube Status:*");
            
            var allGood = tubeStatuses.All(t => t.LineStatuses.All(ls => ls.StatusSeverityDescription == "Good Service"));
            
            if (allGood)
            {
                message.AppendLine("All tube lines reporting a Good service!");
            }
            else
            {
                foreach (var status in tubeStatuses.OrderBy(t => t.Name))
                {
                    var lineStatus = status.LineStatuses.FirstOrDefault();
                    if (lineStatus != null && lineStatus.StatusSeverityDescription != "Good Service")
                    {
                        message.AppendLine($"{status.Name}: *{lineStatus.StatusSeverityDescription}*");
                    }
                }
            }
        }

        await telegramService.SendMessageAsync(message.ToString());
        return huxleyResponse;
    }
}