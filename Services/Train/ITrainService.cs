using TrainChecker.Models;

namespace TrainChecker.Services.Train;

public interface ITrainService
{
    Task<HuxleyResponse?> GetAndSendTrainStatusAsync();
}