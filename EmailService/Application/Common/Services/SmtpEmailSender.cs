using System.Net;
using System.Net.Mail;
using EmailService.Application.Common.Options;
using EmailService.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace EmailService.Application.Common.Services;

public class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;
    
    public async Task SendMessageAsync(
        string email, 
        string subject, 
        string message, 
        CancellationToken cancellationToken)
    {
        using var smtpClient = new SmtpClient(_options.Server, _options.Port);
        using var mailMessage = new MailMessage();
        
        smtpClient.EnableSsl = true;
        smtpClient.Credentials = new NetworkCredential(
            _options.SenderEmail, 
            _options.AppPassword);

        mailMessage.From = new MailAddress(_options.SenderEmail);
        mailMessage.To.Add(email);
        mailMessage.Subject = subject;
        mailMessage.Body = message;
        mailMessage.IsBodyHtml = true; 

        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }
}