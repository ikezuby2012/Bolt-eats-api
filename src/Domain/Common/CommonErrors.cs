using SharedKernel;

namespace Domain.Common;
public static class CommonErrors
{
    public static Error CustomErrorMessage(string message) => Error.Conflict(
        "Common.CustomErrorMessage", $"{message}, Please contact the Administrator for assistance");
}
