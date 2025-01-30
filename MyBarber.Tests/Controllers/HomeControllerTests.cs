using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBarber.Controllers;
using MyBarber.Data;
using MyBarber.Models;
using Xunit;

namespace MyBarber.Tests.Controllers;

public class HomeControllerTests
{
    private ApplicationDbContext GetTestDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestMyBarberDb")
            .Options;

        var context = new ApplicationDbContext(options);
        return context;
    }

    [Fact]
    public void Index_ReturnsViewWithBarbers()
    {
        // Arrange
        var dbContext = GetTestDbContext();
        var controller = new HomeController(dbContext);

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Barber>>(viewResult.Model);
    }
}