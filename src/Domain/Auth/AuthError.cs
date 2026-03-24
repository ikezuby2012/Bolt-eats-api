using SharedKernel;

namespace Domain.Auth;
public static class AuthError
{
    public static readonly Error LoginFailed = Error.NotFound(
       "Auth.LoginFailed",
       "username and password not correct!");
    public static readonly Error InvalidOtp = Error.NotFound(
       "Auth.InvalidOtp",
       "OTP is Invalid!");
    public static readonly Error NotAuthenticated = Error.NotFound(
       "Auth.NotAuthenticated",
       "User not authenticated!");
    public static readonly Error NotPermitted = Error.NotFound(
      "Auth.NotPermitted",
      "You don't have permission to delete this Doc");
}
