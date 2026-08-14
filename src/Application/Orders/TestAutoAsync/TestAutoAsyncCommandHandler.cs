using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Rider;
using Domain.Order;
using SharedKernel;

namespace Application.Orders.TestAutoAsync;

internal sealed class TestAutoAsyncCommandHandler(IRiderAssignmentService riderAssignmentService) : ICommandHandler<TestAutoAsyncCommand>
{
    public async Task<Result> Handle(TestAutoAsyncCommand command, CancellationToken cancellationToken)
    {
        await riderAssignmentService.TryAutoAssignAsync(
               command.OrderId,
               CancellationToken.None);

        return Result.Success();
    }
}
