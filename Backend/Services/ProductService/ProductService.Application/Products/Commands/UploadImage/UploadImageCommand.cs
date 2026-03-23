using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Kernel.Results;

namespace ProductService.Application.Products.Commands.UploadImage;

public record UploadImageCommand(IFormFile File) : IRequest<Result<string>>;
