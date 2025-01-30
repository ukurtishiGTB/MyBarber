namespace MyBarber.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int BarberId { get; set; } // For barbers
        public Barber Barber { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public int? AppointmentId  { get; set; }
        public Appointment Appointment { get; set; }
    }


}
