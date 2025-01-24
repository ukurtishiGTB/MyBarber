using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBarber.Data;
using MyBarber.Models;
using MyBarber.ViewModels;

namespace MyBarber.Controllers;

[Authorize] // Ensure all actions in this controller require authentication
public class AppointmentController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public AppointmentController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private List<string> GetAllTimeSlots()
    {
        return new List<string>
        {
            "09:00 AM",
            "10:00 AM",
            "11:00 AM",
            "12:00 PM",
            "01:00 PM",
            "02:00 PM",
            "03:00 PM"
        };
    }
    private int GetCurrentUserId()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return userId.Value; // Since GetInt32 returns a nullable int
    }

    public IActionResult Book(int barberId, DateTime date)
    {
        // Fetch existing appointments for the barber on the selected day
        var existingAppointments = _dbContext.Appointments
            .Where(a => a.BarberId == barberId && a.AppointmentDate.Date == date.Date)
            .Select(a => a.AppointmentDate)
            .ToList();

        // Get all possible time slots for the barber
        var allTimeSlots = GetAllTimeSlots()
            .Select(slot => date.Date.Add(DateTime.Parse(slot).TimeOfDay))
            .ToList();

        // Filter out booked time slots
        var availableTimeSlots = allTimeSlots.Except(existingAppointments).ToList();

        // Ensure availableTimeSlots is not null
        if (availableTimeSlots == null || !availableTimeSlots.Any())
        {
            availableTimeSlots = new List<DateTime>(); // Initialize to an empty list
        }

        // Pass the available slots to the view
        var viewModel = new AppointmentBookingViewModel
        {
            BarberId = barberId,
            Date = date,
            AvailableTimeSlots = availableTimeSlots
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Book(AppointmentBookingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage); // Log the error message
            }
            return View(model); // Return validation errors to the user
        }

        var userId = GetCurrentUserId(); // Ensure session is valid

        // Ensure the selected time slot is valid
        var validTimeSlots = GetAllTimeSlots()
            .Select(slot => model.Date.Date.Add(DateTime.Parse(slot).TimeOfDay))
            .ToList();

        if (!validTimeSlots.Contains(model.SelectedTimeSlot))
        {
            ModelState.AddModelError("", "Invalid time slot selected.");
            return View(model);
        }

        var appointment = new Appointment
        {
            UserId = userId, // Use session UserId
            BarberId = model.BarberId,
            AppointmentDate = model.SelectedTimeSlot,
            Status = AppointmentStatus.Pending
        };

        _dbContext.Appointments.Add(appointment);
        _dbContext.SaveChanges();

        return RedirectToAction("Confirmation");
    }
    public IActionResult Confirmation()
    {
        return View();
    }

    public IActionResult ManageAppointments(int barberId)
    {
        var loggedInBarberId = HttpContext.Session.GetInt32("BarberId");
        if (loggedInBarberId == null || loggedInBarberId != barberId)
        {
            return RedirectToAction("Login", "Barber"); // Ridrejto nëse sesioni është null
        }

        var pendingAppointments = _dbContext.Appointments
            .Where(a => a.BarberId == barberId)
            .Include(a => a.User)
            .ToList();

        return View(pendingAppointments);
    }


    [HttpPost]
    public IActionResult Accepted(int appointmentId)
    {
        var appointment = _dbContext.Appointments.Find(appointmentId);
        if (appointment == null)
        {
            return NotFound("Appointment not found.");
        }

        appointment.Status = AppointmentStatus.Accepted;
        _dbContext.SaveChanges();

        return RedirectToAction("ManageAppointments", new { barberId = appointment.BarberId });
    }

    [HttpPost]
    public IActionResult Rejected(int appointmentId)
    {
        var appointment = _dbContext.Appointments.Find(appointmentId);
        if (appointment == null)
        {
            return NotFound("Appointment not found.");
        }

        appointment.Status = AppointmentStatus.Rejected;
        _dbContext.SaveChanges();

        return RedirectToAction("ManageAppointments", new { barberId = appointment.BarberId });
    }
}