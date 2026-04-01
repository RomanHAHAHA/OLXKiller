using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.Categories.Commands.SetProductCategory;

public record SetProductCategoryCommand(Guid ProductId, Guid? CategoryId) : IRequest<ApiResponse>;