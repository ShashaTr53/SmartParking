using System.ComponentModel.DataAnnotations;
using SmartParking.Models;

namespace SmartParking.Models.DTOs
{
    public class SpotDto
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public int ZoneId { get; set; }

        public SpotStatus Status { get; set; } = SpotStatus.Free;
    }
}
