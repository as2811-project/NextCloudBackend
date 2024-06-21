using Amazon.S3;
using Amazon.S3.Model;
using NextCloudBackendServices.Interfaces;

namespace NextCloudBackendServices.Services
{
    public class FileService : IFileService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName = "nextcloudbase"; 

        public FileService(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task UploadFileAsync(string userId, IFormFile file)
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = $"{userId}/{file.FileName}",
                InputStream = file.OpenReadStream(),
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(request);
        }
    }
}