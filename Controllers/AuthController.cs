using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartParking.DTOs;
using SmartParking.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace SmartParking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // Endpoint pour l'inscription
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            // 1) Vérifier si l'email existe déjà
            var exists = await _db.Users.AnyAsync(u => u.Email == req.Email);
            if (exists)
                return BadRequest(new { message = "Email déjà utilisé" });

            // 2) Hash du mot de passe (SHA256 simple pour ce sprint)
            var hash = HashPassword(req.Password);

            // 3) Créer un utilisateur
            var user = new User
            {
                Email = req.Email,
                PasswordHash = hash,
                Role = req.Role ?? UserRole.Driver,
                IsActive = true
            };

            // 4) Ajouter à la base de données
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // 5) Retourner une réponse avec l'utilisateur créé
            return Ok(new
            {
                user.Id,
                user.Email,
                role = user.Role.ToString()
            });
        }

        // Méthode pour hasher le mot de passe
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // Endpoint pour la connexion et génération de JWT
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == req.Email);
            if (user == null)
                return Unauthorized(new { message = "Identifiants invalides" });

            var hash = HashPassword(req.Password);
            if (hash != user.PasswordHash)
                return Unauthorized(new { message = "Identifiants invalides" });

            if (!user.IsActive)
                return Forbid();

            // Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expireMinutes = 60;
            int.TryParse(_config["Jwt:ExpireMinutes"], out expireMinutes);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = tokenString,
                expires = token.ValidTo,
                user = new { user.Id, user.Email, role = user.Role.ToString() }
            });
        }
    }
}