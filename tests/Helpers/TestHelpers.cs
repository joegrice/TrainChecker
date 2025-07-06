using TrainChecker.Models;

namespace TrainChecker.Tests.Helpers;

public static class TestHelpers
{
    public static HuxleyResponse CreateSampleHuxleyResponse(
        string locationName = "London Euston",
        string crs = "EUS",
        int numberOfServices = 1)
    {
        var trainServices = new List<TrainService>();
        
        for (int i = 0; i < numberOfServices; i++)
        {
            trainServices.Add(new TrainService
            {
                ScheduledTimeOfDeparture = $"0{8 + i}:30",
                EstimatedTimeOfDeparture = (i % 3) switch
                {
                    0 => "On time",
                    1 => "Cancelled",
                    _ => $"0{8 + i}:{30 + (i * 5)}"
                },
                Platform = $"{i + 1}",
                Operator = "Test Operator",
                Origin = new[] { new Location { LocationName = locationName, Crs = crs } },
                Destination = new[] { new Location { LocationName = "Birmingham New Street", Crs = "BHM" } }
            });
        }

        return new HuxleyResponse
        {
            LocationName = locationName,
            Crs = crs,
            TrainServices = trainServices.ToArray()
        };
    }

    public static TrainService CreateSampleTrainService(
        string scheduledTime = "08:30",
        string estimatedTime = "On time",
        string platform = "1",
        string originName = "London Euston",
        string destinationName = "Birmingham New Street")
    {
        return new TrainService
        {
            ScheduledTimeOfDeparture = scheduledTime,
            EstimatedTimeOfDeparture = estimatedTime,
            Platform = platform,
            Operator = "Test Operator",
            Origin = new[] { new Location { LocationName = originName, Crs = "TST" } },
            Destination = new[] { new Location { LocationName = destinationName, Crs = "TST2" } }
        };
    }

    public static Location CreateSampleLocation(
        string locationName = "Test Station",
        string crs = "TST")
    {
        return new Location
        {
            LocationName = locationName,
            Crs = crs
        };
    }

    public static HuxleyResponse CreateEmptyHuxleyResponse()
    {
        return new HuxleyResponse
        {
            LocationName = "Test Station",
            Crs = "TST",
            TrainServices = Array.Empty<TrainService>()
        };
    }

    public static HuxleyResponse CreateNullTrainServicesResponse()
    {
        return new HuxleyResponse
        {
            LocationName = "Test Station",
            Crs = "TST",
            TrainServices = null
        };
    }

    public static TrainService CreateDelayedTrainService(
        int delayMinutes = 15)
    {
        var scheduledTime = TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(30));
        var delayedTime = scheduledTime.Add(TimeSpan.FromMinutes(delayMinutes));

        return new TrainService
        {
            ScheduledTimeOfDeparture = scheduledTime.ToString(@"hh\:mm"),
            EstimatedTimeOfDeparture = delayedTime.ToString(@"hh\:mm"),
            Platform = "1",
            Operator = "Test Operator",
            Origin = new[] { CreateSampleLocation("London Euston", "EUS") },
            Destination = new[] { CreateSampleLocation("Birmingham New Street", "BHM") }
        };
    }

    public static TrainService CreateCancelledTrainService()
    {
        return new TrainService
        {
            ScheduledTimeOfDeparture = "08:30",
            EstimatedTimeOfDeparture = "Cancelled",
            Platform = "1",
            Operator = "Test Operator",
            Origin = new[] { CreateSampleLocation("London Euston", "EUS") },
            Destination = new[] { CreateSampleLocation("Birmingham New Street", "BHM") }
        };
    }

    public static string CreateExpectedTelegramMessage(
        TrainService trainService)
    {
        var origin = trainService.Origin?.FirstOrDefault()?.LocationName ?? "Unknown";
        var destination = trainService.Destination?.FirstOrDefault()?.LocationName ?? "Unknown";
        var statusText = trainService.EstimatedTimeOfDeparture?.ToLowerInvariant() switch
        {
            "on time" => "🟢 *On time*",
            "cancelled" => "🔴 *Cancelled*",
            _ => $"🟠 *{trainService.EstimatedTimeOfDeparture}*",
        };

        return $"*Train Status Update*\n- *{trainService.ScheduledTimeOfDeparture}* from {origin} (Platform {trainService.Platform}) to {destination} is {statusText}\n";
    }
}