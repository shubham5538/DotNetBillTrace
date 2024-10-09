using KotBot.Models;
using KotBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KotBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MongoService _mongoService;
        private readonly IConfiguration _configuration;

        public AuthController(MongoService mongoService, IConfiguration configuration)
        {
            _mongoService = mongoService;
            _configuration = configuration;

            // Check the secret key length during controller initialization
            var secretKey = _configuration.GetValue<string>("JwtSettings:SecretKey");
            if (string.IsNullOrEmpty(secretKey) || Encoding.UTF8.GetByteCount(secretKey) < 32) // Updated to check for at least 32 bytes
            {
                throw new Exception("The secret key must be at least 32 characters long for HS256.");
            }
        }

        // Register new user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // Validate MobileNo (10 digits)
            if (string.IsNullOrEmpty(user.MobileNo) || !System.Text.RegularExpressions.Regex.IsMatch(user.MobileNo, @"^\d{10}$"))
            {
                return BadRequest("Mobile number must be exactly 10 digits.");
            }

            // Check if MobileNo already exists
            var existingUser = await _mongoService.GetUserByMobileNoAsync(user.MobileNo);
            if (existingUser != null)
            {
                return BadRequest("Mobile number already exists.");
            }

            // Ensure password is provided
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return BadRequest("Password is required.");
            }

            // Ensure FCMToken is provided
            if (string.IsNullOrEmpty(user.FCMToken))
            {
                return BadRequest("FCMToken is required.");
            }

            // Register the new user
            await _mongoService.RegisterUserAsync(user);
            return Ok("Registration successful.");
        }

        // Login user
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginData)
        {
            // Validate MobileNo (10 digits)
            if (string.IsNullOrEmpty(loginData.MobileNo) || !System.Text.RegularExpressions.Regex.IsMatch(loginData.MobileNo, @"^\d{10}$"))
            {
                return BadRequest("Mobile number must be exactly 10 digits.");
            }

            // Get user by MobileNo
            var user = await _mongoService.GetUserByMobileNoAsync(loginData.MobileNo);
            if (user == null || !_mongoService.VerifyPassword(loginData.PasswordHash, user.PasswordHash))
            {
                return Unauthorized("Invalid mobile number or password.");
            }

            // Generate JWT token upon successful login
            var token = GenerateJwtToken(user);

            // Return JWT token and necessary data
            return Ok(new
            {
                token,
                mobileNo = user.MobileNo,
                fcmToken = user.FCMToken
            });
        }

        // Method to generate JWT token
        private string GenerateJwtToken(User user)
        {
            var secretKey = _configuration.GetValue<string>("JwtSettings:SecretKey");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.MobileNo),
                new Claim("MobileNo", user.MobileNo),
                new Claim("FCMToken", user.FCMToken),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("JwtSettings:Issuer"),
                audience: _configuration.GetValue<string>("JwtSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("JwtSettings:ExpiryMinutes")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // DTO for login requests
    public class LoginRequest
    {
        public string MobileNo { get; set; } // Mobile number required for login
        public string PasswordHash { get; set; } // Password required for login
    }
}
