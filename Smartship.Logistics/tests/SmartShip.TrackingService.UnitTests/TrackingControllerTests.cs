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

[TestFixture]
/// <summary>
/// Represents TrackingControllerTests.
/// </summary>
public class TrackingControllerTests
{
    private Mock<ITrackingService> _serviceMock = null!;
    private TrackingController _controller = null!;

    [SetUp]
    /// <summary>
    /// Executes SetUp.
    /// </summary>
    public void SetUp()
    {
        _serviceMock = new Mock<ITrackingService>(MockBehavior.Strict);
        _controller = new TrackingController(_serviceMock.Object);
    }

    [Test]
    /// <summary>
    /// Executes GetTrackingInfo_ReturnsOkWithPayload.
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

    [Test]
    /// <summary>
    /// Executes AddEvent_CallsServiceAndReturnsOk.
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

    [Test]
    /// <summary>
    /// Executes UpdateStatus_NormalizesTrackingNumberInDto.
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

    [Test]
    /// <summary>
    /// Executes UpdateStatus_WhenTrackingNumberBlank_ThrowsValidationException.
    /// </summary>
    public void UpdateStatus_WhenTrackingNumberBlank_ThrowsValidationException()
    {
        var dto = new StatusUpdateDTO { Status = "Delivered", Location = "Delivery Center - Bangalore" };

        var ex = Assert.ThrowsAsync<RequestValidationException>(async () => await _controller.UpdateStatus("  ", dto));

        Assert.That(ex!.Message, Does.Contain("Tracking number is required"));
    }
}


