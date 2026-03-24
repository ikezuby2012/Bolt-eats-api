namespace Application.Auth.Dto;

public class SendOtpEmailModel
{
    public string UserName { get; set; }
    public string OtpCode { get; set; }
}
