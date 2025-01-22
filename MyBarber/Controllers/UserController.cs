using Microsoft.AspNetCore.Mvc;
using MyBarber.Data;
using MyBarber.Models;
using MyBarber.ViewModels;

namespace MyBarber.Controllers
{
    [Route("User")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    isActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.Name);
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
        public IActionResult Login(UserLoginViewModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.Name);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt");
            return View(model);
        }
        [Route("MyAccount")]
        public IActionResult MyAccount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

    }
}
