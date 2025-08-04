using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.Categories.Commands.AddCategoryToProduct;

public record AddCategoryCommand(Guid ProductId, Guid CategoryId) : IRequest<ApiResponse>;