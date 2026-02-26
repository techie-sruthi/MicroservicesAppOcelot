namespace ProductService.Infrastructure.FileStorage;

public class MinIOSettings
{
    public string Endpoint { get; set; } = default!;
    public string PublicEndpoint { get; set; } = default!;
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string BucketName { get; set; } = default!;
    public bool UseSSL { get; set; } = false;
}
