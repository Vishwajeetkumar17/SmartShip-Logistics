/// <summary>
/// Provides backend implementation for TrackingControllerTests.
/// </summary>

using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartShip.Shared.Common.Exceptions;
using SmartShip.TrackingService.Controllers;
using SmartShip.TrackingService.DTOs;
using SmartShip.TrackingService.Services;

namespace SmartShip.TrackingService.UnitTests;

/// <summary>
    /// Represents the tracking controller tests entity or configuration model.
    /// </summary>
    [TestFixture]
/// <summary>
/// Represents TrackingControllerTests.
/// </summary>
public class TrackingControllerTests
{
    private Mock<ITrackingService> _serviceMock = null!;
    private TrackingController _controller = null!;

    /// <summary>
    /// Asynchronously handles the set up process.
    /// </summary>
    [SetUp]
    /// <summary>
    /// Executes the SetUp operation.
    /// </summary>
    public void SetUp()
    {
        _serviceMock = new Mock<ITrackingService>(MockBehavior.Strict);
        _controller = new TrackingController(_serviceMock.Object);
    }

    /// <summary>
    /// Asynchronously handles the get tracking info_returns ok with payload process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the GetTrackingInfo_ReturnsOkWithPayload operation.
    /// </summary>
    public async Task GetTrackingInfo_ReturnsOkWithPayload()
    {
        var response = new TrackingResponseDTO { TrackingNumber = "TRK123", CurrentStatus = "InTransit" };
        _serviceMock.Setup(s => s.GetTrackingInfoAsync("TRK123")).ReturnsAsync(response);

        var result = await _controller.GetTrackingInfo("TRK123");

        var ok = result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        Assert.That(ok!.Value, Is.SameAs(response));
    }

    /// <summary>
    /// Asynchronously handles the add event_calls service and returns ok process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the AddEvent_CallsServiceAndReturnsOk operation.
    /// </summary>
    public async Task AddEvent_CallsServiceAndReturnsOk()
    {
        var dto = new TrackingEventDTO
        {
            TrackingNumber = "TRK123",
            Status = "InTransit",
            Location = "Distribution Center - Delhi",
            Description = "Package Scanned",
            Timestamp = DateTime.Now
        };

        _serviceMock.Setup(s => s.AddTrackingEventAsync(dto)).ReturnsAsync(dto);

        var result = await _controller.AddEvent(dto);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        _serviceMock.Verify(s => s.AddTrackingEventAsync(dto), Times.Once);
    }

    /// <summary>
    /// Asynchronously handles the update status_normalizes tracking number in dto process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the UpdateStatus_NormalizesTrackingNumberInDto operation.
    /// </summary>
    public async Task UpdateStatus_NormalizesTrackingNumberInDto()
    {
        var dto = new StatusUpdateDTO { Status = "Delivered", Location = "Delivery Center - Mumbai", Description = "Delivered to customer" };
        _serviceMock
            .Setup(s => s.UpdateDeliveryStatusAsync(" trk-900 ", It.IsAny<StatusUpdateDTO>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.UpdateStatus(" trk-900 ", dto);

        Assert.That(result, Is.TypeOf<OkResult>());
        Assert.That(dto.TrackingNumber, Is.EqualTo("TRK-900"));
        _serviceMock.Verify(s => s.UpdateDeliveryStatusAsync(" trk-900 ", dto), Times.Once);
    }

    /// <summary>
    /// Asynchronously handles the update status_when tracking number blank_throws validation exception process.
    /// </summary>
    [Test]
    /// <summary>
    /// Executes the UpdateStatus_WhenTrackingNumberBlank_ThrowsValidationException operation.
    /// </summary>
    public void UpdateStatus_WhenTrackingNumberBlank_ThrowsValidationException()
    {
        var dto = new StatusUpdateDTO { Status = "Delivered", Location = "Delivery Center - Bangalore" };

        var ex = Assert.ThrowsAsync<RequestValidationException>(async () => await _controller.UpdateStatus("  ", dto));

        Assert.That(ex!.Message, Does.Contain("Tracking number is required"));
    }
}


