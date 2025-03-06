using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Models.Entities;
using ExpenseTracker.Data;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using ExpenseTracker.Models.DTOs;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace ExpenseTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public IActionResult Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
                return BadRequest("Invalid user data.");

            if (_context.Users.Any(u => u.Email == registerDto.Email))
                return BadRequest("Email is already taken.");

            // إنشاء Salt عشوائي
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash الباسوورد مع الـ Salt
            string hashedPassword = HashPassword(registerDto.Password, salt);

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Password = hashedPassword,
                PasswordSalt = Convert.ToBase64String(salt) // تخزين الـ Salt
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null)
                return BadRequest("Invalid login data.");

            var user = _context.Users.FirstOrDefault(u => u.Email == loginDto.Email);
            if (user == null || !VerifyPassword(loginDto.Password, user.Password, user.PasswordSalt))
                return Unauthorized(new { message = "Invalid email or password." });

            var token = GenerateJwtToken(user);
            return Ok(new { token, userId = user.Id }); // إضافة userId في الـ Response
        }

        private string GenerateJwtToken(User user)
        {
            var secretKey = _configuration["JWT:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("JWT SecretKey is not configured.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(20),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));
        }

        private bool VerifyPassword(string enteredPassword, string storedHashedPassword, string storedSalt)
        {
            if (string.IsNullOrEmpty(storedSalt))
                return false;

            byte[] salt = Convert.FromBase64String(storedSalt);
            string hashedEnteredPassword = HashPassword(enteredPassword, salt);
            return hashedEnteredPassword == storedHashedPassword;
        }
    }
}