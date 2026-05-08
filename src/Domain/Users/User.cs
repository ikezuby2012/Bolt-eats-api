using SharedKernel;

namespace Domain.Users;

public sealed class User : Auditable<Guid>
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string OTP { get; set; }
    public int RoleId { get; set; }
    public UserRole? UserRole { get; set; }
    public DateTime? LastLogin { get; set; }
    public string? CreatedById { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? MonnifyCustomerId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool isVerifed { get; set; }
    public bool IsOnline { get; set; }
    public ICollection<Address.Address>? Addresses { get; set; } = new List<Address.Address>();

    public string? GetGatewayCustomerId(int GatewayId) => GatewayId switch
    {
        1 => StripeCustomerId,
        2 => MonnifyCustomerId,
        _ => null
    };
}
