using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using TrainChecker.Jobs;
using TrainChecker.Models;
using TrainChecker.Services.Train;
using Xunit;

namespace TrainChecker.Tests.Jobs;

public class TrainCheckJobTests
{
    private readonly Mock<ILogger<TrainCheckJob>> _mockLogger;
    private readonly Mock<ITrainService> _mockTrainService;
    private readonly Mock<IJobExecutionContext> _mockJobContext;
    private readonly Mock<JobDataMap> _mockJobDataMap;
    private readonly TrainCheckJob _job;

    public TrainCheckJobTests()
    {
        _mockLogger = new Mock<ILogger<TrainCheckJob>>();
        _mockTrainService = new Mock<ITrainService>();
        _mockJobContext = new Mock<IJobExecutionContext>();
        _mockJobDataMap = new Mock<JobDataMap>();

        _job = new TrainCheckJob(_mockLogger.Object, _mockTrainService.Object);

        // Setup job context
        Mock<IJobDetail> mockJobDetail = new();
        mockJobDetail.Setup(x => x.Key).Returns(() => new JobKey(nameof(TrainCheckJob)));
        mockJobDetail.Setup(x => x.JobDataMap).Returns(_mockJobDataMap.Object);
        _mockJobContext.Setup(x => x.JobDetail).Returns(mockJobDetail.Object);
    }

    [Fact]
    public async Task Execute_WithValidJobData_CallsTrainService()
    {
        // Arrange
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        
        _mockJobDataMap.Setup(x => x.GetString("DepartureStation")).Returns(departureStation);
        _mockJobDataMap.Setup(x => x.GetString("ArrivalStation")).Returns(arrivalStation);

        var expectedResponse = new HuxleyResponse();
        _mockTrainService
            .Setup(x => x.GetAndSendTrainStatusAsync(departureStation, arrivalStation))
            .ReturnsAsync(expectedResponse);

        // Act
        await _job.Execute(_mockJobContext.Object);

        // Assert
        _mockTrainService.Verify(
            x => x.GetAndSendTrainStatusAsync(departureStation, arrivalStation),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WithNullDepartureStation_LogsErrorAndReturns()
    {
        // Arrange
        _mockJobDataMap.Setup(x => x.GetString("DepartureStation")).Returns((string?)null);
        _mockJobDataMap.Setup(x => x.GetString("ArrivalStation")).Returns("BHM");

        // Act
        await _job.Execute(_mockJobContext.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Departure or arrival station not provided")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockTrainService.Verify(
            x => x.GetAndSendTrainStatusAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_WithNullArrivalStation_LogsErrorAndReturns()
    {
        // Arrange
        _mockJobDataMap.Setup(x => x.GetString("DepartureStation")).Returns("LDN");
        _mockJobDataMap.Setup(x => x.GetString("ArrivalStation")).Returns((string?)null);

        // Act
        await _job.Execute(_mockJobContext.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Departure or arrival station not provided")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockTrainService.Verify(
            x => x.GetAndSendTrainStatusAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_WithEmptyDepartureStation_LogsErrorAndReturns()
    {
        // Arrange
        _mockJobDataMap.Setup(x => x.GetString("DepartureStation")).Returns("");
        _mockJobDataMap.Setup(x => x.GetString("ArrivalStation")).Returns("BHM");

        // Act
        await _job.Execute(_mockJobContext.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Departure or arrival station not provided")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockTrainService.Verify(
            x => x.GetAndSendTrainStatusAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_WithEmptyArrivalStation_LogsErrorAndReturns()
    {
        // Arrange
        _mockJobDataMap.Setup(x => x.GetString("DepartureStation")).Returns("LDN");
        _mockJobDataMap.Setup(x => x.GetString("ArrivalStation")).Returns("");

        // Act
        await _job.Execute(_mockJobContext.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Departure or arrival station not provided")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockTrainService.Verify(
            x => x.GetAndSendTrainStatusAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_WhenTrainServiceThrows_LogsErrorAndDoesNotRethrow()
    {
        // Arrange
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        var expectedException = new InvalidOperationException("Service error");
        
        _mockJobDataMap.Setup(x => x.GetString("DepartureStation")).Returns(departureStation);
        _mockJobDataMap.Setup(x => x.GetString("ArrivalStation")).Returns(arrivalStation);

        _mockTrainService
            .Setup(x => x.GetAndSendTrainStatusAsync(departureStation, arrivalStation))
            .ThrowsAsync(expectedException);

        // Act
        await _job.Execute(_mockJobContext.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error checking train status")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WithValidJobData_DoesNotLogErrors()
    {
        // Arrange
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        
        _mockJobDataMap.Setup(x => x.GetString("DepartureStation")).Returns(departureStation);
        _mockJobDataMap.Setup(x => x.GetString("ArrivalStation")).Returns(arrivalStation);

        var expectedResponse = new HuxleyResponse();
        _mockTrainService
            .Setup(x => x.GetAndSendTrainStatusAsync(departureStation, arrivalStation))
            .ReturnsAsync(expectedResponse);

        // Act
        await _job.Execute(_mockJobContext.Object);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}