using System.ComponentModel.DataAnnotations;

namespace MyBarber.Models;

public class LoyaltyProgram
{
    public int Id { get; set; } 
    [Required]
    public int UserId { get; set; } 

    [Required]
    public int Points { get; set; } = 0; // Loyalty points earned by the user
    
    public User User { get; set; }
}