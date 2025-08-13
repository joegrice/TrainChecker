using System.Text.Json;
using TrainChecker.Models;
using Xunit;

namespace TrainChecker.Tests.Models;

public class TrainServicesTests
{
    [Fact]
    public void HuxleyResponse_CanBeSerializedAndDeserialized()
    {
        // Arrange
        var original = new HuxleyResponse
        {
            OriginName = "London Euston",
            Crs = "EUS",
            TrainServices =
            [
                new TrainService
                {
                    ScheduledTimeOfDeparture = "08:30",
                    EstimatedTimeOfDeparture = "On time",
                    Platform = "1",
                    Operator = "Avanti West Coast",
                    Origin = [new Location { LocationName = "London Euston", Crs = "EUS" }],
                    Destination = [new Location { LocationName = "Birmingham New Street", Crs = "BHM" }],
                    Length = 5
                }
            ]
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<HuxleyResponse>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.OriginName, deserialized.OriginName);
        Assert.Equal(original.Crs, deserialized.Crs);
        Assert.Single(deserialized.TrainServices!);
        
        var originalService = original.TrainServices[0];
        var deserializedService = deserialized.TrainServices[0];
        
        Assert.Equal(originalService.ScheduledTimeOfDeparture, deserializedService.ScheduledTimeOfDeparture);
        Assert.Equal(originalService.EstimatedTimeOfDeparture, deserializedService.EstimatedTimeOfDeparture);
        Assert.Equal(originalService.Platform, deserializedService.Platform);
        Assert.Equal(originalService.Operator, deserializedService.Operator);
        Assert.Equal(originalService.Length, deserializedService.Length);
    }

    [Fact]
    public void TrainService_WithNullOriginAndDestination_HandlesGracefully()
    {
        // Arrange
        var trainService = new TrainService
        {
            ScheduledTimeOfDeparture = "08:30",
            EstimatedTimeOfDeparture = "On time",
            Platform = "1",
            Origin = null,
            Destination = null
        };

        // Act & Assert
        Assert.Null(trainService.Origin);
        Assert.Null(trainService.Destination);
        Assert.Equal("08:30", trainService.ScheduledTimeOfDeparture);
        Assert.Equal("On time", trainService.EstimatedTimeOfDeparture);
        Assert.Equal("1", trainService.Platform);
    }

    [Fact]
    public void TrainService_WithEmptyOriginAndDestination_HandlesGracefully()
    {
        // Arrange
        var trainService = new TrainService
        {
            ScheduledTimeOfDeparture = "08:30",
            EstimatedTimeOfDeparture = "On time",
            Platform = "1",
            Origin = [],
            Destination = []
        };

        // Act & Assert
        Assert.Empty(trainService.Origin);
        Assert.Empty(trainService.Destination);
    }

    [Fact]
    public void Location_CanBeCreatedWithAllProperties()
    {
        // Arrange & Act
        var location = new Location
        {
            LocationName = "London Euston",
            Crs = "EUS"
        };

        // Assert
        Assert.Equal("London Euston", location.LocationName);
        Assert.Equal("EUS", location.Crs);
    }

    [Fact]
    public void HuxleyResponse_WithNullTrainServices_HandlesGracefully()
    {
        // Arrange
        var response = new HuxleyResponse
        {
            OriginName = "London Euston",
            Crs = "EUS",
            TrainServices = null
        };

        // Act & Assert
        Assert.Equal("London Euston", response.OriginName);
        Assert.Equal("EUS", response.Crs);
        Assert.Null(response.TrainServices);
    }

    [Fact]
    public void TrainService_WithAllNullProperties_HandlesGracefully()
    {
        // Arrange & Act
        var trainService = new TrainService();

        // Assert
        Assert.Null(trainService.ScheduledTimeOfDeparture);
        Assert.Null(trainService.EstimatedTimeOfDeparture);
        Assert.Null(trainService.Platform);
        Assert.Null(trainService.Operator);
        Assert.Null(trainService.Origin);
        Assert.Null(trainService.Destination);
    }

    [Fact]
    public void JsonSerialization_UsesCorrectPropertyNames()
    {
        // Arrange
        var response = new HuxleyResponse
        {
            OriginName = "Test Location",
            Crs = "TST",
            TrainServices =
            [
                new TrainService
                {
                    ScheduledTimeOfDeparture = "08:30",
                    EstimatedTimeOfDeparture = "On time",
                    Platform = "1"
                }
            ]
        };

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        Assert.Contains("\"locationName\":", json);
        Assert.Contains("\"crs\":", json);
        Assert.Contains("\"trainServices\":", json);
        Assert.Contains("\"std\":", json);
        Assert.Contains("\"etd\":", json);
        Assert.Contains("\"platform\":", json);
    }

    [Fact]
    public void JsonDeserialization_HandlesApiResponse()
    {
        // Arrange
        var json = """
        {
            "locationName": "London Euston",
            "crs": "EUS",
            "trainServices": [
                {
                    "std": "08:30",
                    "etd": "On time",
                    "platform": "1",
                    "operator": "Avanti West Coast",
                    "origin": [
                        {
                            "locationName": "London Euston",
                            "crs": "EUS"
                        }
                    ],
                    "destination": [
                        {
                            "locationName": "Birmingham New Street",
                            "crs": "BHM"
                        }
                    ]
                }
            ]
        }
        """;

        // Act
        var response = JsonSerializer.Deserialize<HuxleyResponse>(json);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("London Euston", response.OriginName);
        Assert.Equal("EUS", response.Crs);
        Assert.Single(response.TrainServices!);
        
        var service = response.TrainServices[0];
        Assert.Equal("08:30", service.ScheduledTimeOfDeparture);
        Assert.Equal("On time", service.EstimatedTimeOfDeparture);
        Assert.Equal("1", service.Platform);
        Assert.Equal("Avanti West Coast", service.Operator);
        Assert.Single(service.Origin!);
        Assert.Single(service.Destination!);
        Assert.Equal("London Euston", service.Origin[0].LocationName);
        Assert.Equal("Birmingham New Street", service.Destination[0].LocationName);
    }
}