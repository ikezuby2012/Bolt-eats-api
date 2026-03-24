using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

internal sealed class UserContext : IUserContext
{
    //internal sealed class UserContextUnavailableException : Exception
    //{
    //    public UserContextUnavailableException() : base("User context is unavailable")
    //    {
    //    }
    //}

    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId =>
        _httpContextAccessor
            .HttpContext?
            .User
            .GetUserId() ??
        throw new ApplicationException("User context is unavailable");
}
