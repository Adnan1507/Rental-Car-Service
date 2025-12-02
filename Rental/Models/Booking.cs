using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rental.Models
{
    public enum BookingStatus
    {
        Requested,
        Approved,
        Rejected,
        Active,
        Completed,
        Cancelled
    }

    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int CarId { get; set; }
        [ForeignKey(nameof(CarId))]
        public Car Car { get; set; }

        [Required]
        public string RenterId { get; set; }
        [ForeignKey(nameof(RenterId))]
        public ApplicationUser Renter { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Requested;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}