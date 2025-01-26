﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var barbers = _context.Barbers
                .Select(b => new BarberListViewModel
                {
                    Id = b.Id,
                    Name = b.Name,
                    Location = b.Location,
                    PhoneNumber = b.PhoneNumber
                })
                .ToList();

            return View(barbers);
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
                TempData["LoginMessage"] = "Welcome to the platform, " + barber.Name + "!";

                // Redirect to My Account page
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        /*
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
        */
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
        [Route("Dashboard")]
        public IActionResult Dashboard()
        {
            var barberId = HttpContext.Session.GetInt32("BarberId");
            if (barberId == null) return RedirectToAction("Login");

            var barber = _context.Barbers.FirstOrDefault(b => b.Id == barberId);
            return View(barber);
        }
        [HttpGet("Details/{id}")]
        public IActionResult Details(int id)
        {
            var barber = _context.Barbers
                .Include(b => b.Ratings) // Include Ratings
                .FirstOrDefault(b => b.Id == id);

            if (barber == null)
            {
                return NotFound("Barber not found.");
            }

            return View(barber);
        }

        [HttpGet("Search")]
        public IActionResult Search(string query)
        {
            var barbers = _context.Barbers.AsQueryable();

            if (!string.IsNullOrEmpty(query))
            {
                // Search by multiple fields using a single query
                barbers = barbers.Where(b =>
                    b.Name.Contains(query) ||               // Search by barber name
                    b.Location.Contains(query) ||           // Search by location
                    b.Services.Contains(query) ||           // Search by services
                    b.Rating.ToString().Contains(query)     // Search by rating (optional for numeric input)
                );
            }

            return View(barbers.ToList());
        }
        [HttpGet]
        public IActionResult Ratings(int barberId)
        {
            var barber = _context.Barbers
                .Include(b => b.Ratings)
                .FirstOrDefault(b => b.Id == barberId);

            if (barber == null)
            {
                return NotFound("Barber not found.");
            }

            // Map the data to the view model
            var viewModel = new RatingsViewModel
            {
                BarberId = barber.Id,
                BarberName = barber.Name,
                BarberLocation = barber.Location,
                BarberPhoneNumber = barber.PhoneNumber,
                BarberRating = barber.Rating,
                BarberNumberOfRatings = barber.NumberOfRatings,
                Ratings = barber.Ratings.Select(r => new RatingViewModel
                {
                    Stars = r.Stars,
                    Comment = r.Comment
                }).ToList()
            };

            return View(viewModel); // Pass the strongly-typed view model to the view
        }


        [HttpGet("SubmitRating")]
        public IActionResult SubmitRating(int id) // `id` is the BarberId
        {
            var barber = _context.Barbers.FirstOrDefault(b => b.Id == id);
            if (barber == null)
            {
                return NotFound("Barber not found.");
            }

            var model = new RatingViewModel
            {
                BarberId = id
            };

            return View(model); // Render the view with the barber's ID
        }

        [HttpPost("SubmitRating")]
        public IActionResult SubmitRating(RatingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid rating data.");
            }

            var barber = _context.Barbers.FirstOrDefault(b => b.Id == model.BarberId);
            if (barber == null)
            {
                return NotFound("Barber not found.");
            }

            // Add the new rating
            var rating = new Rating
            {
                BarberId = model.BarberId,
                UserId = HttpContext.Session.GetInt32("UserId").Value, // Current user
                Stars = model.Stars,
                Comment = model.Comment
            };
            _context.Ratings.Add(rating);

            // Update barber's average rating
            barber.NumberOfRatings++;
            barber.Rating = ((barber.Rating * (barber.NumberOfRatings - 1)) + model.Stars) / barber.NumberOfRatings;

            _context.SaveChanges();

            return RedirectToAction("Ratings", new { barberId = model.BarberId });
        }
    }


}

