using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NextCloudBackendServices.Interfaces;
using NextCloudBackendServices.Models;

namespace NextCloudBackendServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDynamoDBContext _context;
        private readonly IS3Service _s3Service;
        private readonly IConfiguration _configuration;
        public AuthController(IDynamoDBContext context, IS3Service s3Service, IConfiguration configuration)
        {
            _context = context;
            _s3Service = s3Service;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserRegistration request)
        {
            var user = new User
            {
                Id = Guid.NewGuid().ToString(), // User GUID generation
                UserName = request.UserName,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password), // Password Hashing
            };
            await _context.SaveAsync(user);
            await _s3Service.CreateFolderAsync(user.Id);
            return CreatedAtAction(nameof(Create), new { id = user.Id }, request);
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin request)
        {
                var user = await _context.LoadAsync<User>(request.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    return Unauthorized("Invalid credentials");
                }

                var token = GenerateJwt(user);
                return Ok(new { message = "Login successful", userId = user.Id, token });
        }

        private string GenerateJwt(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}