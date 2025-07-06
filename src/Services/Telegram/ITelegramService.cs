namespace TrainChecker.Services.Telegram;

public interface ITelegramService
{
    Task SendMessageAsync(string message);
}