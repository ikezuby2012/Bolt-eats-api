using Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Infrastructure.Services;

internal class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string? GetSenderEmail()
    {
        return _configuration["Email:FromEmail"];
    }

    private async Task SendEmailFunc(MimeMessage message, CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient();
        client.CheckCertificateRevocation = false;
        await client.ConnectAsync("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(_configuration["Email:FromEmail"]!, _configuration["Email:Password"]!, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    public async Task SendCommonEmail(string name, string email, string htmlBody, List<string> ccReceipients, string subject)
    {
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        using var message = new MimeMessage
        {
            Subject = subject
        };

        string address2 = GetSenderEmail();
        message.From.Add(new MailboxAddress("UberEats", address2!));
        message.Body = bodyBuilder.ToMessageBody();

        message.To.Add(new MailboxAddress(name, email));

        foreach (string? ccEmail in ccReceipients.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            message.Cc.Add(new MailboxAddress("", ccEmail));
        }

        await SendEmailFunc(message);
    }

    public async Task SendCommonEmailWithPdf(string name, string email, string htmlBody, List<string> ccReceipients, string subject, string attachmentFileName, byte[] attachmentBytes, CancellationToken cancellationToken = default)
    {
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        using var message = new MimeMessage
        {
            Subject = subject
        };

        if (attachmentBytes != null && attachmentBytes.Length > 0)
        {
            using var memoryStream = new MemoryStream(attachmentBytes);
            await bodyBuilder.Attachments.AddAsync(attachmentFileName, memoryStream, ContentType.Parse("application/pdf"), cancellationToken: cancellationToken);
        }

        message.From.Add(new MailboxAddress("UberEats", GetSenderEmail()!));
        message.Body = bodyBuilder.ToMessageBody();

        message.To.Add(new MailboxAddress(name, email));

        foreach (string? ccEmail in ccReceipients.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            message.Cc.Add(new MailboxAddress("", ccEmail));
        }

        await SendEmailFunc(message, cancellationToken);
    }
}
