using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TrainChecker.Configuration;
using TrainChecker.Models;
using TrainChecker.Services.NationalRail;
using Xunit;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace TrainChecker.Tests.Services.NationalRail;

public class NationalRailServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger<NationalRailService>> _mockLogger;
    private readonly TrainCheckerOptions _options;
    private readonly NationalRailService _nationalRailService;

    public NationalRailServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<NationalRailService>>();
        _options = new TrainCheckerOptions
        {
            ApiKey = "test-api-key",
            BaseAddress = "https://fake-api.test"
        };

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(_options.BaseAddress)
        };
        var optionsWrapper = MsOptions.Create(_options);
        
        _nationalRailService = new NationalRailService(httpClient, optionsWrapper, _mockLogger.Object);
    }

    [Fact]
    public async Task GetTrainStatusAsync_WithValidResponse_ReturnsHuxleyResponse()
    {
        // Arrange
        var time = "08:30";
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        
        var expectedResponse = new HuxleyResponse
        {
            OriginName = "London",
            Crs = "LDN",
            TrainServices = new[]
            {
                new TrainService
                {
                    ScheduledTimeOfDeparture = "08:30",
                    EstimatedTimeOfDeparture = "On time",
                    Platform = "1"
                }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(expectedResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri!.ToString().Contains($"/departures/{departureStation}/to/{arrivalStation}") &&
                    req.RequestUri.ToString().Contains($"accessToken={_options.ApiKey}") &&
                    req.RequestUri.ToString().Contains($"time={time}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _nationalRailService.GetTrainStatusAsync(time, departureStation, arrivalStation);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.OriginName, result.OriginName);
        Assert.Equal(expectedResponse.Crs, result.Crs);
        Assert.Single(result.TrainServices!);
        Assert.Equal("08:30", result.TrainServices[0].ScheduledTimeOfDeparture);
    }

    [Fact]
    public async Task GetTrainStatusAsync_WithHttpError_LogsErrorAndThrows()
    {
        // Arrange
        var time = "08:30";
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request"),
            ReasonPhrase = "Bad Request"
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _nationalRailService.GetTrainStatusAsync(time, departureStation, arrivalStation));

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error from National Rail API")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTrainStatusAsync_LogsRequestInformation()
    {
        // Arrange
        var time = "08:30";
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        await _nationalRailService.GetTrainStatusAsync(time, departureStation, arrivalStation);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Requesting train status from")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTrainStatusAsync_BuildsCorrectRequestUri()
    {
        // Arrange
        var time = "08:30";
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        };

        HttpRequestMessage? capturedRequest = null;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(httpResponse);

        // Act
        await _nationalRailService.GetTrainStatusAsync(time, departureStation, arrivalStation);

        // Assert
        Assert.NotNull(capturedRequest);
        var requestUri = capturedRequest.RequestUri!.ToString();
        Assert.Contains($"/departures/{departureStation}/to/{arrivalStation}", requestUri);
        Assert.Contains($"accessToken={_options.ApiKey}", requestUri);
        Assert.Contains($"time={time}", requestUri);
        Assert.Contains("timeWindow=60", requestUri);
        Assert.Contains("expand=true", requestUri);
    }

    [Fact]
    public async Task GetTrainStatusAsync_WithEmptyResponse_ReturnsNull()
    {
        // Arrange
        var time = "08:30";
        var departureStation = "LDN";
        var arrivalStation = "BHM";
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act
        var result = await _nationalRailService.GetTrainStatusAsync(time, departureStation, arrivalStation);

        // Assert
        Assert.Null(result);
    }
}