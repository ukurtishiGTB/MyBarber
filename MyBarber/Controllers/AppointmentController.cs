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
        var availableDates = Enumerable
            .Range(0, 5) // Generate numbers 0 to 4
            .Select(days => DateTime.Now.Date.AddDays(days)) // Add days to today's date
            .ToList();

        // Filter out booked time slots
        var availableTimeSlots = allTimeSlots.Except(existingAppointments).ToList();

        // Ensure availableTimeSlots is not null
        if (availableTimeSlots == null || !availableTimeSlots.Any())
        {
            availableTimeSlots = new List<DateTime>(); // Initialize to an empty list
        }

        if (availableDates == null || !availableDates.Any())
        {
            availableTimeSlots = new List<DateTime>(); // Initialize to an empty list
        }

        // Pass the available slots to the view
        var viewModel = new AppointmentBookingViewModel
        {
            BarberId = barberId,
            Date = date,
            AvailableTimeSlots = availableTimeSlots,
            AvailableDates = availableDates
        };

        return View(viewModel);
    }

   [HttpPost]
public IActionResult Book(AppointmentBookingViewModel model)
{
    if (!ModelState.IsValid)
    {
        Console.WriteLine("Validation Errors:");
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine(error.ErrorMessage); // Log the error message
        }

        // Regenerate available dates and time slots for redisplaying the form
        model.AvailableDates = Enumerable
            .Range(0, 5)
            .Select(days => DateTime.Now.Date.AddDays(days))
            .ToList();

        model.AvailableTimeSlots = GetAllTimeSlots()
            .Select(slot => model.Date.Date.Add(DateTime.Parse(slot).TimeOfDay))
            .ToList();

        return View(model); // Return the form with errors
    }

    // Get the user ID from session
    var userId = GetCurrentUserId();

    // Validate the selected date is within the allowed range (today to 5 days ahead)
    var validDates = Enumerable
        .Range(0, 5)
        .Select(days => DateTime.Now.Date.AddDays(days))
        .ToList();

    if (!validDates.Contains(model.Date.Date))
    {
        ModelState.AddModelError("", "Selected date is not valid.");

        // Regenerate form data
        model.AvailableDates = validDates;
        model.AvailableTimeSlots = GetAllTimeSlots()
            .Select(slot => model.Date.Date.Add(DateTime.Parse(slot).TimeOfDay))
            .ToList();

        return View(model);
    }

    // Correct SelectedTimeSlot's date to match the selected Date
    model.SelectedTimeSlot = model.Date.Date.Add(model.SelectedTimeSlot.TimeOfDay);

    // Ensure the selected time slot is valid
    var validTimeSlots = GetAllTimeSlots()
        .Select(slot => model.Date.Date.Add(DateTime.Parse(slot).TimeOfDay))
        .ToList();

    if (!validTimeSlots.Contains(model.SelectedTimeSlot))
    {
        ModelState.AddModelError("", "Invalid time slot selected.");

        // Regenerate form data
        model.AvailableDates = validDates;
        model.AvailableTimeSlots = validTimeSlots;

        return View(model);
    }

    // Create a new appointment
    var appointment = new Appointment
    {
        UserId = userId, // Use session UserId
        BarberId = model.BarberId,
        AppointmentDate = model.SelectedTimeSlot, // Use the corrected DateTime
        Status = AppointmentStatus.Pending
    };

    try
    {
        // Save the appointment to the database
        _dbContext.Appointments.Add(appointment);
        _dbContext.SaveChanges();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error saving appointment: {ex.Message}");

        // Show a user-friendly error
        ModelState.AddModelError("", "An error occurred while booking the appointment. Please try again.");
        model.AvailableDates = validDates;
        model.AvailableTimeSlots = validTimeSlots;

        return View(model);
    }

    return RedirectToAction("Confirmation");
}

    public IActionResult Confirmation()
    {
        return View();
    }

    [Authorize]
    public IActionResult ManageAppointments(int barberId)
    {
        barberId = HttpContext.Session.GetInt32("BarberId") ?? barberId;
        if (barberId == 0)
        {
            return RedirectToAction("Login", "Barber"); // Redirect if barberId is missing
        }

        // Fetch appointments for the specified barber and include the related User entity
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

    //For the AJAX script
    public IActionResult GetTimeSlots(DateTime date, int barberId)
    {
        // Fetch booked appointments for the selected date
        var bookedSlots = _dbContext.Appointments
            .Where(a => a.BarberId == barberId && a.AppointmentDate.Date == date.Date)
            .Select(a => a.AppointmentDate.TimeOfDay)
            .ToList();

        // Generate available time slots
        var allTimeSlots = GetAllTimeSlots()
            .Select(slot => date.Date.Add(DateTime.Parse(slot).TimeOfDay))
            .ToList();

        var availableSlots = allTimeSlots
            .Where(slot => !bookedSlots.Contains(slot.TimeOfDay))
            .Select(slot => slot.ToString("hh:mm tt"));

        return Json(availableSlots);
    }
}