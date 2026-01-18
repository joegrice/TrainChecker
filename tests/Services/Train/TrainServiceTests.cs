using Moq;
using TrainChecker.Models;
using TrainChecker.Services.NationalRail;
using TrainChecker.Services.Telegram;
using TrainChecker.Services.Tfl;
using Xunit;
using TrainService = TrainChecker.Services.Train.TrainService;

namespace TrainChecker.Tests.Services.Train;

public class TrainServiceTests
{
    private readonly Mock<INationalRailService> _mockNationalRailService;
    private readonly Mock<ITelegramService> _mockTelegramService;
    private readonly Mock<ITflService> _mockTflService;
    private readonly TrainService _trainService;

    public TrainServiceTests()
    {
        _mockNationalRailService = new Mock<INationalRailService>();
        _mockTelegramService = new Mock<ITelegramService>();
        _mockTflService = new Mock<ITflService>();
        _trainService = new TrainService(_mockNationalRailService.Object, _mockTelegramService.Object, _mockTflService.Object);
        
        // Default setup for TflService to return empty list so existing tests pass
        _mockTflService.Setup(x => x.GetTubeStatusesAsync()).ReturnsAsync(new List<TubeStatus>());
    }

    [Fact]
    public async Task GetAndSendTrainStatusAsync_WithValidResponse_SendsFormattedMessage()
    {
        // Arrange
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        var huxleyResponse = new HuxleyResponse
        {
            OriginName = "London",
            DestinationName = "Birmingham New Street",
            TrainServices =
            [
                new TrainChecker.Models.TrainService
                {
                    ScheduledTimeOfDeparture = "08:30",
                    EstimatedTimeOfDeparture = "On time",
                    Platform = "1",
                    Origin = [new Location { LocationName = "London Euston" }],
                    Destination = [new Location { LocationName = "Birmingham New Street" }]
                }
            ]
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
            msg.Contains("*LONDON → BIRMINGHAM NEW STREET*") &&
            msg.Contains("🟢 On time") &&
            msg.Contains("08:30") &&
            msg.Contains("Plat 1")
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
            OriginName = "London",
            DestinationName = "Birmingham New Street",
            TrainServices =
            [
                new TrainChecker.Models.TrainService
                {
                    ScheduledTimeOfDeparture = "08:30",
                    EstimatedTimeOfDeparture = "08:45",
                    Platform = "2",
                    Origin = [new Location { LocationName = "London Euston" }],
                    Destination = [new Location { LocationName = "Birmingham New Street" }]
                }
            ]
        };

        _mockNationalRailService
            .Setup(x => x.GetTrainStatusAsync(It.IsAny<string>(), departureStation, arrivalStation))
            .ReturnsAsync(huxleyResponse);

        // Act
        await _trainService.GetAndSendTrainStatusAsync(departureStation, arrivalStation);

        // Assert
        _mockTelegramService.Verify(x => x.SendMessageAsync(It.Is<string>(msg => 
            msg.Contains("🟠 08:45") &&
            msg.Contains("(+15m)")
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
            OriginName = "London",
            DestinationName = "Birmingham New Street",
            TrainServices =
            [
                new TrainChecker.Models.TrainService
                {
                    ScheduledTimeOfDeparture = "08:30",
                    EstimatedTimeOfDeparture = "Cancelled",
                    Platform = "3",
                    Origin = [new Location { LocationName = "London Euston" }],
                    Destination = [new Location { LocationName = "Birmingham New Street" }]
                }
            ]
        };

        _mockNationalRailService
            .Setup(x => x.GetTrainStatusAsync(It.IsAny<string>(), departureStation, arrivalStation))
            .ReturnsAsync(huxleyResponse);

        // Act
        await _trainService.GetAndSendTrainStatusAsync(departureStation, arrivalStation);

        // Assert
        _mockTelegramService.Verify(x => x.SendMessageAsync(It.Is<string>(msg => 
            msg.Contains("🔴 Cancelled")
        )), Times.Once);
    }

    [Fact]
    public async Task GetAndSendTrainStatusAsync_WithNullResponse_SendsNoServicesMessage()
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
        _mockTelegramService.Verify(x => x.SendMessageAsync(It.Is<string>(msg => msg.Contains("No train services found for LDN to BHM."))), Times.Once);
    }

    [Fact]
    public async Task GetAndSendTrainStatusAsync_WithNullTrainServices_SendsNoServicesMessage()
    {
        // Arrange
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        var huxleyResponse = new HuxleyResponse
        {
            OriginName = "London",
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
        _mockTelegramService.Verify(x => x.SendMessageAsync(It.Is<string>(msg => msg.Contains("No train services found for LDN to BHM."))), Times.Once);
    }

    [Fact]
    public async Task GetAndSendTrainStatusAsync_WithGoodTubeService_ShowsCorrectMessage()
    {
        // Arrange
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        
        _mockNationalRailService
            .Setup(x => x.GetTrainStatusAsync(It.IsAny<string>(), departureStation, arrivalStation))
            .ReturnsAsync((HuxleyResponse?)null);

        var tubeStatuses = new List<TubeStatus>
        {
            new TubeStatus { Name = "Bakerloo", LineStatuses = [new LineStatus { StatusSeverityDescription = "Good Service" }] },
            new TubeStatus { Name = "Central", LineStatuses = [new LineStatus { StatusSeverityDescription = "Good Service" }] }
        };
        
        _mockTflService.Setup(x => x.GetTubeStatusesAsync()).ReturnsAsync(tubeStatuses);

        // Act
        await _trainService.GetAndSendTrainStatusAsync(departureStation, arrivalStation);

        // Assert
        _mockTelegramService.Verify(x => x.SendMessageAsync(It.Is<string>(msg =>
            msg.Contains("All tube lines reporting a Good service!")
        )), Times.Once);
    }

    [Fact]
    public async Task GetAndSendTrainStatusAsync_WithBadTubeService_ShowsIssues()
    {
        // Arrange
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        
        _mockNationalRailService
            .Setup(x => x.GetTrainStatusAsync(It.IsAny<string>(), departureStation, arrivalStation))
            .ReturnsAsync((HuxleyResponse?)null);

        var tubeStatuses = new List<TubeStatus>
        {
            new TubeStatus { Name = "Bakerloo", LineStatuses = [new LineStatus { StatusSeverityDescription = "Good Service" }] },
            new TubeStatus { Name = "Northern", LineStatuses = [new LineStatus { StatusSeverityDescription = "Minor Delays" }] }
        };
        
        _mockTflService.Setup(x => x.GetTubeStatusesAsync()).ReturnsAsync(tubeStatuses);

        // Act
        await _trainService.GetAndSendTrainStatusAsync(departureStation, arrivalStation);

        // Assert
        _mockTelegramService.Verify(x => x.SendMessageAsync(It.Is<string>(msg =>
            !msg.Contains("All tube lines reporting a Good service") &&
            msg.Contains("Northern: *Minor Delays*") &&
            !msg.Contains("Bakerloo")
        )), Times.Once);
    }
}