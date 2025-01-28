namespace MyBarber.ViewModels
{
    public class RatingsViewModel
    {
        public int BarberId { get; set; }
        public string BarberName { get; set; }
        public string BarberLocation { get; set; }
        public string BarberPhoneNumber { get; set; }
        public double? BarberRating { get; set; } // Average rating
        public int? BarberNumberOfRatings { get; set; } // Total number of ratings
        public List<RatingViewModel> Ratings { get; set; } // List of individual reviews
    }


}
