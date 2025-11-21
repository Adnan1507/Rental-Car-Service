using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Rental.ViewModels
{
    public class CarCreateViewModel
    {
        [Required]
        public string Brand { get; set; }

        [Required]
        public string Model { get; set; }

        [Range(1950, 2050)]
        public int Year { get; set; }

        [Required]
        public string Transmission { get; set; }

        [Required]
        public string FuelType { get; set; }

        [Range(1, 20)]
        public int Seats { get; set; }

        [Required]
        public decimal PricePerDay { get; set; }

        [Required]
        public string Location { get; set; }

        public string? Description { get; set; }

        [Required]
        public IFormFile Image { get; set; }
    }
}
