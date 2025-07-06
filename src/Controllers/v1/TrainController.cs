using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TrainChecker.Configuration;
using TrainChecker.Services.Train;

namespace TrainChecker.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v1/trains")]
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

    [HttpGet("{origin}/to/{destination}")]
    public async Task<IActionResult> GetTrainStatus(string origin, string destination)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            return BadRequest("Origin station code is required");
        }

        if (string.IsNullOrWhiteSpace(destination))
        {
            return BadRequest("Destination station code is required");
        }

        var huxleyResponse = await trainService.GetAndSendTrainStatusAsync(origin.ToUpper(), destination.ToUpper());
        return Ok(huxleyResponse);
    }
}
