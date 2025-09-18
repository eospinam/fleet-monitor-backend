using System.Security.Cryptography;
using FleetMonitor.Api.Services;
using FleetMonitor.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FleetMonitor.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public AuthController(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Usuario ya existe");

            // hash con PBKDF2
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[16];
            rng.GetBytes(salt);

            var hash = HashPassword(dto.Password, salt);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                PasswordHash = hash,
                Salt = salt,
                Role = dto.Role ?? "user"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok("Usuario creado");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null) return Unauthorized();

            var hash = HashPassword(dto.Password, user.Salt);
            if (!hash.SequenceEqual(user.PasswordHash)) return Unauthorized();

            var token = _jwt.CreateToken(user.Id.ToString(),user.Username, user.Role, TimeSpan.FromHours(8));
            return Ok(new { token, expiresAt = DateTime.UtcNow.AddHours(8) });
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32);
        }
    }

    public record RegisterDto(string Username, string Password, string? Role);
    public record LoginDto(string Username, string Password);

}
