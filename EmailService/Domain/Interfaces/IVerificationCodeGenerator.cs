namespace EmailService.Domain.Interfaces;

public interface IVerificationCodeGenerator
{
    string Generate();
}