using SharedKernel;

namespace Domain.Users;
public sealed class UserRole : Enumeration<UserRole>
{
    public static readonly UserRole User = new(1, "User");
    public static readonly UserRole BusinessDeveloper = new(2, "Business_Developer");
    public static readonly UserRole Admin = new(3, "Admin");
    public static readonly UserRole Rider = new(4, "Rider");

    private UserRole(int Id, string name) : base(Id, name) { }
}
