using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens; 
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims; 
using System.Text; 
using ToDoList.DTO; 

namespace ToDoList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private static List<UserDto> users = new List<UserDto>();

        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserDto request)
        {
            var existingUser = users.FirstOrDefault(u => u.Username == request.Username);
            if (existingUser != null)
            {
                return BadRequest("Bu kullanıcı adı zaten kullanılıyor.");
            }

            users.Add(new UserDto
            {
                Username = request.Username,
                Password = request.Password 
            });

            return Ok("Kayıt başarılı");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDto request)
        {
            var user = users.FirstOrDefault(u => 
                u.Username == request.Username && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized("Kullanıcı adı veya şifre hatalı");
            }

            var token = GenerateToken(user);
            return Ok(new { token });
        }

        private string GenerateToken(UserDto user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, "User")
            };

            var keyString = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key appsettings.json içinde tanımlı değil.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));


            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}