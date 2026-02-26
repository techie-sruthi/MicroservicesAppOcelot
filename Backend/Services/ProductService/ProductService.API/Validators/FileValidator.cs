namespace ProductService.API.Validators;

public static class FileValidator
{
    private static readonly string[] AllowedImageTypes = new[]
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp"
    };

    private const int MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB

    public static void ValidateImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("No file uploaded");
        }

        if (!AllowedImageTypes.Contains(file.ContentType.ToLower()))
        {
            throw new ArgumentException("Invalid file type. Only images are allowed (JPEG, PNG, GIF, WEBP)");
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            throw new ArgumentException("File size exceeds 5MB limit");
        }
    }
}
