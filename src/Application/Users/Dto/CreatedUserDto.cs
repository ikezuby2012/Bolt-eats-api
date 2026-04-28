using Domain.Users;

namespace Application.Users.Dto;

public class CreatedUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string OTP { get; set; }
    public string RoleName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool isVerifed { get; set; }

    public static explicit operator CreatedUserDto(User user) => new CreatedUserDto
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        OTP = user.OTP,
        RoleName = UserRole.FromValue(user.RoleId!)?.Name!,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        IsActive = user.IsActive,
        isVerifed = user.isVerifed
    };
}
