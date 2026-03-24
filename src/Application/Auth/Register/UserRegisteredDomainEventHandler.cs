using Application.Abstractions.Services;
using Application.Auth.Dto;
using Domain.Users;
using SharedKernel;

namespace Application.Auth.Register;

internal sealed class UserRegisteredDomainEventHandler(IRazorViewToString razorViewToString, IEmailService emailService) : IDomainEventHandler<UserRegisteredDomainEvent>
{
    public async Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        User user = domainEvent.user;
        string otp = domainEvent.otp;

        var emailModel = new SendOtpEmailModel
        {
            OtpCode = otp,
            UserName = user.FirstName
        };

        string emailBody = await razorViewToString.RenderViewToStringAsync("/Views/EmailTemplates/OTPVerificationEmail.cshtml", emailModel);

        await emailService.SendCommonEmail(user.FirstName, user.Email, emailBody, new List<string> { }, "an email to verify email, OTP verification code");
    }
}
