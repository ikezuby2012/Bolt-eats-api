using Application.Users.Dto;

namespace Application.Auth.Dto;

public class LoginSuccessDto
{
    public string Token { get; set; }
    public string? RefreshToken { get; set; }
    public CreatedUserDto User { get; set; }
}

