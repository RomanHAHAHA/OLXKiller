using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.Categories.Commands.RemoveCategoryFromProduct;

public record RemoveCategoryCommand(
    Guid ProductId, 
    Guid CategoryId) : IRequest<ApiResponse>;