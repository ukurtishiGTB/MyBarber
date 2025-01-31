using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBarber.Data;
using MyBarber.Models;
using MyBarber.ViewModels;

namespace MyBarber.Controllers;

// Ensure all actions in this controller require authentication
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
            // Regenerate available dates and time slots
            model.AvailableDates = Enumerable
                .Range(0, 5)
                .Select(days => DateTime.Now.Date.AddDays(days))
                .ToList();

            model.AvailableTimeSlots = GetAllTimeSlots()
                .Select(slot => model.Date.Date.Add(DateTime.Parse(slot).TimeOfDay))
                .ToList();

            return View(model);
        }

        var userId = GetCurrentUserId();
        var appointmentDateTime = model.Date.Date + model.SelectedTimeSlot.TimeOfDay;


        var appointment = new Appointment
        {
            UserId = userId,
            BarberId = model.BarberId,
            AppointmentDate = appointmentDateTime,
            Status = AppointmentStatus.Pending
        };

        _dbContext.Appointments.Add(appointment);
        _dbContext.SaveChanges();

        // Create a notification linked to the appointment
        var notification = new Notification
        {
            BarberId = model.BarberId,
            AppointmentId = appointment.Id, // Link to the appointment
            Message = $"You have a new appointment scheduled on {model.SelectedTimeSlot}.",
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _dbContext.Notifications.Add(notification);
        _dbContext.SaveChanges();

        return RedirectToAction("Confirmation");
    }



    public IActionResult Confirmation()
    {
        return View();
    }
    public IActionResult ManageAppointments()
    {
        var barberId = HttpContext.Session.GetInt32("BarberId");
        if (barberId == 0)
        {
            return RedirectToAction("Login", "Barber"); // Redirect if barberId is missing
        }

        // Fetch appointments for the specified barber and include the related User entity
        var pendingAppointments = _dbContext.Appointments
            .Where(a => a.BarberId == barberId)
            .Include(a => a.User)
            .ToList();
        
        
        var notifications = _dbContext.Notifications
       .Where(n => n.BarberId == barberId && !n.IsRead)
       .ToList();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        return View(pendingAppointments);
    }
    
    private void SendNotification(int userId, string message)
    {
        var notification = new UserNotification()
        {
            UserId = userId, // For users
            Message = message,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _dbContext.UserNotifications.Add(notification);
        _dbContext.SaveChanges();
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
        
        SendNotification(appointment.UserId, "Your appointment has been accepted.");

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
        SendNotification(appointment.UserId, "Your appointment has been rejected.");

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
    [HttpPost]
    public IActionResult MarkNotificationsAsRead()
    {
        var barberId = HttpContext.Session.GetInt32("BarberId");
        if (barberId == null)
        {
            return Unauthorized();
        }

        var unreadNotifications = _dbContext.Notifications
            .Where(n => n.BarberId == barberId && !n.IsRead)
            .ToList();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        _dbContext.SaveChanges();

        return Ok(); // Return a success response
    }

}