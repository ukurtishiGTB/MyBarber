using MyBarber.Models;

public class UserNotification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public int? AppointmentId { get; set; }
    public Appointment Appointment { get; set; }
}