using Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

internal sealed class HttpContextService : IHttpContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetIpAddress()
    {
        HttpContext? context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return null;
        }


        string? forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {

            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }


        string? realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }


        return context.Connection.RemoteIpAddress?.ToString();
    }

    public string? GetUserAgent()
    {
        HttpContext? context = _httpContextAccessor.HttpContext;
        return context?.Request.Headers["User-Agent"].FirstOrDefault();
    }
}
