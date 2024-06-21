namespace NextCloudBackendServices.Interfaces;

public interface IFileService
{
    Task UploadFileAsync(string userId, IFormFile file);
}