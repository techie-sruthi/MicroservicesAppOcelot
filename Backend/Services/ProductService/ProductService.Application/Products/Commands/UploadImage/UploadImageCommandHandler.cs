using MediatR;
using ProductService.Application.Common.Interfaces;

namespace ProductService.Application.Products.Commands.UploadImage;

public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, string>
{
    private static readonly string[] AllowedImageTypes =
    [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp"
    ];

    private const int MaxFileSizeInBytes = 5 * 1024 * 1024;

    private readonly IFileStorageService _fileStorageService;

    public UploadImageCommandHandler(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<string> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        var file = request.File;

        if (file == null || file.Length == 0)
            throw new ArgumentException("No file uploaded");

        if (!AllowedImageTypes.Contains(file.ContentType.ToLower()))
            throw new ArgumentException("Invalid file type. Only images are allowed (JPEG, PNG, GIF, WEBP)");

        if (file.Length > MaxFileSizeInBytes)
            throw new ArgumentException("File size exceeds 5MB limit");

        using var stream = file.OpenReadStream();
        return await _fileStorageService.UploadFileAsync(stream, file.FileName, file.ContentType);
    }
}
