# Docker Setup for TrainChecker

This document provides instructions for running the TrainChecker application using Docker.

## Prerequisites

- Docker installed on your system
- Docker Compose (usually included with Docker Desktop)

## Configuration

Before running the application, you need to configure the settings:

1. Edit `src/appsettings.Production.json` and fill in your configuration:
   ```json
   {
     "TrainChecker": {
       "DepartureStation": "YOUR_DEPARTURE_STATION",
       "ArrivalStation": "YOUR_ARRIVAL_STATION"
     },
     "Telegram": {
       "BotToken": "YOUR_TELEGRAM_BOT_TOKEN",
       "ChatId": "YOUR_TELEGRAM_CHAT_ID"
     }
   }
   ```

## Running with Docker Compose

### Option 1: Build Locally (Development)

1. Build and run the application:
   ```bash
   docker-compose up --build
   ```

2. To run in detached mode (background):
   ```bash
   docker-compose up -d --build
   ```

3. To stop the application:
   ```bash
   docker-compose down
   ```

### Option 2: Use Published Image (Production)

1. Use the production compose file that pulls from DockerHub:
   ```bash
   docker-compose -f docker-compose.prod.yml up -d
   ```

2. To stop the application:
   ```bash
   docker-compose -f docker-compose.prod.yml down
   ```

3. To update to the latest version:
   ```bash
   docker-compose -f docker-compose.prod.yml pull
   docker-compose -f docker-compose.prod.yml up -d
   ```

## Running with Docker directly

1. Build the Docker image:
   ```bash
   docker build -t trainchecker .
   ```

2. Run the container:
   ```bash
   docker run -p 8080:8080 \
     -v $(pwd)/src/appsettings.json:/app/appsettings.json:ro \
     -v $(pwd)/src/appsettings.Production.json:/app/appsettings.Production.json:ro \
     trainchecker
   ```

## Accessing the Application

Once running, the application will be available at:
- http://localhost:8080

## Environment Variables

You can also configure the application using environment variables in the docker-compose.yml file:

```yaml
environment:
  - TrainChecker__DepartureStation=YOUR_DEPARTURE_STATION
  - TrainChecker__ArrivalStation=YOUR_ARRIVAL_STATION
  - Telegram__BotToken=YOUR_TELEGRAM_BOT_TOKEN
  - Telegram__ChatId=YOUR_TELEGRAM_CHAT_ID
```

## Logs

To view application logs:
```bash
docker-compose logs -f trainchecker
```

## Integration with Other Docker Compose Files

If you want to integrate this service into an existing docker-compose setup:

1. **Clone the repository:**
   ```bash
   git clone <your-repo-url>
   cd TrainChecker
   ```

2. **Copy the service definition** from `docker-compose.yml` into your existing docker-compose file:
   ```yaml
   services:
     trainchecker:
       build:
         context: ./TrainChecker  # Path to the cloned repo
         dockerfile: Dockerfile
       ports:
         - "8080:8080"
       env_file:
         - .env  # Will automatically load environment variables from .env file
       environment:
         - ASPNETCORE_ENVIRONMENT=Production
         - ASPNETCORE_URLS=http://+:8080
       restart: unless-stopped
   ```

3. **Set up environment variables:**
   - Copy `.env.example` to `.env` in your main docker-compose directory
   - Fill in your configuration values in the `.env` file

4. **Run your complete stack:**
   ```bash
   docker-compose up --build
   ```

## Environment Variables Setup

Create a `.env` file in your docker-compose directory:
```bash
cp TrainChecker/.env.example .env
```

Then edit the `.env` file with your actual values:
```
TrainChecker__DepartureStation=London Paddington
TrainChecker__ArrivalStation=Reading
TrainChecker__ApiKey=your_huxley2_api_key_here
Telegram__BotToken=1234567890:ABCdefGHIjklMNOpqrsTUVwxyz
Telegram__ChatId=-1001234567890
```

## Troubleshooting

- Ensure all configuration values are properly set in your `.env` file
- Check that port 8080 is not already in use
- Verify Docker and Docker Compose are properly installed
- Make sure the build context path is correct when integrating with other services