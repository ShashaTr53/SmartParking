using System.ComponentModel.DataAnnotations;
using SmartParking.Models;

namespace SmartParking.DTOs
{
    public class RegisterRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // Role optionnel, si pas précisé, on le met par défaut à 'Driver'
        public UserRole? Role { get; set; } = UserRole.Driver;
    }
}