using System.Text;
using Microsoft.Extensions.Options;
using TrainChecker.Configuration;

namespace TrainChecker.Services.Telegram;

public class TelegramService : ITelegramService
{
    private readonly HttpClient _httpClient;
    private readonly TelegramOptions _options;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(HttpClient httpClient, IOptions<TelegramOptions> options, ILogger<TelegramService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri($"https://api.telegram.org/bot{_options.BotToken}/");
    }

    public async Task SendMessageAsync(string message)
    {
        _logger.LogInformation("Sending Telegram message.");
        var content = new StringContent($"{{\"chat_id\":\"{_options.ChatId}\",\"text\":\"{message}\",\"parse_mode\":\"Markdown\"}}", Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("sendMessage", content);
        response.EnsureSuccessStatusCode();
    }
}

