using System.ComponentModel.DataAnnotations;

namespace MyBarber.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int BarberId { get; set; }
        public Barber Barber { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        [Range(1, 5)]
        public int Stars { get; set; } // User's rating (1-5)
        public string Comment { get; set; } // Optional comment
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
