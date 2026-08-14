
using Application.Abstractions.Messaging;

namespace Application.Orders.TestAutoAsync;
public sealed record TestAutoAsyncCommand(Guid OrderId) : ICommand;
