using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.ProductImages.SetMain;

public record SetMainImageCommand(Guid ImageId) : IRequest<ApiResponse>;