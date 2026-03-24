using SharedKernel;

namespace Infrastructure.Time;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    private static readonly TimeZoneInfo WestAfricaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time");

    public DateTime UtcNow => DateTime.UtcNow.AddHours(1);
    public DateTime Now => DateTime.UtcNow;
    public DateTime WatTime => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, WestAfricaTimeZone);
}
