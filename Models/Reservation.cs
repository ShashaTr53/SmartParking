using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartParking.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime PlannedEndTime { get; set; }

        public double InitialAmount { get; set; } = 3;

        public double ExtraAmount { get; set; } = 0;

        public double TotalAmount { get; set; } = 3;

        public ReservationStatus Status { get; set; } = ReservationStatus.Created;

        // Relation avec User
        public int UserId { get; set; }
        public User? User { get; set; }

        // Relation avec Spot
        public int SpotId { get; set; }
        public Spot? Spot { get; set; }
    }
}