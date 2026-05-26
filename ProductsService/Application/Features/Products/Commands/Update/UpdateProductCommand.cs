using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Application.Common.Dtos;

namespace ProductsService.Application.Features.Products.Commands.Update;

public record UpdateProductCommand(
    Guid InitiatorUserId,
    Guid ProductId,
    ProductCreateDto ProductCreateDto) : IRequest<ApiResponse<Guid>>; 