using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBarber.Data;
using MyBarber.Models;
using MyBarber.ViewModels;

namespace MyBarber.Controllers;

public class AppointmentController:Controller
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
            // Add more slots as needed
        };
    }
    public IActionResult Book(int barberId, DateTime date)
    {
        // Fetch existing appointments for the barber on the selected day
        var existingAppointments = _dbContext.Appointments
            .Where(a => a.BarberId == barberId && a.AppointmentDate.Date == date.Date)
            .Select(a => a.AppointmentDate) // Include full DateTime for booked slots
            .ToList();

        // Get all possible time slots for the barber
        var allTimeSlots = GetAllTimeSlots()
            .Select(slot => date.Date.Add(DateTime.Parse(slot).TimeOfDay)) // Parse "09:00 AM" as DateTime
            .ToList();

        // Filter out booked time slots
        var availableTimeSlots = allTimeSlots.Except(existingAppointments).ToList();

        // Pass the available slots to the view
        var viewModel = new AppointmentBookingViewModel
        {
            BarberId = barberId,
            Date = date,
            AvailableTimeSlots = availableTimeSlots
        };

        return View(viewModel);
    }
}