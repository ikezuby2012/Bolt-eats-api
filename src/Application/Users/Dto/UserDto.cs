using System.Text.Json.Serialization;

namespace Application.Users.Dto;

public sealed class UserDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("fullName")]
    public string FullName => $"{FirstName} {LastName}".Trim();

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("isVerified")]
    public bool IsVerified { get; set; }

    [JsonPropertyName("roleId")]
    public int? RoleId { get; set; }

    [JsonPropertyName("userRole")]
    public string? UserRole { get; set; }


    [JsonPropertyName("lastLogin")]
    public DateTime? LastLogin { get; set; }
    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("createdById")]
    public string? CreatedById { get; set; }

    [JsonPropertyName("addresses")]
    public ICollection<AddressDto>? Addresses { get; set; }

    public static explicit operator UserDto(Domain.Users.User user) => new UserDto
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        IsActive = user.IsActive,
        IsVerified = user.isVerifed,
        RoleId = user.RoleId,
        UserRole = Domain.Users.UserRole.FromValue(user.RoleId)!.Name,
        LastLogin = user.LastLogin,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        CreatedById = user.CreatedById,
        Addresses = user.Addresses?
                .Select(a => (AddressDto)a)
                .ToList()
    };
}
