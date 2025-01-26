using Microsoft.AspNetCore.Mvc;
using MyBarber.Data;
using MyBarber.Models;
using MyBarber.ViewModels;
using Microsoft.AspNetCore.Http;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class LoginController : Controller
{
    private readonly ApplicationDbContext _context;

    public LoginController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Kthimi i view-it të login-it
    [HttpGet("Index")]
    public IActionResult Index()
    {
        return View();
    }

    // Menaxhimi i postimit të formularit të login-it
    [HttpPost("Index")]
    [ValidateAntiForgeryToken]
    public IActionResult Index(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Kontrollojmë nëse është përdoruesi apo berberi
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            var barber = _context.Barbers.FirstOrDefault(b => b.Email == model.Email);

            // Logimi i përdoruesit
            if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                try
                {
                    // Verifikimi i ID për përdoruesin
                    if (user.Id <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(user.Id), "User ID is out of the valid range.");
                    }

                    // Ruaj ID-në e përdoruesit në sesion
                    HttpContext.Session.SetInt32("UserId", user.Id);

                    // Krijimi i Claims për përdoruesin
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // User ID
                        new Claim(ClaimTypes.Name, user.Email) // User email
                    };

                    // Krijimi i Claims Identity
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Kyçja e përdoruesit me Authentication Cookie
                        HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe // Përdorimi i "Remember Me" për të mbajtur sesionin aktiv
                        });

                    Console.WriteLine($"Logging in user with ID: {user?.Id}, Email: {user?.Email}");
                    // Redirektoni në faqen kryesore
                    return RedirectToAction("Index", "Home");
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    // Logimi i gabimeve për debugging
                    Console.WriteLine($"Error during login: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                    return View(model);
                }
                catch (Exception ex)
                {
                    // Ndjekja e gabimeve të papritura
                    Console.WriteLine($"Unexpected error during login: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                    return View(model);
                }
            }
            // Logimi i berberit
            else if (barber != null && BCrypt.Net.BCrypt.Verify(model.Password, barber.HashPassword))
            {
                // Ruaj ID dhe Emrin në sesion për berberin
                HttpContext.Session.SetInt32("BarberId", barber.Id);
                HttpContext.Session.SetString("BarberName", barber.Name);

                // Redirektoni në faqen kryesore për berberin
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }
        }

        return View(model); // Ktheje nëse ka gabime
    }

    // Funksioni për logout (pastrimi i sesionit)
    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); // Pastron të dhënat e sesionit
        return RedirectToAction("Login", "Login"); // Ktheje në formularin e login-it
    }
}
