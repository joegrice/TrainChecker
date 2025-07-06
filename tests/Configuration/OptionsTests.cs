using TrainChecker.Configuration;
using Xunit;

namespace TrainChecker.Tests.Configuration;

public class OptionsTests
{
    [Fact]
    public void TrainCheckerOptions_HasCorrectSectionName()
    {
        // Assert
        Assert.Equal("TrainChecker", TrainCheckerOptions.TrainChecker);
    }

    [Fact]
    public void TrainCheckerOptions_DefaultValues_AreEmpty()
    {
        // Arrange & Act
        var options = new TrainCheckerOptions();

        // Assert
        Assert.Equal(string.Empty, options.DepartureStation);
        Assert.Equal(string.Empty, options.ArrivalStation);
        Assert.Equal(string.Empty, options.ApiKey);
        Assert.Equal(string.Empty, options.Platform);
    }

    [Fact]
    public void TrainCheckerOptions_CanSetAllProperties()
    {
        // Arrange
        var options = new TrainCheckerOptions();

        // Act
        options.DepartureStation = "LDN";
        options.ArrivalStation = "BHM";
        options.ApiKey = "test-api-key";
        options.Platform = "1";

        // Assert
        Assert.Equal("LDN", options.DepartureStation);
        Assert.Equal("BHM", options.ArrivalStation);
        Assert.Equal("test-api-key", options.ApiKey);
        Assert.Equal("1", options.Platform);
    }

    [Fact]
    public void TelegramOptions_HasCorrectSectionName()
    {
        // Assert
        Assert.Equal("Telegram", TelegramOptions.Telegram);
    }

    [Fact]
    public void TelegramOptions_DefaultValues_AreEmpty()
    {
        // Arrange & Act
        var options = new TelegramOptions();

        // Assert
        Assert.Equal(string.Empty, options.BotToken);
        Assert.Equal(string.Empty, options.ChatId);
    }

    [Fact]
    public void TelegramOptions_CanSetAllProperties()
    {
        // Arrange
        var options = new TelegramOptions();

        // Act
        options.BotToken = "test-bot-token";
        options.ChatId = "test-chat-id";

        // Assert
        Assert.Equal("test-bot-token", options.BotToken);
        Assert.Equal("test-chat-id", options.ChatId);
    }

    [Fact]
    public void TrainCheckerOptions_CanBeInitializedWithObjectInitializer()
    {
        // Arrange & Act
        var options = new TrainCheckerOptions
        {
            DepartureStation = "LDN",
            ArrivalStation = "BHM",
            ApiKey = "test-api-key",
            Platform = "1"
        };

        // Assert
        Assert.Equal("LDN", options.DepartureStation);
        Assert.Equal("BHM", options.ArrivalStation);
        Assert.Equal("test-api-key", options.ApiKey);
        Assert.Equal("1", options.Platform);
    }

    [Fact]
    public void TelegramOptions_CanBeInitializedWithObjectInitializer()
    {
        // Arrange & Act
        var options = new TelegramOptions
        {
            BotToken = "test-bot-token",
            ChatId = "test-chat-id"
        };

        // Assert
        Assert.Equal("test-bot-token", options.BotToken);
        Assert.Equal("test-chat-id", options.ChatId);
    }

    [Fact]
    public void TrainCheckerOptions_PropertiesCanBeSetToNull()
    {
        // Arrange
        var options = new TrainCheckerOptions
        {
            DepartureStation = "LDN",
            ArrivalStation = "BHM",
            ApiKey = "test-api-key",
            Platform = "1"
        };

        // Act
        options.DepartureStation = null!;
        options.ArrivalStation = null!;
        options.ApiKey = null!;
        options.Platform = null!;

        // Assert
        Assert.Null(options.DepartureStation);
        Assert.Null(options.ArrivalStation);
        Assert.Null(options.ApiKey);
        Assert.Null(options.Platform);
    }

    [Fact]
    public void TelegramOptions_PropertiesCanBeSetToNull()
    {
        // Arrange
        var options = new TelegramOptions
        {
            BotToken = "test-bot-token",
            ChatId = "test-chat-id"
        };

        // Act
        options.BotToken = null!;
        options.ChatId = null!;

        // Assert
        Assert.Null(options.BotToken);
        Assert.Null(options.ChatId);
    }
}