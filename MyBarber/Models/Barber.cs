using System.ComponentModel.DataAnnotations;

namespace MyBarber.Models;

public class Barber
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Location { get; set; }
    [Phone]
    [Required]
    public string PhoneNumber { get; set; }
    [Required]
    public decimal Pricing { get; set; }
    public string ProfileImage { get; set; } //url to picture
    [Range(0,5)]
    public double Rating { get; set; } = 0;
    public string Services { get; set; }
    public bool isActive { get; set; }
    
    public ICollection<Appointment> Appointments { get; set; }

}