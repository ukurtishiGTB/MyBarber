using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBarber.Models;

public class Appointment
{
    public int Id { get; set; }
    [Required]
    public int UserId { get; set; }
    [Required]
    public int BarberId { get; set; }
    [Required]
    public DateTime AppointmentDateee { get; set; }
    
    [ForeignKey("UserId")]
    public User User { get; set; }
    [ForeignKey("BarberId")]
    public Barber Barber { get; set; }
}