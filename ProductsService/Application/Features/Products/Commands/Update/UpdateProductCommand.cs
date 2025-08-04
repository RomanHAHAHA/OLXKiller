using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Application.Common.Dtos;
using ProductsService.Application.Features.Products.Commands.Create;

namespace ProductsService.Application.Features.Products.Commands.Update;

public record UpdateProductCommand(
    Guid InitiatorUserId,
    Guid ProductId,
    ProductCreateDto ProductCreateDto) : IRequest<ApiResponse<Guid>>; 