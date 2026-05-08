using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Services.Payments;
using Application.Payment.Dto;
using Domain.Common;
using Domain.Payment;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Payment.GetPaymentMethods;

internal sealed class GetPaymentQueryHandler(IApplicationDbContext context, IPaymentGatewayFactory factory) : IQueryHandler<GetPaymentMethodQuery, IReadOnlyList<PaymentMethodDto>>
{
    public async Task<Result<IReadOnlyList<PaymentMethodDto>>> Handle(GetPaymentMethodQuery query, CancellationToken cancellationToken)
    {
        Domain.Users.User? user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<IReadOnlyList<PaymentMethodDto>>(CommonErrors.CustomErrorMessage("No User was found"));
        }

        IPaymentGateway gateway = factory.GetGateway(PaymentGateway.Monnify.Id);
        IReadOnlyList<SavedPaymentMethod> methods = await gateway.ListPaymentMethodsAsync(user.Id.ToString(), cancellationToken);

        var dtos = methods.Select(m => new PaymentMethodDto(
           m.Id, m.Brand, m.Last4, m.ExpMonth, m.ExpYear, m.IsDefault))
           .ToList();

        return dtos;
    }
}
