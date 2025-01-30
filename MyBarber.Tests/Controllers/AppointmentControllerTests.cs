using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBarber.Controllers;
using MyBarber.Data;
using MyBarber.Models;
using MyBarber.ViewModels;
using Xunit;

namespace MyBarber.Tests.Controllers;

public class AppointmentControllerTests
{
    private ApplicationDbContext GetTestDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestAppointmentsDb")
            .Options;

        return new ApplicationDbContext(options);
    }

    private AppointmentController SetupController()
    {
        var context = GetTestDbContext();
        var controller = new AppointmentController(context);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Session = new TestSession();
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public void Book_ReturnsViewWithAvailableTimeSlots()
    {
        // Arrange
        var controller = SetupController();
        var barberId = 1;
        var date = DateTime.Today;

        // Act
        var result = controller.Book(barberId, date);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<AppointmentBookingViewModel>(viewResult.Model);
        Assert.Equal(barberId, model.BarberId);
        Assert.NotNull(model.AvailableTimeSlots);
    }
}

// Helper class for testing sessions
public class TestSession : ISession
{
    private Dictionary<string, byte[]> _store = new();

    public string Id => "testSession";
    public bool IsAvailable => true;
    public IEnumerable<string> Keys => _store.Keys;

    public void Clear() => _store.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void Remove(string key) => _store.Remove(key);
    public void Set(string key, byte[] value) => _store[key] = value;
    public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
}