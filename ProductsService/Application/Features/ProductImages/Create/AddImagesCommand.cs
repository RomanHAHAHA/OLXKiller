using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.ProductImages.Create;

public record AddImagesCommand(
    List<IFormFile> Images,
    Guid ProductId) : IRequest<ApiResponse>;