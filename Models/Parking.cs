using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartParking.Models
{
    public class Parking
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public ICollection<Zone> Zones { get; set; } = new List<Zone>();

        // Ajout de la propriété IsActive
        public bool IsActive { get; set; } = true;  // Par défaut, le parking est actif
    }
}
