using Common.Domain.Models.Results;
using MediatR;
using ProductsService.Application.Common.Dtos;

namespace ProductsService.Application.Features.Categories.Commands.Create;

public record CreateCategoryCommand(
    Guid InitiatorUserId,
    CategoryCreateDto CategoryCreateDto) : IRequest<ApiResponse>;