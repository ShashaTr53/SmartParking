using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.Models;

namespace SmartParking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ParkingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ParkingsController(AppDbContext context)
        {
            _context = context;
        }

        // ParkingsController - GetAll
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Driver")]
        public async Task<ActionResult> GetParkings()
        {
            var parkings = await _context.Parkings
            .Include(p => p.Zones)
            .Where(p => p.IsActive)  // ← drivers only see active parkings
            .Select(p => new {
                p.Id,
                p.Name,
                p.Address,
                p.IsActive,
                Zones = p.Zones.Select(z => new { z.Id, z.Name })
            })
            .ToListAsync();

                return Ok(parkings);
        }

        // Récupérer un parking spécifique par ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Driver")]  // Ajout du rôle Driver
        public async Task<ActionResult<Parking>> GetParking(int id)
        {
            var parking = await _context.Parkings
                .Include(p => p.Zones)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parking == null)
                return NotFound(new { message = "Parking not found" });

            return Ok(parking);
        }

        // Créer un nouveau parking
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Parking>> CreateParking([FromBody] ParkingDto dto)
        {
            var parking = new Parking
            {
                Name = dto.Name,
                Address = dto.Address,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            _context.Parkings.Add(parking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetParking), new { id = parking.Id }, parking);
        }

        // Mettre à jour un parking existant
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]  // Seul l'Admin peut mettre à jour un parking
        public async Task<IActionResult> UpdateParking(int id, [FromBody] Parking updatedParking)
        {
            var existingParking = await _context.Parkings.FindAsync(id);

            if (existingParking == null)
                return NotFound(new { message = "Parking not found" });

            existingParking.Name = updatedParking.Name;
            existingParking.Address = updatedParking.Address;
            existingParking.Description = updatedParking.Description;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Parking updated successfully" });
        }

        // Supprimer un parking
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]  // Seul l'Admin peut supprimer un parking
        public async Task<IActionResult> DeleteParking(int id)
        {
            var parking = await _context.Parkings
                .Include(p => p.Zones)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parking == null)
                return NotFound(new { message = "Parking not found" });

            if (parking.Zones.Any())
                return BadRequest(new { message = "Cannot delete parking with existing zones" });

            _context.Parkings.Remove(parking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Parking deleted successfully" });
        }

        // Méthode pour activer ou désactiver un parking
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]  // Seul l'Admin peut changer le statut
        public async Task<IActionResult> SetParkingStatus(int id, [FromQuery] string status)
        {
            // Vérification du statut
            if (status != "active" && status != "inactive")
                return BadRequest(new { message = "Invalid status. Use 'active' or 'inactive'." });

            var parking = await _context.Parkings.FindAsync(id);

            if (parking == null)
                return NotFound(new { message = "Parking not found" });

            // Mettre à jour le statut du parking
            parking.IsActive = (status == "active");
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Parking status set to {status}" });
        }
    }
}