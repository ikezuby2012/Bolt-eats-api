using SharedKernel;

namespace Domain.Users;

public sealed class User : Auditable<Guid>
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public string OTP { get; set; }
    public int? RoleId { get; set; }
    public UserRole? UserRole { get; set; }
    public DateTime? LastLogin { get; set; }
    public string? CreatedById { get; set; }
    public bool IsActive { get; set; } = true;
    public bool isVerifed { get; set; }
}
