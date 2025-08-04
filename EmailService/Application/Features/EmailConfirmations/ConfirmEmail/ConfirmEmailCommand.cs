using Common.Domain.Models.Results;
using MediatR;

namespace EmailService.Application.Features.EmailConfirmations.ConfirmEmail;

public record ConfirmEmailCommand(string Email, string Code) : IRequest<ApiResponse>;