using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NextCloudBackendServices.Interfaces;
using NextCloudBackendServices.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace NextCloudBackendServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IFileService fileService, IConfiguration configuration, ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] S3DTO s3Dto, [FromQuery] string jwt)
        {
            var file = s3Dto.File;
            if (file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            _logger.LogInformation($"JWT received: {jwt}");

            var userId = GetUserIdFromToken(jwt);
            if (userId == null)
            {
                return Unauthorized("User ID is required.");
            }

            await _fileService.UploadFileAsync(userId, file);
            return Ok("File Uploaded.");
        }

        private string GetUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                _logger.LogInformation("Validating token...");

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "sub").Value;

                _logger.LogInformation($"User ID extracted: {userId}");

                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token validation failed: {ex.Message}");
                return null;
            }
        }
    }
}
