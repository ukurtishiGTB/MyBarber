namespace MyBarber.Models;

public class BarberListViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    public string PhoneNumber { get; set; }
    public decimal Pricing { get; set; }
    public string? ProfileImage { get; set; }
    public string? Services { get; set; }
}