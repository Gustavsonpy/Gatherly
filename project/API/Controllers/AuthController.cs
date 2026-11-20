using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return Unauthorized("Email ou senha inválidos.");

        if (user.IsSuspended)
            return Unauthorized("Você está suspenso, entrada não permitida.");

        if (string.IsNullOrWhiteSpace(user.Email))
            return Unauthorized("E-mail não preenchido");

        if (string.IsNullOrWhiteSpace(user.HashPassword))
            return Unauthorized("Senha não preenchida.");

        bool correctPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.HashPassword);

        if (!correctPassword)
            return Unauthorized("Email ou senha inválidos.");

        var token = GenerateToken(user);
        return Ok(new { token });
    }

        private string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("name", user.Name)
            };

            var jwtKey = _config["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("JWT Key não configurada.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public record LoginRequest(string Email, string Password);
    }
}