using Microsoft.AspNetCore.Mvc;
using MyBarber.Data;
using MyBarber.Models;

namespace MyBarber.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        // Fetch a list of barbers (e.g., top 5 based on ratings)
        var barbers = _context.Barbers
    .OrderBy(b => Guid.NewGuid()) // Randomize the order
    .Take(5) // Limit to 5 barbers
    .ToList();


        return View(barbers);
    }
}
