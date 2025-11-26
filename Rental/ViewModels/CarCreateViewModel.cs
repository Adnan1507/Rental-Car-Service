using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Rental.ViewModels
{
    public class CarCreateViewModel
    {
        [Required]
        public string Brand { get; set; }

        [Required]
        public string CarType { get; set; }   // Luxury, Sedan, SUV

        [Required]
        public string Model { get; set; }

        [Required]
        [Range(1950, 2050)]
        public int? Year { get; set; }

        [Required]
        public string Transmission { get; set; }

        [Required]
        public string FuelType { get; set; }

        [Required]
        [Range(1, 20)]
        public int? Seats { get; set; }

        [Required]
        public decimal? PricePerDay { get; set; }

        [Required]
        public string Location { get; set; }

        public string? Description { get; set; }

        [Required]
        public IFormFile Image { get; set; }
    }
}
