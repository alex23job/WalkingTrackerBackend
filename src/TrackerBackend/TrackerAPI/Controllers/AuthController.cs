using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TrackerAPI.Data;
using TrackerAPI.Data.Entities;
using TrackerAPI.DTOs;

namespace TrackerAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Проверка на дубликат email через хеширование
                var existingUser = await _context.Users
                    .AsNoTracking() // Оптимизация чтения
                    .FirstOrDefaultAsync(u => u.EmailHash == HashEmail(request.Email));

                if (existingUser != null)
                {
                    return Conflict(new { message = "Пользователь с такой почтой уже существует" });
                }

                // Создание пользователя
                var user = new User
                {
                    EmailHash = HashEmail(request.Email),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow,
                    StepLengthMeters = 0.72
                };

                _context.Users.Add(user);

                // Сохраняем в реальную БД (если строка подключения настроена верно)
                await _context.SaveChangesAsync();

                // Генерация токена
                var token = GenerateJwtToken(user);

                // !!! ИСПРАВЛЕНИЕ: Возвращаем строго наш DTO !!!
                return Ok(new AuthResponseDto
                {
                    Token = token
                });
            }
            catch (Exception ex)
            {
                // Логируем внутреннюю ошибку сервера
                Console.WriteLine($"[ERROR] Registration failed: {ex.Message}");
                return StatusCode(500, new { message = "Внутренняя ошибка сервера регистрации." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] RegisterRequest request) // Используем тот же DTO для простоты
        {
            var emailHash = HashEmail(request.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailHash == emailHash);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Неверная почта или пароль");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        // --- Вспомогательные методы ---

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.EmailHash),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashEmail(string email)
            => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(email.ToLowerInvariant())));
    }
}
