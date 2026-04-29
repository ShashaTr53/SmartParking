using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.Models;
using SmartParking.Models.DTOs;

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

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Driver")]
        public async Task<ActionResult<IEnumerable<ParkingDto>>> GetParkings()
        {
            var parkings = await _context.Parkings
                .Select(p => new ParkingDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    Description = p.Description,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return Ok(parkings);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Driver")]
        public async Task<ActionResult<ParkingDto>> GetParking(int id)
        {
            var parking = await _context.Parkings.FindAsync(id);

            if (parking == null)
                return NotFound(new { message = "Parking not found" });

            var result = new ParkingDto
            {
                Id = parking.Id,
                Name = parking.Name,
                Address = parking.Address,
                Description = parking.Description,
                IsActive = parking.IsActive
            };

            return Ok(result);
        }

        [HttpGet("search-by-address")]
        [Authorize(Roles = "Admin,Manager,Driver")]
        public async Task<ActionResult<IEnumerable<ParkingDto>>> SearchParkingsByAddress([FromQuery] string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return BadRequest(new { message = "Address is required" });

            var search = address.ToLower();

            var parkings = await _context.Parkings
                .Where(p => p.Address.ToLower().Contains(search))
                .Select(p => new ParkingDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    Description = p.Description,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return Ok(parkings);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ParkingDto>> CreateParking([FromBody] ParkingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parking = new Parking
            {
                Name = dto.Name,
                Address = dto.Address,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            _context.Parkings.Add(parking);
            await _context.SaveChangesAsync();

            var result = new ParkingDto
            {
                Id = parking.Id,
                Name = parking.Name,
                Address = parking.Address,
                Description = parking.Description,
                IsActive = parking.IsActive
            };

            return CreatedAtAction(nameof(GetParking), new { id = parking.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateParking(int id, [FromBody] ParkingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var parking = await _context.Parkings.FindAsync(id);

            if (parking == null)
                return NotFound(new { message = "Parking not found" });

            parking.Name = dto.Name;
            parking.Address = dto.Address;
            parking.Description = dto.Description;
            parking.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Parking updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetParkingStatus(int id, [FromQuery] string status)
        {
            if (status != "active" && status != "inactive")
                return BadRequest(new { message = "Invalid status. Use 'active' or 'inactive'." });

            var parking = await _context.Parkings.FindAsync(id);

            if (parking == null)
                return NotFound(new { message = "Parking not found" });

            parking.IsActive = status == "active";
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Parking status set to {status}" });
        }
    }
}