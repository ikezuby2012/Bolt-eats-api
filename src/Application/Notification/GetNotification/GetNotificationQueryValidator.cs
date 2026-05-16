using FluentValidation;

namespace Application.Notification.GetNotification;

public class GetNotificationQueryValidator : AbstractValidator<GetNotificationQuery>
{
    public GetNotificationQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
