using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TrainChecker.Configuration;
using TrainChecker.Services.Train;

namespace TrainChecker.Controllers;

[ApiController]
[Route("trains")]
public class TrainController(ITrainService trainService, IOptions<TrainCheckerOptions> options)
    : ControllerBase
{
    private readonly TrainCheckerOptions _options = options.Value;

    [HttpGet]
    public async Task<IActionResult> GetTrainStatus()
    {
        var huxleyResponse = await trainService.GetAndSendTrainStatusAsync(_options.DepartureStation, _options.ArrivalStation);
        return Ok(huxleyResponse);
    }
}
