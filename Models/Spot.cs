using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartParking.Models
{
    public class Spot
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public int ZoneId { get; set; }

        [ForeignKey("ZoneId")]
        public Zone? Zone { get; set; }

        [Required]
        public SpotStatus Status { get; set; } = SpotStatus.Free;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
