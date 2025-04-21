using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ToDoList.DTO;
using TodoListApp.Models;

namespace ToDoList.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly TodoDbContext _context;

        public AuthController(IConfiguration configuration, TodoDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
            {
                return BadRequest("Bu kullanıcı adı zaten kullanılıyor.");
            }

            // String role gönderildiyse çeviriyoruz
            var parsedRole = Enum.TryParse<UserRole>(request.Role.ToString(), out var userRole)
                ? userRole
                : UserRole.User;

            var user = new User
            {
                Username = request.Username,
                Password = request.Password,
                Role = parsedRole
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Kayıt başarılı");
        }


        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] UserDto request)
{
    Console.WriteLine($"Gelen login: {request.Username} - {request.Password}");

    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

    if (user == null)
    {
        Console.WriteLine("Kullanıcı bulunamadı!");
        return Unauthorized("Kullanıcı adı veya şifre hatalı");
    }

    var token = GenerateToken(user);
    return Ok(new { token });
}


        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()), 
                new Claim("userId", user.Id.ToString()) 
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key eksik")));

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