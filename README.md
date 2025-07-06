# TrainChecker

This project is a .NET application that checks train times and sends notifications via Telegram.

## Configuration

To run this application, you need to configure the following secret values:

- **Telegram:BotToken**: Your Telegram bot token. You can obtain this from BotFather.
- **Telegram:ChatId**: The chat ID where the bot will send messages. You can get this by sending a message to your bot and then accessing `https://api.telegram.org/bot<YOUR_BOT_TOKEN>/getUpdates`.
- **TrainChecker:ApiKey**: Your API key for the train information service.

These values should be set in `appsettings.json` or `appsettings.Development.json`, or via environment variables/user secrets for production environments.

Example `appsettings.json` configuration:

```json
{
  "TrainChecker": {
    "DepartureStation": "CHE",
    "ArrivalStation": "VIC",
    "ReverseDepartureStation": "VIC",
    "ReverseArrivalStation": "CHE",
    "Time": "07:30",
    "ApiKey": "YOUR_API_KEY"
  },
  "Telegram": {
    "BotToken": "YOUR_TELEGRAM_BOT_TOKEN",
    "ChatId": "YOUR_TELEGRAM_CHAT_ID"
  }
}
```

The `TrainChecker:ApiKey` is used to access the train information service. You can find the API documentation at: [https://huxley2.azurewebsites.net/](https://huxley2.azurewebsites.net/)


## Running the Application

To run the application, navigate to the `TrainChecker` directory and execute:

```bash
dotnet run
```
