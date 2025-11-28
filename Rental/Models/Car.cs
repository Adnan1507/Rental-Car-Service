using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rental.Models
{
    public enum CarStatus
    {
        Pending,   // Waiting for admin approval
        Approved,  // Visible to renters
        Rejected   // Rejected by admin
    }

    public class Car
    {
        public int Id { get; set; }

        // Which HOST owns this car (ApplicationUser)
        [Required]
        public string HostId { get; set; }   // FK to AspNetUsers table

        [ForeignKey(nameof(HostId))]
        public ApplicationUser Host { get; set; }

        // Basic car info
        [Required]
        [StringLength(50)]
        public string Brand { get; set; }    // e.g. Toyota, BMW

        // New: CarType (Luxury, Sedan, SUV)
        [Required]
        [StringLength(20)]
        public string CarType { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }    // e.g. Corolla, 3-Series

        [Required]
        [Range(1950, 2050)]
        public int Year { get; set; }

        [Required]
        [StringLength(20)]
        public string Transmission { get; set; } // Auto / Manual

        [Required]
        [StringLength(20)]
        public string FuelType { get; set; }     // Petrol / Diesel / EV

        [Required]
        [Range(1, 20)]
        public int Seats { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerDay { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }     // City / Area

        [StringLength(500)]
        public string? Description { get; set; }

        public string? ImagePath { get; set; }   // where we store the car photo

        public CarStatus Status { get; set; } = CarStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
