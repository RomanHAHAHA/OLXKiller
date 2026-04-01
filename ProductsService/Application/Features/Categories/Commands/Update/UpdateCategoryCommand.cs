using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.Categories.Commands.Update;

public record UpdateCategoryCommand(
    Guid InitiatorUserId,
    CategoryUpdateDto Dto) : IRequest<ApiResponse>;