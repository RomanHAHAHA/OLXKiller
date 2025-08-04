namespace EmailService.Domain.Interfaces;

public interface IEmailSender
{
    Task SendMessageAsync(string email, string subject, string message);
}