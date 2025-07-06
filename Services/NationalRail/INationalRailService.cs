using TrainChecker.Models;

namespace TrainChecker.Services.NationalRail;

public interface INationalRailService
{
    Task<HuxleyResponse?> GetTrainStatusAsync(string time, string departureStation, string arrivalStation);
}