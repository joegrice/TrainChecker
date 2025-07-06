using Microsoft.AspNetCore.Mvc;
using TrainChecker.Services.Train;

namespace TrainChecker.Controllers;

[ApiController]
[Route("trains")]
public class TrainController : ControllerBase
{
    private readonly ITrainService _trainService;

    public TrainController(ITrainService trainService)
    {
        _trainService = trainService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrainStatus()
    {
        var huxleyResponse = await _trainService.GetAndSendTrainStatusAsync();
        return Ok(huxleyResponse);
    }
}
