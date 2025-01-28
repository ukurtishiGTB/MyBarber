using System.ComponentModel.DataAnnotations;

namespace MyBarber.Models;

public class Barber
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Email { get; set; }
    [Required] 
    public string HashPassword { get; set; }
    [Required]
    public string Location { get; set; }
    [Phone]
    [Required]
    public string PhoneNumber { get; set; }
    [Required]
    public decimal Pricing { get; set; }
    public string? ProfileImage { get; set; } //url to picture
    [Range(0,5)]
    public double? Rating { get; set; } = 0;
    public int? NumberOfRatings { get; set; } = 0; // Number of ratings
    public string? Services { get; set; }
    public bool? isActive { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public ICollection<Appointment> Appointments { get; set; }
    public ICollection<Rating> Ratings { get; set; }

}