using Common.Domain.Models.Results;
using MediatR;

namespace EmailService.Application.Features.EmailConfirmations.SendCode;

public record SendVerificationCodeCommand(string Email) : IRequest<ApiResponse>;