using Common.Domain.Models.Results;
using MediatR;

namespace ProductsService.Application.Features.Categories.Commands.Delete;

public record DeleteCategoryCommand(Guid InitiatorUserId, Guid CategoryId) : IRequest<ApiResponse>;