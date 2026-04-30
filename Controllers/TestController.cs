using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartParking.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            return Ok("Accès Admin autorisé");
        }

        [HttpGet("manager")]
        [Authorize(Roles = "Manager")]
        public IActionResult ManagerOnly()
        {
            return Ok("Accès Manager autorisé");
        }

        [HttpGet("driver")]
        [Authorize(Roles = "Driver")]
        public IActionResult DriverOnly()
        {
            return Ok("Accès Driver autorisé");
        }

        [HttpGet("admin-manager")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult AdminManager()
        {
            return Ok("Accès Admin ou Manager autorisé");
        }
    }
}