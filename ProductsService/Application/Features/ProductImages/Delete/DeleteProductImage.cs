using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.ProductImages.Delete;

public record DeleteProductImage(Guid ImageId) : IRequest<ApiResponse>; 