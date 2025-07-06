using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using TrainChecker.Models;
using TrainChecker.Services.NationalRail;
using TrainChecker.Services.Telegram;
using Xunit;

namespace TrainChecker.Tests.Integration;

public class TrainControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TrainControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTrainStatus_WithMockedServices_ReturnsExpectedResponse()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var mockTelegramHandler = new Mock<HttpMessageHandler>();

        var expectedHuxleyResponse = new HuxleyResponse
        {
            LocationName = "London Euston",
            Crs = "EUS",
            TrainServices = new[]
            {
                new TrainChecker.Models.TrainService
                {
                    ScheduledTimeOfDeparture = "08:30",
                    EstimatedTimeOfDeparture = "On time",
                    Platform = "1",
                    Origin = new[] { new Location { LocationName = "London Euston", Crs = "EUS" } },
                    Destination = new[] { new Location { LocationName = "Birmingham New Street", Crs = "BHM" } }
                }
            }
        };

        var huxleyJson = JsonSerializer.Serialize(expectedHuxleyResponse);
        var huxleyResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(huxleyJson)
        };

        var telegramResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"ok\":true}")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.Host == "huxley2.azurewebsites.net"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(huxleyResponse);

        mockTelegramHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.Host == "api.telegram.org"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(telegramResponse);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TrainChecker:DepartureStation"] = "EUS",
                    ["TrainChecker:ArrivalStation"] = "BHM",
                    ["TrainChecker:ApiKey"] = "test-api-key",
                    ["Telegram:BotToken"] = "test-bot-token",
                    ["Telegram:ChatId"] = "test-chat-id"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove existing HttpClient registrations
                var nationalRailDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(HttpClient) && 
                    d.ImplementationType?.Name.Contains("NationalRail") == true);
                if (nationalRailDescriptor != null)
                    services.Remove(nationalRailDescriptor);

                var telegramDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(HttpClient) && 
                    d.ImplementationType?.Name.Contains("Telegram") == true);
                if (telegramDescriptor != null)
                    services.Remove(telegramDescriptor);

                // Add mocked HttpClients
                services.AddHttpClient<INationalRailService, NationalRailService>()
                    .ConfigurePrimaryHttpMessageHandler(() => mockHttpMessageHandler.Object);

                services.AddHttpClient<ITelegramService, TelegramService>()
                    .ConfigurePrimaryHttpMessageHandler(() => mockTelegramHandler.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/trains");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<HuxleyResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.Equal(expectedHuxleyResponse.LocationName, result.LocationName);
        Assert.Equal(expectedHuxleyResponse.Crs, result.Crs);
        Assert.Single(result.TrainServices!);
    }

    [Fact]
    public async Task GetTrainStatus_WithoutConfiguration_Returns500()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear(); // Remove all configuration sources
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/trains");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task Application_StartsSuccessfully()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TrainChecker:DepartureStation"] = "EUS",
                    ["TrainChecker:ArrivalStation"] = "BHM",
                    ["TrainChecker:ApiKey"] = "test-api-key",
                    ["Telegram:BotToken"] = "test-bot-token",
                    ["Telegram:ChatId"] = "test-chat-id"
                });
            });
        }).CreateClient();

        // Act & Assert
        Assert.NotNull(client);
    }
}