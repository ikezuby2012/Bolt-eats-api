namespace Application.Abstractions.Services;

public interface IHttpContextService
{
    string? GetIpAddress();
    string? GetUserAgent();
}
