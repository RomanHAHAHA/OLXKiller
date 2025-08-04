using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Application.Common.Dtos;

namespace ProductsService.Application.Features.Products.Commands.Create;

public record CreateProductCommand(
    Guid InitiatorUserId,
    ProductCreateDto ProductCreateDto) : IRequest<ApiResponse<Guid>>;