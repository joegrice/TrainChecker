using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using TrainChecker.Configuration;
using TrainChecker.Services.Telegram;
using Xunit;

namespace TrainChecker.Tests.Services.Telegram;

public class TelegramServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly TelegramOptions _options;
    private readonly TelegramService _telegramService;
    private readonly NullLogger<TelegramService> _logger;

    public TelegramServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _options = new TelegramOptions
        {
            BotToken = "test-bot-token",
            ChatId = "test-chat-id"
        };
        _logger = new NullLogger<TelegramService>();

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        var optionsWrapper = Options.Create(_options);
        
        _telegramService = new TelegramService(httpClient, optionsWrapper, _logger);
    }

    [Fact]
    public async Task SendMessageAsync_WithValidMessage_SendsCorrectRequest()
    {
        // Arrange
        var message = "Test message";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

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
        await _telegramService.SendMessageAsync(message);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest.Method);
        Assert.Equal($"https://api.telegram.org/bot{_options.BotToken}/sendMessage", capturedRequest.RequestUri!.ToString());
        
        var content = await capturedRequest.Content!.ReadAsStringAsync();
        Assert.Contains($"\"chat_id\":\"{_options.ChatId}\"", content);
        Assert.Contains($"\"text\":\"{message}\"", content);
        Assert.Contains("\"parse_mode\":\"Markdown\"", content);
        
        Assert.Equal("application/json", capturedRequest.Content.Headers.ContentType!.MediaType);
        Assert.Equal(Encoding.UTF8.WebName, capturedRequest.Content.Headers.ContentType.CharSet);
    }

    [Fact]
    public async Task SendMessageAsync_WithSpecialCharacters_EscapesCorrectly()
    {
        // Arrange
        var message = "Test message with *bold* and _italic_";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

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
        await _telegramService.SendMessageAsync(message);

        // Assert
        Assert.NotNull(capturedRequest);
        var content = await capturedRequest.Content!.ReadAsStringAsync();
        Assert.Contains(message, content);
    }

    [Fact]
    public async Task SendMessageAsync_WithHttpError_ThrowsException()
    {
        // Arrange
        var message = "Test message";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _telegramService.SendMessageAsync(message));
    }

    [Fact]
    public void Constructor_SetsCorrectBaseAddress()
    {
        // Arrange & Act
        var httpClient = new HttpClient();
        var optionsWrapper = Options.Create(_options);
        var service = new TelegramService(httpClient, optionsWrapper, _logger);

        // Assert
        Assert.Equal($"https://api.telegram.org/bot{_options.BotToken}/", httpClient.BaseAddress!.ToString());
    }

    [Fact]
    public async Task SendMessageAsync_WithEmptyMessage_SendsEmptyMessage()
    {
        // Arrange
        var message = "";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

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
        await _telegramService.SendMessageAsync(message);

        // Assert
        Assert.NotNull(capturedRequest);
        var content = await capturedRequest.Content!.ReadAsStringAsync();
        Assert.Contains("\"text\":\"\"", content);
    }

    [Fact]
    public async Task SendMessageAsync_WithMultilineMessage_PreservesFormatting()
    {
        // Arrange
        var message = "Line 1\nLine 2\nLine 3";
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

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
        await _telegramService.SendMessageAsync(message);

        // Assert
        Assert.NotNull(capturedRequest);
        var content = await capturedRequest.Content!.ReadAsStringAsync();
        Assert.Contains(message, content);
    }
}