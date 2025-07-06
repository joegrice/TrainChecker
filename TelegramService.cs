using System.Text;
using Microsoft.Extensions.Options;

namespace TrainChecker;

public class TelegramService
{
    private readonly HttpClient _httpClient;
    private readonly TelegramOptions _options;

    public TelegramService(HttpClient httpClient, IOptions<TelegramOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri($"https://api.telegram.org/bot{_options.BotToken}/");
    }

    public async Task SendMessageAsync(string message)
    {
        var content = new StringContent($"{{\"chat_id\":\"{_options.ChatId}\",\"text\":\"{message}\",\"parse_mode\":\"Markdown\"}}", Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("sendMessage", content);
        response.EnsureSuccessStatusCode();
    }
}

