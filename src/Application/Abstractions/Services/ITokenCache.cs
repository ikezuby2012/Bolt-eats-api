namespace Application.Abstractions.Services;

public interface ITokenCache
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan expiry);
}
