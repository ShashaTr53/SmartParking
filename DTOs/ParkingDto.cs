using System.ComponentModel.DataAnnotations;

namespace SmartParking.Models.DTOs
{
    public class ParkingDto
    {
        public int Id { get; set; }   

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}