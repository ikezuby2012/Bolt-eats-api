namespace Application.Abstractions.Authentication;


public interface IOtpHandler
{
    string GenerateOtp();
    string EncryptOtp(string otp);
    bool VerifyOtp(string encryptedOtp, string otpToVerify);
}
