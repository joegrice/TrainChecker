using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TrainChecker.Data;
using TrainChecker.Models;
using TrainChecker.Models.DTOs;

namespace TrainChecker.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthController(IConfiguration configuration, ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        if (await _context.Users.AnyAsync(u => u.Email == registerRequest.Email))
        {
            return BadRequest("User with this email already exists.");
        }

        var user = new User
        {
            Email = registerRequest.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Create default user preferences
        var userPreferences = new UserPreferences
        {
            UserId = user.Id,
            IsTelegramEnabled = false // Default to false
        };
        _context.UserPreferences.Add(userPreferences);
        await _context.SaveChangesAsync();

        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginRequest.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        user.LastLoggedIn = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Ensure user has preferences, create if not
        var existingPreferences = await _context.UserPreferences.FirstOrDefaultAsync(up => up.UserId == user.Id);
        if (existingPreferences == null)
        {
            var userPreferences = new UserPreferences
            {
                UserId = user.Id,
                IsTelegramEnabled = false
            };
            _context.UserPreferences.Add(userPreferences);
            await _context.SaveChangesAsync();
        }

        var token = GenerateJwtToken(user.Email, user.Id);
        return Ok(new AuthLoginResponse { Token = token, UserId = user.Id, Email = user.Email });
    }

    private string GenerateJwtToken(string email, int userId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", userId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"]!,
            audience: _configuration["Jwt:Audience"]!,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}