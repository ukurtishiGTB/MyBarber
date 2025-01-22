namespace MyBarber.ViewModels
{
    public class BarberRegisterViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Location { get; set; }
        public string PhoneNumber { get; set; }
        public decimal Pricing { get; set; }
        public string? ProfileImage { get; set; }
        public string? Services { get; set; }
    }

}
