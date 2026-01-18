# TrainChecker Project Context for AI

## Project Overview
TrainChecker is a .NET 9.0 application designed to check train times and send notifications via Telegram using scheduled jobs. It serves as a personal assistant for monitoring train commutes.

## Tech Stack
- **Framework**: .NET 9.0 (ASP.NET Core)
- **Language**: C#
- **Scheduling**: Quartz.NET
- **Containerization**: Docker & Docker Compose
- **Testing**: xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing
- **API Documentation**: Swagger/OpenAPI

## Architecture & Key Components

### Project Structure
- **src/**: Main application source code.
  - **Program.cs**: Application entry point, service registration (DI), and middleware configuration.
  - **Services/**: Core business logic.
    - `TelegramService`: Handles communication with the Telegram Bot API.
    - `NationalRailService`: Interacts with the National Rail (Huxley2) API.
    - `TrainService`: Orchestrates data retrieval and processing.
  - **Jobs/**: Quartz.NET job definitions.
    - `TrainCheckJob`: The main background task that checks train status and triggers notifications.
  - **Controllers/**: REST API endpoints (e.g., for manual checks).
  - **Configuration/**: Strongly-typed configuration classes (Options pattern).

- **tests/**: Unit and integration tests.

### Key Dependencies
- `Quartz.Extensions.Hosting`: For background job scheduling.
- `Telegram.Bot` (or custom HTTP client implementation): For sending alerts.
- `Microsoft.Extensions.Http`: For resilient HTTP requests.

## Configuration
The application uses `appsettings.json` and Environment Variables.

### Critical Settings
- **TrainChecker**:
  - `DepartureStation`: Station code (e.g., "DKG").
  - `ArrivalStation`: Station code (e.g., "VIC").
  - `ApiKey`: Token for the train data provider.
- **Telegram**:
  - `BotToken`: Authentication token for the Telegram bot.
  - `ChatId`: Target chat ID for notifications.
- **Quartz**:
  - Cron schedules for Forward and Return journeys.

## Development Workflow

### Running Locally
```bash
dotnet run --project src/TrainChecker.csproj
```

### Running with Docker
```bash
docker-compose up --build
```

### Running Tests
```bash
dotnet test
```

## Coding Guidelines
- **Style**: Follow standard C# coding conventions.
- **Async/Await**: Use asynchronous programming for all I/O bound operations.
- **Dependency Injection**: Register all services in `Program.cs`.
- **Logging**: Use `ILogger<T>` for logging important events and errors.
- **Error Handling**: Ensure graceful failure, especially in background jobs to prevent crashing the host.

## Common Tasks
- **Adding a new Job**: Create a class implementing `IJob`, register it in `Program.cs` with a trigger.
- **Modifying Notifications**: Update `TelegramService` or the message formatting logic in `TrainCheckJob`.
- **API Changes**: Update `NationalRailService` if the external API contract changes.
