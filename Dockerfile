# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY src/*.csproj ./src/
RUN dotnet restore ./src/TrainChecker.csproj

# Copy everything else and build
COPY . .
WORKDIR /app/src
RUN dotnet publish -c Release -o out --no-restore

# Use the official .NET 9.0 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' --shell /bin/bash --uid 1001 appuser

# Copy the published app from the build stage
COPY --from=build /app/src/out .

# Change ownership of the app directory to the non-root user
RUN chown -R appuser:appuser /app

# Switch to the non-root user
USER appuser

# Expose the port the app runs on
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["dotnet", "TrainChecker.dll"]