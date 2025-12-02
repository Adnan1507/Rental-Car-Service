using Rental.Models;
using System.Collections.Generic;

namespace Rental.ViewModels
{
    public class RenterDashboardViewModel
    {
        public IEnumerable<Car> AvailableCars { get; set; } = new List<Car>();
        public IEnumerable<Booking> Bookings { get; set; } = new List<Booking>();
    }
}