namespace Application.Abstractions.Services;

public interface IRiderAssignmentService
{
    Task TryAutoAssignAsync(Guid orderId, CancellationToken cancellationToken = default);
}
