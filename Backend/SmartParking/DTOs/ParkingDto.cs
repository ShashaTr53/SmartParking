using System.ComponentModel.DataAnnotations;
using SmartParking.Models;

// Models/DTOs/ParkingDto.cs
public class ParkingDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}