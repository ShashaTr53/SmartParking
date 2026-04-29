using System.ComponentModel.DataAnnotations;

namespace SmartParking.Models.DTOs
{
    public class ZoneDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int ParkingId { get; set; }
    }
}
