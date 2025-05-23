using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventMngt.DTOs;
using EventMngt.Models;
using EventMngt.Validators;
using FluentValidation;

namespace EventMngt.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IValidator<RegisterDTO> _registerValidator;
    private readonly IValidator<LoginDTO> _loginValidator;

    public AuthController(
        UserManager<User> userManager,
        IConfiguration configuration,
        IValidator<RegisterDTO> registerValidator,
        IValidator<LoginDTO> loginValidator)
    {
        _userManager = userManager;
        _configuration = configuration;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDTO model)
    {
        var validationResult = await _registerValidator.ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "User with this email already exists" });
        }

        var user = new User
        {
            UserName = model.UserName, // Use the provided username
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { message = "Registration failed", errors });
        }

        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO model)
    {
        var validationResult = await _loginValidator.ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        // Try to find user by email first
        var user = await _userManager.FindByEmailAsync(model.UserName);
        if (user == null)
        {
            // If not found by email, try username
            user = await _userManager.FindByNameAsync(model.UserName);
        }

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var result = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!result)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var token = GenerateJwtToken(user);
        return Ok(new { 
            token,
            user = new {
                id = user.Id,
                email = user.Email,
                userName = user.UserName
            }
        });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.CreatedAt,
            user.LastLoginAt
        });
    }

    private string GenerateJwtToken(User user)
    {
        if (user == null || string.IsNullOrEmpty(user.Id) || string.IsNullOrEmpty(user.Email))
        {
            throw new ArgumentException("Invalid user data for token generation");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class RegisterModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class LoginModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
} 