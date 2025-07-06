namespace TrainChecker.Options;

public class TelegramOptions
{
    public const string Telegram = "Telegram";

    public string BotToken { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
}
