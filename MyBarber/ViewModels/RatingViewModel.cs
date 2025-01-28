using System.ComponentModel.DataAnnotations;

namespace MyBarber.ViewModels
{
    public class RatingViewModel
    {
        public int BarberId { get; set; }
        [Range(1, 5)]
        public int Stars { get; set; }
        public string Comment { get; set; }
    }

}
