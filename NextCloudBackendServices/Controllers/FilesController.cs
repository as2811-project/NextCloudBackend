using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using NextCloudBackendServices.Interfaces;
using NextCloudBackendServices.Models;

namespace NextCloudBackendServices.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] S3DTO s3Dto, string userId)
    {
        var file = s3Dto.File;
        if (file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        if (userId == null)
        {
            return Unauthorized("User ID is required.");
        }

        await _fileService.UploadFileAsync(userId, file);
        return Ok("File Uploaded.");
    }
}