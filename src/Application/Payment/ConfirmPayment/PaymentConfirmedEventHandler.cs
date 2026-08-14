using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Orders.CreateOrder;
using Application.Orders.Dto;
using Domain.Payment;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Payment.ConfirmPayment;

internal sealed class PaymentConfirmedEventHandler(
    //ICommandHandler<CreateOrderCommand, OrderDto> orderHandler,
    //IApplicationDbContext context
    ) : IDomainEventHandler<PaymentConfirmedEvent>
{
    public Task Handle(PaymentConfirmedEvent domainEvent, CancellationToken cancellationToken)
    {
        //Domain.Payment.Payment payment = await context.Payment.FirstAsync(p => p.Id == domainEvent.PaymentId, cancellationToken);

        //Result<OrderDto> orderResult = await orderHandler.Handle(new CreateOrderCommand(domainEvent.CustomerNotes), cancellationToken);

        //if (orderResult.IsFailure)
        //{
        //    payment.OrderCreationFailed = true;
        //    payment.FailureMessage = orderResult.Error.Description;
        //    await context.SaveChangesAsync(cancellationToken);
        //    return;
        //}

        //payment.OrderId = orderResult.Value.Id;
        //await context.SaveChangesAsync(cancellationToken);
        return Task.CompletedTask;
    }
}
