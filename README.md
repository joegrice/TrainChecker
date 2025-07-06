# TrainChecker

This project is a .NET 9.0 application that checks train times and sends notifications via Telegram using scheduled jobs.

## Features

- Scheduled train checking using Quartz.NET
- Telegram notifications for train updates
- RESTful API endpoints for train information
- Docker support for easy deployment
- Configurable departure and arrival stations

## Configuration

To run this application, you need to configure the following values:

- **Telegram:BotToken**: Your Telegram bot token. You can obtain this from BotFather.
- **Telegram:ChatId**: The chat ID where the bot will send messages. You can get this by sending a message to your bot and then accessing `https://api.telegram.org/bot<YOUR_BOT_TOKEN>/getUpdates`.
- **TrainChecker:DepartureStation**: Your departure station code
- **TrainChecker:ArrivalStation**: Your arrival station code

### Configuration Methods

#### 1. Using appsettings.json
```json
{
  "TrainChecker": {
    "DepartureStation": "CHE",
    "ArrivalStation": "VIC"
  },
  "Telegram": {
    "BotToken": "YOUR_TELEGRAM_BOT_TOKEN",
    "ChatId": "YOUR_TELEGRAM_CHAT_ID"
  }
}
```

#### 2. Using Environment Variables (Recommended for Docker)
```bash
TrainChecker__DepartureStation=CHE
TrainChecker__ArrivalStation=VIC
Telegram__BotToken=YOUR_TELEGRAM_BOT_TOKEN
Telegram__ChatId=YOUR_TELEGRAM_CHAT_ID
```

## Running the Application

### Option 1: .NET CLI
Navigate to the `src` directory and execute:
```bash
dotnet run
```

### Option 2: Docker (Recommended)
1. Copy the environment template:
   ```bash
   cp .env.example .env
   ```

2. Edit `.env` with your configuration values

3. Run with Docker Compose:
   ```bash
   docker-compose up --build
   ```

For detailed Docker setup instructions, see [DOCKER.md](DOCKER.md).

## API Endpoints

The application exposes RESTful endpoints for train information:
- `GET /api/train` - Get train information

## Scheduled Jobs

The application runs scheduled jobs to check train times:
- **Forward Journey**: Runs at 7:30 AM on weekdays
- **Return Journey**: Runs at 5:00 PM and 5:30 PM on weekdays

## Development

### Prerequisites
- .NET 9.0 SDK
- Docker (optional, for containerized development)

### Project Structure
- `src/` - Main application code
- `tests/` - Unit and integration tests
- `Dockerfile` - Docker configuration
- `docker-compose.yml` - Docker Compose configuration
