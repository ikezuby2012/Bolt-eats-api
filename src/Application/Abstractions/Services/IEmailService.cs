namespace Application.Abstractions.Services;

public interface IEmailService
{
    Task SendCommonEmail(string name, string email, string htmlBody, List<string> ccReceipients, string subject);
    Task SendCommonEmailWithPdf(string name, string email, string htmlBody, List<string> ccReceipients, string subject, string attachmentFileName, byte[] attachmentBytes, CancellationToken cancellationToken = default);
}
