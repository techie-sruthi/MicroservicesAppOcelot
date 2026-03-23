using MediatR;
using ProductService.Application.Common.Interfaces;
using Shared.Kernel.Results;

namespace ProductService.Application.Products.Commands.UploadImage;

public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, Result<string>>
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

    public async Task<Result<string>> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        var file = request.File;

        if (file == null || file.Length == 0)
            return Result<string>.Failure("No file uploaded");

        if (!AllowedImageTypes.Contains(file.ContentType.ToLower()))
            return Result<string>.Failure("Invalid file type. Only images are allowed (JPEG, PNG, GIF, WEBP)");

        if (file.Length > MaxFileSizeInBytes)
            return Result<string>.Failure("File size exceeds 5MB limit");

        using var stream = file.OpenReadStream();
        var url = await _fileStorageService.UploadFileAsync(stream, file.FileName, file.ContentType);
        return Result<string>.Success(url, "Image uploaded successfully.");
    }
}
