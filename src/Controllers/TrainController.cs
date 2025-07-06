using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TrainChecker.Configuration;
using TrainChecker.Services.Train;

namespace TrainChecker.Controllers;

[ApiController]
[Route("trains")]
public class TrainController : ControllerBase
{
    private readonly ITrainService _trainService;
    private readonly TrainCheckerOptions _options;

    public TrainController(ITrainService trainService, IOptions<TrainCheckerOptions> options)
    {
        _trainService = trainService;
        _options = options.Value;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrainStatus()
    {
        var huxleyResponse = await _trainService.GetAndSendTrainStatusAsync(_options.DepartureStation, _options.ArrivalStation);
        return Ok(huxleyResponse);
    }
}
