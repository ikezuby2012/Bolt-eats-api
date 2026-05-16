using System.ComponentModel.DataAnnotations;
using SharedKernel;

namespace Domain.Notification;

public sealed class NotificationChannel : Enumeration<NotificationChannel>
{
    [Display(Name = "Push", Description = "FCM only - Real-time push notification")]
    public static readonly NotificationChannel Push = new(1, "Push");

    [Display(Name = "In-App", Description = "Stored in DB, fetched via GET /notifications")]
    public static readonly NotificationChannel InApp = new(2, "InApp");

    [Display(Name = "Both", Description = "FCM + stored in DB")]
    public static readonly NotificationChannel Both = new(3, "Both");

    private NotificationChannel(int Id, string name) : base(Id, name) { }
}
