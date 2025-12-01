using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Rental.ViewModels
{
    public class CarEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public decimal? PricePerDay { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        // optional image on edit — if provided we will replace the existing one
        public IFormFile? Image { get; set; }

        // Keep the existing image path for display (not posted by the form)
        public string? ExistingImagePath { get; set; }
    }
}