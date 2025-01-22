namespace MyBarber.ViewModels;

public class AppointmentBookingViewModel
{
    public int BarberId { get; set; }
    public DateTime Date { get; set; }
    public List<DateTime> AvailableTimeSlots { get; set; }
    public DateTime SelectedTimeSlot { get; set; }
}