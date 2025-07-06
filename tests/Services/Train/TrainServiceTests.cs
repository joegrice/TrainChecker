using Moq;
using TrainChecker.Models;
using TrainChecker.Services.NationalRail;
using TrainChecker.Services.Telegram;
using Xunit;

namespace TrainChecker.Tests.Services.Train;

public class TrainServiceTests
{
    private readonly Mock<INationalRailService> _mockNationalRailService;
    private readonly Mock<ITelegramService> _mockTelegramService;
    private readonly TrainChecker.Services.Train.TrainService _trainService;

    public TrainServiceTests()
    {
        _mockNationalRailService = new Mock<INationalRailService>();
        _mockTelegramService = new Mock<ITelegramService>();
        _trainService = new TrainChecker.Services.Train.TrainService(_mockNationalRailService.Object, _mockTelegramService.Object);
    }

    [Fact]
    public async Task GetAndSendTrainStatusAsync_WithValidResponse_SendsFormattedMessage()
    {
        // Arrange
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        var huxleyResponse = new HuxleyResponse
        {
            LocationName = "London",
            TrainServices = new[]
            {
                new TrainService
                {
                    ScheduledTimeOfDeparture = "08:30",
                    EstimatedTimeOfDeparture = "On time",
                    Platform = "1",
                    Origin = new[] { new Location { LocationName = "London Euston" } },
                    Destination = new[] { new Location { LocationName = "Birmingham New Street" } }
                }
            }
        };

        _mockNationalRailService
            .Setup(x => x.GetTrainStatusAsync(It.IsAny<string>(), departureStation, arrivalStation))
            .ReturnsAsync(huxleyResponse);

        // Act
        var result = await _trainService.GetAndSendTrainStatusAsync(departureStation, arrivalStation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(huxleyResponse, result);
        
        _mockTelegramService.Verify(x => x.SendMessageAsync(It.Is<string>(msg => 
            msg.Contains("*Train Status Update*") &&
            msg.Contains("🟢 *On time*") &&
            msg.Contains("08:30") &&
            msg.Contains("London Euston") &&
            msg.Contains("Birmingham New Street") &&
            msg.Contains("Platform 1")
        )), Times.Once);
    }

    [Fact]
    public async Task GetAndSendTrainStatusAsync_WithDelayedTrain_ShowsDelayInformation()
    {
        // Arrange
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        var huxleyResponse = new HuxleyResponse
        {
            TrainServices = new[]
            {
                new TrainService
                {
                    ScheduledTimeOfDeparture = "08:30",
                    EstimatedTimeOfDeparture = "08:45",
                    Platform = "2",
                    Origin = new[] { new Location { LocationName = "London Euston" } },
                    Destination = new[] { new Location { LocationName = "Birmingham New Street" } }
                }
            }
        };

        _mockNationalRailService
            .Setup(x => x.GetTrainStatusAsync(It.IsAny<string>(), departureStation, arrivalStation))
            .ReturnsAsync(huxleyResponse);

        // Act
        await _trainService.GetAndSendTrainStatusAsync(departureStation, arrivalStation);

        // Assert
        _mockTelegramService.Verify(x => x.SendMessageAsync(It.Is<string>(msg => 
            msg.Contains("🟠 *08:45*") &&
            msg.Contains("delayed by 15 min")
        )), Times.Once);
    }

    [Fact]
    public async Task GetAndSendTrainStatusAsync_WithCancelledTrain_ShowsCancelledStatus()
    {
        // Arrange
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        var huxleyResponse = new HuxleyResponse
        {
            TrainServices = new[]
            {
                new TrainService
                {
                    ScheduledTimeOfDeparture = "08:30",
                    EstimatedTimeOfDeparture = "Cancelled",
                    Platform = "3",
                    Origin = new[] { new Location { LocationName = "London Euston" } },
                    Destination = new[] { new Location { LocationName = "Birmingham New Street" } }
                }
            }
        };

        _mockNationalRailService
            .Setup(x => x.GetTrainStatusAsync(It.IsAny<string>(), departureStation, arrivalStation))
            .ReturnsAsync(huxleyResponse);

        // Act
        await _trainService.GetAndSendTrainStatusAsync(departureStation, arrivalStation);

        // Assert
        _mockTelegramService.Verify(x => x.SendMessageAsync(It.Is<string>(msg => 
            msg.Contains("🔴 *Cancelled*")
        )), Times.Once);
    }

    [Fact]
    public async Task GetAndSendTrainStatusAsync_WithNullResponse_DoesNotSendMessage()
    {
        // Arrange
        var departureStation = "LDN";
        var arrivalStation = "BHM";

        _mockNationalRailService
            .Setup(x => x.GetTrainStatusAsync(It.IsAny<string>(), departureStation, arrivalStation))
            .ReturnsAsync((HuxleyResponse?)null);

        // Act
        var result = await _trainService.GetAndSendTrainStatusAsync(departureStation, arrivalStation);

        // Assert
        Assert.Null(result);
        _mockTelegramService.Verify(x => x.SendMessageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAndSendTrainStatusAsync_WithNullTrainServices_DoesNotSendMessage()
    {
        // Arrange
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        var huxleyResponse = new HuxleyResponse
        {
            LocationName = "London",
            TrainServices = null
        };

        _mockNationalRailService
            .Setup(x => x.GetTrainStatusAsync(It.IsAny<string>(), departureStation, arrivalStation))
            .ReturnsAsync(huxleyResponse);

        // Act
        var result = await _trainService.GetAndSendTrainStatusAsync(departureStation, arrivalStation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(huxleyResponse, result);
        _mockTelegramService.Verify(x => x.SendMessageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAndSendTrainStatusAsync_WithMissingOriginDestination_UsesUnknown()
    {
        // Arrange
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        var huxleyResponse = new HuxleyResponse
        {
            TrainServices = new[]
            {
                new TrainService
                {
                    ScheduledTimeOfDeparture = "08:30",
                    EstimatedTimeOfDeparture = "On time",
                    Platform = "1",
                    Origin = null,
                    Destination = null
                }
            }
        };

        _mockNationalRailService
            .Setup(x => x.GetTrainStatusAsync(It.IsAny<string>(), departureStation, arrivalStation))
            .ReturnsAsync(huxleyResponse);

        // Act
        await _trainService.GetAndSendTrainStatusAsync(departureStation, arrivalStation);

        // Assert
        _mockTelegramService.Verify(x => x.SendMessageAsync(It.Is<string>(msg => 
            msg.Contains("from Unknown") &&
            msg.Contains("to Unknown")
        )), Times.Once);
    }
}