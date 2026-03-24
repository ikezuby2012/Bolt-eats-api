namespace SharedKernel;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
    DateTime WatTime { get; }
}
