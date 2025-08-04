using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Application.Common.Dtos;

namespace ProductsService.Application.Features.Categories.Commands.Update;

public record UpdateCategoryCommand(
    Guid InitiatorUserId,
    Guid CategoryId,
    CategoryCreateDto CategoryCreateDto) : IRequest<ApiResponse>;