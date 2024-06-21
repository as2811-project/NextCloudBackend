using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NextCloudBackendServices.Interfaces;

namespace NextCloudBackendServices.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _Client;
    private readonly string _bucketName = "nextcloudbase";

    public S3Service(IAmazonS3 client)
    {
        _Client = client;
    }

    public async Task CreateFolderAsync(string userId)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = $"{userId}/"
        };
        await _Client.PutObjectAsync(request);
    }
}