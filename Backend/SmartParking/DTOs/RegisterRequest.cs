using System.ComponentModel.DataAnnotations;
using SmartParking.Models;

namespace SmartParking.DTOs
{
    public class RegisterRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? Role { get; set; }
    }
}