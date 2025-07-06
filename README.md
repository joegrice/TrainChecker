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
- **TrainChecker:ApiKey**: Your API key for the train information service (see [Huxley2 API](https://huxley2.azurewebsites.net/))

### Configuration Methods

#### 1. Using appsettings.json
```json
{
  "TrainChecker": {
    "DepartureStation": "DKG",
    "ArrivalStation": "VIC",
    "ApiKey": "YOUR_API_KEY"
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
TrainChecker__ApiKey=YOUR_API_KEY
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
   # For local development (builds from source)
   docker-compose up --build
   
   # For production (uses published image)
   docker-compose -f docker-compose.prod.yml up -d
   ```

For detailed Docker setup instructions, see [DOCKER.md](DOCKER.md).

### Option 3: Using Published Docker Image
You can also use the pre-built image from Docker Hub:
```bash
docker pull joegrice/trainchecker:latest
docker run -d --name trainchecker -p 8080:8080 --env-file .env joegrice/trainchecker:latest
```

For GitHub Actions setup to automatically publish releases, see [GITHUB_ACTIONS.md](GITHUB_ACTIONS.md).

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
