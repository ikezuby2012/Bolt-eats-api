using Domain.Users;
using SharedKernel;

namespace Domain.Notification;

public class DeviceToken : Auditable<Guid>
{
    public Guid UserId { get; set; }
    public User User { get; set; }

    /// <summary>FCM registration token from Flutter firebase_messaging.</summary>
    public string Token { get; set; }

    /// <summary>android | ios | web</summary>
    public string Platform { get; set; }

    public bool IsActive { get; set; } = true;
}
