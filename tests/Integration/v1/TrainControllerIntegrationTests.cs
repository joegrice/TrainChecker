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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TrainChecker.Tests.Helpers;

namespace TrainChecker.Tests.Integration.v1;

public class TrainControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public TrainControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
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
            OriginName = "London Euston",
            Crs = "EUS",
            TrainServices = new[]
            {
                new TrainService
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
        var response = await client.GetAsync("api/v1/trains");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<HuxleyResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.Equal(expectedHuxleyResponse.OriginName, result.OriginName);
        Assert.Equal(expectedHuxleyResponse.Crs, result.Crs);
        Assert.Single(result.TrainServices!);
    }

    [Fact]
    public async Task GetTrainStatus_WithoutConfiguration_Returns500()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("IntegrationTests"); // Set environment for this test
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear(); // Remove all configuration sources
            });
            builder.ConfigureServices(services =>
            {
                // Disable authentication for this test
                services.PostConfigure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters.ValidateIssuer = false;
                    options.TokenValidationParameters.ValidateAudience = false;
                    options.TokenValidationParameters.ValidateLifetime = false;
                    options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                    options.TokenValidationParameters.RequireExpirationTime = false;
                    options.TokenValidationParameters.RequireSignedTokens = false;
                });

                // Mock INationalRailService to throw an exception
                var mockNationalRailService = new Mock<INationalRailService>();
                mockNationalRailService.Setup(s => s.GetTrainStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Simulated API call failure due to missing configuration."));

                var nationalRailServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(INationalRailService));
                if (nationalRailServiceDescriptor != null)
                {
                    services.Remove(nationalRailServiceDescriptor);
                }
                services.AddSingleton(mockNationalRailService.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("api/v1/trains");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public void Application_StartsSuccessfully()
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

    [Fact]
    public async Task GetTrainStatusWithParameters_WithValidParameters_ReturnsExpectedResponse()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var mockTelegramHandler = new Mock<HttpMessageHandler>();

        var expectedHuxleyResponse = new HuxleyResponse
        {
            OriginName = "Manchester Piccadilly",
            Crs = "MAN",
            TrainServices = new[]
            {
                new TrainService
                {
                    ScheduledTimeOfDeparture = "10:15",
                    EstimatedTimeOfDeparture = "On time",
                    Platform = "3",
                    Origin = new[] { new Location { LocationName = "Manchester Piccadilly", Crs = "MAN" } },
                    Destination = new[] { new Location { LocationName = "London Euston", Crs = "EUS" } }
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
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.Host == "huxley2.azurewebsites.net" &&
                    req.RequestUri.ToString().Contains("/departures/MAN/to/EUS")),
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
                    ["TrainChecker:BaseAddress"] = "https://huxley2.azurewebsites.net",
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
        var response = await client.GetAsync("api/v1/trains/MAN/to/EUS");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<HuxleyResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(result);
        Assert.Equal(expectedHuxleyResponse.OriginName, result.OriginName);
        Assert.Equal(expectedHuxleyResponse.Crs, result.Crs);
        Assert.Single(result.TrainServices!);
    }

    [Fact]
    public async Task GetTrainStatusWithParameters_WithInvalidOrigin_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TrainChecker:BaseAddress"] = "https://huxley2.azurewebsites.net",
                    ["TrainChecker:ApiKey"] = "test-api-key",
                    ["Telegram:BotToken"] = "test-bot-token",
                    ["Telegram:ChatId"] = "test-chat-id"
                });
            });
        }).CreateClient();

        // Act - Use a single space character which will be caught by validation
        var response = await client.GetAsync("api/v1/trains/%20/to/EUS");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        // The response should contain the error message from model validation
        Assert.True(content.Contains("The origin field is required") ||
                   content.Contains("Origin station code is required"),
                   $"Expected error message not found in response: {content}");
    }

    [Fact]
    public async Task GetTrainStatusWithParameters_WithInvalidDestination_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TrainChecker:BaseAddress"] = "https://huxley2.azurewebsites.net",
                    ["TrainChecker:ApiKey"] = "test-api-key",
                    ["Telegram:BotToken"] = "test-bot-token",
                    ["Telegram:ChatId"] = "test-chat-id"
                });
            });
        }).CreateClient();

        // Act - Use a single space character which will be caught by validation
        var response = await client.GetAsync("api/v1/trains/MAN/to/%20");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        // The response should contain the error message from model validation
        Assert.True(content.Contains("The destination field is required") ||
                   content.Contains("Destination station code is required"),
                   $"Expected error message not found in response: {content}");
    }

    [Theory]
    [InlineData("eus", "bhm")]
    [InlineData("EUS", "BHM")]
    [InlineData("Eus", "Bhm")]
    public async Task GetTrainStatusWithParameters_WithDifferentCasing_HandlesCorrectly(string origin, string destination)
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var mockTelegramHandler = new Mock<HttpMessageHandler>();

        var expectedHuxleyResponse = new HuxleyResponse
        {
            OriginName = "London Euston",
            Crs = "EUS"
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
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString().Contains("/departures/EUS/to/BHM")), // Should be uppercase
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(huxleyResponse);

        mockTelegramHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(telegramResponse);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TrainChecker:BaseAddress"] = "https://huxley2.azurewebsites.net",
                    ["TrainChecker:ApiKey"] = "test-api-key",
                    ["Telegram:BotToken"] = "test-bot-token",
                    ["Telegram:ChatId"] = "test-chat-id"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.AddHttpClient<INationalRailService, NationalRailService>()
                    .ConfigurePrimaryHttpMessageHandler(() => mockHttpMessageHandler.Object);

                services.AddHttpClient<ITelegramService, TelegramService>()
                    .ConfigurePrimaryHttpMessageHandler(() => mockTelegramHandler.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync($"api/v1/trains/{origin}/to/{destination}");

        // Assert
        response.EnsureSuccessStatusCode();
        
        // Verify that the request was made with uppercase station codes
        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri!.ToString().Contains("/departures/EUS/to/BHM")),
            ItExpr.IsAny<CancellationToken>());
    }
}