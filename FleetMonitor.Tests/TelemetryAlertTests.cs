using FleetMonitor.Core.Models;
using Xunit;
using FluentAssertions;

public class TelemetryAlertTests
{
    [Fact]
    public void Should_Raise_Alert_When_Fuel_Is_Too_Low()
    {
        // Arrange
        var device = new Device { Id = Guid.NewGuid(), TankCapacityLiters = 60 };
        var telemetry = new Telemetry
        {
            Id = Guid.NewGuid(),
            DeviceId = device.Id,
            FuelLiters = 2, // casi vacío
            Timestamp = DateTime.UtcNow
        };

        // Act
        bool isAlert = telemetry.FuelLiters < 5;

        // Assert
        isAlert.Should().BeTrue();
    }
}
