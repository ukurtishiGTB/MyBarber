using Microsoft.AspNetCore.Mvc;
using MyBarber.Data;
using MyBarber.Models;
using MyBarber.ViewModels;

namespace MyBarber.Controllers
{
    [Route("Barber")]
    public class BarberController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BarberController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(BarberRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var barber = new Barber
                {
                    Name = model.Name,
                    Email = model.Email,
                    HashPassword = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Location = model.Location,
                    PhoneNumber = model.PhoneNumber,
                    Pricing = model.Pricing,
                    ProfileImage = model.ProfileImage,
                    Services = model.Services,
                    isActive = true
                };

                _context.Barbers.Add(barber);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetInt32("BarberId", barber.Id);
                HttpContext.Session.SetString("BarberName", barber.Name);

                // Redirect to My Account page
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("Login")]
        public IActionResult Login(BarberLoginViewModel model)
        {
            var barber = _context.Barbers.FirstOrDefault(b => b.Email == model.Email);
            if (barber != null && BCrypt.Net.BCrypt.Verify(model.Password, barber.HashPassword))
            {
                HttpContext.Session.SetInt32("BarberId", barber.Id);
                HttpContext.Session.SetString("BarberName", barber.Name);
                return RedirectToAction("Index","Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt");
            return View(model);
        }
        [Route("MyAccount")]
        public IActionResult MyAccount()
        {
            var barberId = HttpContext.Session.GetInt32("BarberId");
            if (barberId == null)
            {
                return RedirectToAction("Login");
            }

            var barber = _context.Barbers.FirstOrDefault(b => b.Id == barberId);
            if (barber == null)
            {
                return RedirectToAction("Login");
            }

            return View(barber);
        }

    }

}
