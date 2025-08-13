using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TrainChecker.Services.Telegram;

public class TelegramValidationService : ITelegramValidationService
{
    private readonly ILogger<TelegramValidationService> _logger;

    public TelegramValidationService(ILogger<TelegramValidationService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ValidateAndSendMessageAsync(string botToken, long chatId, string message)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri($"https://api.telegram.org/bot{botToken}/");

            _logger.LogInformation($"Attempting to send test Telegram message to chat ID: {chatId}");
            var content = new StringContent($"{{\"chat_id\":\"{chatId}\",\"text\":\"{message}\",\"parse_mode\":\"Markdown\"}}", Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("sendMessage", content);

            response.EnsureSuccessStatusCode(); // Throws an exception if not 2xx
            _logger.LogInformation("Test Telegram message sent successfully.");
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Failed to send test Telegram message. HTTP Request Error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An unexpected error occurred while sending test Telegram message: {ex.Message}");
            return false;
        }
    }
}