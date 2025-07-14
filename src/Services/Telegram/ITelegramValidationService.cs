namespace TrainChecker.Services.Telegram;

public interface ITelegramValidationService
{
    Task<bool> ValidateAndSendMessageAsync(string botToken, long chatId, string message);
}