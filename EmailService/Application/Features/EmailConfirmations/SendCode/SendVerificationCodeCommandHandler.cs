using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using EmailService.Domain.Interfaces;
using MediatR;

namespace EmailService.Application.Features.EmailConfirmations.SendCode;

public class SendVerificationCodeCommandHandler(
    IVerificationCodeGenerator verificationCodeGenerator,
    IEmailSender emailSender, 
    IPasswordHasher passwordHasher,
    ICacheService<string> cacheService) : IRequestHandler<SendVerificationCodeCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(SendVerificationCodeCommand request, CancellationToken cancellationToken)
    {
        var code = verificationCodeGenerator.Generate();
        var hashedCode = passwordHasher.HashPassword(code);
        
        await cacheService.SetAsync(
            request.Email,
            hashedCode,
            TimeSpan.FromMinutes(3),
            cancellationToken);

        await emailSender.SendMessageAsync(
            request.Email, 
            "Verification Code", 
            $"Your verification code is: {code}");

        return ApiResponse.Ok();  
    }
}