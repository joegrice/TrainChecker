using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using TrainChecker.Controllers;
using TrainChecker.Models;
using TrainChecker.Options;
using TrainChecker.Services.Train;
using Xunit;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace TrainChecker.Tests.Controllers;

public class TrainControllerTests
{
    private readonly Mock<ITrainService> _mockTrainService;
    private readonly TrainCheckerOptions _options;
    private readonly TrainController _controller;

    public TrainControllerTests()
    {
        _mockTrainService = new Mock<ITrainService>();
        _options = new TrainCheckerOptions
        {
            DepartureStation = "LDN",
            ArrivalStation = "BHM"
        };

        var optionsWrapper = MsOptions.Create(_options);
        _controller = new TrainController(_mockTrainService.Object, optionsWrapper);
    }

    [Fact]
    public async Task GetTrainStatus_WithValidResponse_ReturnsOkResult()
    {
        // Arrange
        var expectedResponse = new HuxleyResponse
        {
            LocationName = "London",
            Crs = "LDN",
            TrainServices = new[]
            {
                new TrainChecker.Models.TrainService
                {
                    ScheduledTimeOfDeparture = "08:30",
                    EstimatedTimeOfDeparture = "On time",
                    Platform = "1"
                }
            }
        };

        _mockTrainService
            .Setup(x => x.GetAndSendTrainStatusAsync(_options.DepartureStation, _options.ArrivalStation))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetTrainStatus();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = Assert.IsType<HuxleyResponse>(okResult.Value);
        Assert.Equal(expectedResponse.LocationName, returnedResponse.LocationName);
        Assert.Equal(expectedResponse.Crs, returnedResponse.Crs);
        Assert.Single(returnedResponse.TrainServices!);
    }

    [Fact]
    public async Task GetTrainStatus_WithNullResponse_ReturnsOkWithNull()
    {
        // Arrange
        _mockTrainService
            .Setup(x => x.GetAndSendTrainStatusAsync(_options.DepartureStation, _options.ArrivalStation))
            .ReturnsAsync((HuxleyResponse?)null);

        // Act
        var result = await _controller.GetTrainStatus();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Null(okResult.Value);
    }

    [Fact]
    public async Task GetTrainStatus_CallsTrainServiceWithCorrectParameters()
    {
        // Arrange
        var expectedResponse = new HuxleyResponse();
        _mockTrainService
            .Setup(x => x.GetAndSendTrainStatusAsync(_options.DepartureStation, _options.ArrivalStation))
            .ReturnsAsync(expectedResponse);

        // Act
        await _controller.GetTrainStatus();

        // Assert
        _mockTrainService.Verify(
            x => x.GetAndSendTrainStatusAsync(_options.DepartureStation, _options.ArrivalStation),
            Times.Once);
    }

    [Fact]
    public async Task GetTrainStatus_WhenServiceThrows_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Service error");
        _mockTrainService
            .Setup(x => x.GetAndSendTrainStatusAsync(_options.DepartureStation, _options.ArrivalStation))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.GetTrainStatus());
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesCorrectly()
    {
        // Arrange
        var mockTrainService = new Mock<ITrainService>();
        var options = new TrainCheckerOptions
        {
            DepartureStation = "TEST1",
            ArrivalStation = "TEST2"
        };
        var optionsWrapper = MsOptions.Create(options);

        // Act
        var controller = new TrainController(mockTrainService.Object, optionsWrapper);

        // Assert
        Assert.NotNull(controller);
    }

    [Fact]
    public async Task GetTrainStatus_WithEmptyTrainServices_ReturnsOkResult()
    {
        // Arrange
        var expectedResponse = new HuxleyResponse
        {
            LocationName = "London",
            Crs = "LDN",
            TrainServices = Array.Empty<TrainChecker.Models.TrainService>()
        };

        _mockTrainService
            .Setup(x => x.GetAndSendTrainStatusAsync(_options.DepartureStation, _options.ArrivalStation))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetTrainStatus();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = Assert.IsType<HuxleyResponse>(okResult.Value);
        Assert.Empty(returnedResponse.TrainServices!);
    }
}