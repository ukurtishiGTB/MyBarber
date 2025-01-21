using System.ComponentModel.DataAnnotations;

namespace MyBarber.Models;

public class User
{
    public int Id { get; set; }
    [Required] public string Name { get; set; }
    [Required] public string Email { get; set; }
    [Required] public string PasswordHash { get; set; }
    public bool isActive { get; set; }
    public ICollection<Appointment> Appointments { get; set; }
}