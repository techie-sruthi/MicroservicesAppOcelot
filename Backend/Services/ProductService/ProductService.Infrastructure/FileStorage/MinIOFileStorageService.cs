using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using ProductService.Application.Common.Interfaces;
using System.IO;

namespace ProductService.Infrastructure.FileStorage;

public class MinIOFileStorageService : IFileStorageService, IDisposable
{
    private readonly AmazonS3Client _s3Client;
    private readonly MinIOSettings _settings;
    private readonly ILogger<MinIOFileStorageService> _logger;
    private bool _bucketEnsured;
    private bool _disposed;

    public MinIOFileStorageService(IOptions<MinIOSettings> settings, ILogger<MinIOFileStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var credentials = new BasicAWSCredentials(_settings.AccessKey, _settings.SecretKey);
        var config = new AmazonS3Config
        {
            ServiceURL = $"{(_settings.UseSSL ? "https" : "http")}://{_settings.Endpoint}",
            ForcePathStyle = true // MinIO requires path-style URLs
        };

        _s3Client = new AmazonS3Client(credentials, config);
    }

    private async Task EnsureBucketExistsAsync()
    {
        try
        {
            try
            {
                await _s3Client.GetBucketLocationAsync(_settings.BucketName);
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("MinIO bucket already exists: {BucketName}", _settings.BucketName);
                }
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(ex, "Bucket not found. Creating MinIO bucket: {BucketName}", _settings.BucketName);
                }
                await _s3Client.PutBucketAsync(_settings.BucketName);

                var policy = @"
                {
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {
                            ""Effect"": ""Allow"",
                            ""Principal"": {""AWS"": ""*""},
                            ""Action"": ""s3:GetObject"",
                            ""Resource"": ""arn:aws:s3:::" + _settings.BucketName + @"/products/*""
                        }
                    ]
                }";

                await _s3Client.PutBucketPolicyAsync(new PutBucketPolicyRequest
                {
                    BucketName = _settings.BucketName,
                    Policy = policy
                });

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("MinIO bucket created: {BucketName}", _settings.BucketName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring MinIO bucket exists");
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            if (!_bucketEnsured)
            {
                await EnsureBucketExistsAsync();
                _bucketEnsured = true;
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var key = $"products/{uniqueFileName}";

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = key,
                BucketName = _settings.BucketName,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead
            };

            using var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            var protocol = _settings.UseSSL ? "https" : "http";
            var host = string.IsNullOrEmpty(_settings.PublicEndpoint) ? _settings.Endpoint : _settings.PublicEndpoint;
            var fileUrl = $"{protocol}://{host}/{_settings.BucketName}/{key}";

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("File uploaded to MinIO: {FileUrl}", fileUrl);
            }
            return fileUrl;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "MinIO S3 error uploading file");
            throw new IOException($"Error uploading file to MinIO: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error uploading file to MinIO");
            throw new IOException($"Unexpected error during file upload: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogWarning("Could not extract key from URL: {FileUrl}", fileUrl);
                return false;
            }

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("File deleted from MinIO: {Key}", key);
            }
            return true;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "MinIO S3 error deleting file");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting file from MinIO");
            return false;
        }
    }

    public Task<string> GetFileUrlAsync(string fileName)
    {
        var key = $"products/{fileName}";
        var protocol = _settings.UseSSL ? "https" : "http";
        var host = string.IsNullOrEmpty(_settings.PublicEndpoint) ? _settings.Endpoint : _settings.PublicEndpoint;
        var fileUrl = $"{protocol}://{host}/{_settings.BucketName}/{key}";
        return Task.FromResult(fileUrl);
    }

    private string? ExtractKeyFromUrl(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var path = uri.AbsolutePath;
            var bucketPrefix = $"/{_settings.BucketName}/";
            if (path.StartsWith(bucketPrefix))
            {
                return path.Substring(bucketPrefix.Length);
            }
            return path.TrimStart('/');
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting key from URL: {FileUrl}", fileUrl);
            return null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _s3Client?.Dispose();
            }
            _disposed = true;
        }
    }
}
