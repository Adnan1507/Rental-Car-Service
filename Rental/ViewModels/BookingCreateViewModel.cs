using System.ComponentModel.DataAnnotations;

namespace Rental.ViewModels
{
    public class BookingCreateViewModel
    {
        public int CarId { get; set; }

        public string CarTitle { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public decimal PricePerDay { get; set; }

        public decimal TotalPrice { get; set; }
    }
}