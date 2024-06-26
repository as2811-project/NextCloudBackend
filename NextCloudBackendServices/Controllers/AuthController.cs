using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
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
        public AuthController(IDynamoDBContext context, IS3Service s3Service)
        {
            _context = context;
            _s3Service = s3Service;
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

                return Ok(new { message = "Login successful", userId = user.Id });
        }

    }
}