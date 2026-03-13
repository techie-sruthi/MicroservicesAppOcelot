using MediatR;
using Microsoft.AspNetCore.Http;

namespace ProductService.Application.Products.Commands.UploadImage;

public record UploadImageCommand(IFormFile File) : IRequest<string>;
