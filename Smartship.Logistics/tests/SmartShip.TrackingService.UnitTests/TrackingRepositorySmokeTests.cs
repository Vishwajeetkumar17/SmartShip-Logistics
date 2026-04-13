/// <summary>
/// Provides backend implementation for TrackingRepositorySmokeTests.
/// </summary>

using Microsoft.EntityFrameworkCore;
using SmartShip.TrackingService.Data;
using SmartShip.TrackingService.Models;
using SmartShip.TrackingService.Repositories;

namespace SmartShip.TrackingService.UnitTests;

/// <summary>
    /// Represents the tracking repository smoke tests entity or configuration model.
    /// </summary>
    [TestFixture]
/// <summary>
/// Represents TrackingRepositorySmokeTests.
/// </summary>
public class TrackingRepositorySmokeTests
{
    private static TrackingRepository CreateRepository(out TrackingDbContext context)
    {
        var options = new DbContextOptionsBuilder<TrackingDbContext>()
            .UseInMemoryDatabase(databaseName: $"tracking-smoke-{Guid.NewGuid()}")
            .Options;

        context = new TrackingDbContext(options);
        return new TrackingRepository(context);
    }

    /// <summary>
    /// Asynchronously handles the add event async_should store all shipment stages including repeated transit process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the AddEventAsync_ShouldStoreAllShipmentStagesIncludingRepeatedTransit operation.
    /// </summary>
    public async Task AddEventAsync_ShouldStoreAllShipmentStagesIncludingRepeatedTransit()
    {
        var repository = CreateRepository(out var context);
        var trackingNumber = "SS-SMOKE-1001";
        var baseTime = DateTime.UtcNow.AddMinutes(-15);

        var events = new[]
        {
            new TrackingEvent { TrackingNumber = trackingNumber, Status = "Booked", Location = "Hub A", Description = "Shipment booked by admin", Timestamp = baseTime },
            new TrackingEvent { TrackingNumber = trackingNumber, Status = "PickedUp", Location = "Sender Address", Description = "Shipment picked up from sender", Timestamp = baseTime.AddMinutes(2) },
            new TrackingEvent { TrackingNumber = trackingNumber, Status = "InTransit", Location = "Hub A", Description = "Shipment moved through transit hub", Timestamp = baseTime.AddMinutes(4) },
            new TrackingEvent { TrackingNumber = trackingNumber, Status = "InTransit", Location = "Hub B", Description = "Shipment moved through transit hub", Timestamp = baseTime.AddMinutes(6) },
            new TrackingEvent { TrackingNumber = trackingNumber, Status = "OutForDelivery", Location = "Hub B", Description = "Shipment is out for delivery", Timestamp = baseTime.AddMinutes(8) },
            new TrackingEvent { TrackingNumber = trackingNumber, Status = "Delivered", Location = "Receiver Address", Description = "Shipment delivered to receiver", Timestamp = baseTime.AddMinutes(10) }
        };

        foreach (var trackingEvent in events)
        {
            await repository.AddEventAsync(trackingEvent);
        }

        var stored = await repository.GetEventsAsync(trackingNumber);

        Assert.That(stored.Count, Is.EqualTo(6), "All stage events should be stored.");
        Assert.That(stored.Count(e => e.Status == "InTransit"), Is.EqualTo(2), "Repeated InTransit hops should be retained.");
        Assert.That(stored.Any(e => e.Status == "OutForDelivery"), Is.True, "OutForDelivery stage should be present.");
        Assert.That(stored.Any(e => e.Status == "Booked"), Is.True, "Booked stage should be present.");

        await context.DisposeAsync();
    }

    /// <summary>
    /// Asynchronously handles the add event async_should ignore near identical duplicate event process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the AddEventAsync_ShouldIgnoreNearIdenticalDuplicateEvent operation.
    /// </summary>
    public async Task AddEventAsync_ShouldIgnoreNearIdenticalDuplicateEvent()
    {
        var repository = CreateRepository(out var context);
        var when = DateTime.UtcNow;

        await repository.AddEventAsync(new TrackingEvent
        {
            TrackingNumber = "SS-SMOKE-1002",
            Status = "InTransit",
            Location = "Hub A",
            Description = "Shipment moved through transit hub",
            Timestamp = when
        });

        await repository.AddEventAsync(new TrackingEvent
        {
            TrackingNumber = "SS-SMOKE-1002",
            Status = "InTransit",
            Location = "Hub A",
            Description = "Shipment moved through transit hub",
            Timestamp = when.AddMilliseconds(600)
        });

        var stored = await repository.GetEventsAsync("SS-SMOKE-1002");
        Assert.That(stored.Count, Is.EqualTo(1), "Near-identical duplicate should be suppressed.");

        await context.DisposeAsync();
    }
}


