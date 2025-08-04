using Common.Domain.Models.Results;
using MediatR;

namespace UsersService.Application.Features.Users.MarkEmailConfirmed;

public record MarkEmailAsConfirmedCommand(string Email) : IRequest<ApiResponse>;