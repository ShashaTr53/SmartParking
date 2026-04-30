using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartParking.Models
{
    public class Zone
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int ParkingId { get; set; }

        [ForeignKey("ParkingId")]
        public Parking? Parking { get; set; }

        public ICollection<Spot> Spots { get; set; } = new List<Spot>();
    }
}