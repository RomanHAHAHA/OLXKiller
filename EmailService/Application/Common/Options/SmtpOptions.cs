namespace EmailService.Application.Common.Options;

public class SmtpOptions
{
    public string Server { get; set; } = string.Empty;

    public string SenderEmail { get; set; } = string.Empty;

    public string AppPassword { get; set; } = string.Empty; 
    
    public int Port { get; set; }
}