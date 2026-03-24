using SharedKernel;

namespace Domain.Auth;
public class RefreshToken : Auditable<int>
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
    public string? CreatedByIp { get; set; }
    public string? RevokedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
}
