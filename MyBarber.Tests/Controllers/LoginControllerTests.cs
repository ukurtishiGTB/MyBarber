using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBarber.Data;
using MyBarber.Models;
using MyBarber.ViewModels;
using Xunit;

namespace MyBarber.Tests.Controllers;

public class LoginControllerTests
{
    private ApplicationDbContext GetTestDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestLoginDb")
            .Options;

        var context = new ApplicationDbContext(options);
        
        if (!context.Users.Any())
        {
            context.Users.Add(new User
            {
                Id = 1,
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                isActive = true
            });
            
            context.Barbers.Add(new Barber
            {
                Id = 1,
                Name = "Test Barber",
                Email = "barber@example.com",
                HashPassword = BCrypt.Net.BCrypt.HashPassword("barber123"),
                Location = "Test Location",
                Services = "Haircut",
                PhoneNumber = "1234567890" // Added missing required field
            });
            
            context.SaveChanges();
        }
        
        return context;
    }

    private LoginController SetupController()
    {
        var context = GetTestDbContext();
        var controller = new LoginController(context);
        
        // Setup Session
        var httpContext = new DefaultHttpContext();
        var sessionMock = new TestSession();
        httpContext.Session = sessionMock;
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        return controller;
    }

    // Test Session implementation
    private class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();
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

    [Fact]
    public void Index_Get_ReturnsView()
    {
        // Arrange
        var controller = SetupController();

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
    }


    [Fact]
public void Index_Post_ValidUserCredentials_ReturnsRedirectToHome()
{
    // Arrange
    var controller = SetupController();
    var loginModel = new LoginViewModel
    {
        Email = "test@example.com",
        Password = "password123",
        RememberMe = false
    };

    // Set ModelState to valid
    controller.ModelState.Clear();

    // Act
    var result = controller.Index(loginModel);

    // Assert
    var redirectResult = Assert.IsType<RedirectToActionResult>(result);
    Assert.Equal("Index", redirectResult.ActionName);
    Assert.Equal("Home", redirectResult.ControllerName);
}
    [Fact]
    public async Task Index_Post_ValidBarberCredentials_ReturnsRedirectToHome()
    {
        // Arrange
        var controller = SetupController();
        var loginModel = new LoginViewModel
        {
            Email = "barber@example.com",
            Password = "barber123",
            RememberMe = false
        };

        // Act
        var result = controller.Index(loginModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);
    }

    [Fact]
    public void Logout_ClearsSessionAndRedirectsToLogin()
    {
        // Arrange
        var controller = SetupController();

        // Act
        var result = controller.Logout();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Login", redirectResult.ControllerName);
    }
}