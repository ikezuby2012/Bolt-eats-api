using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Cart.ClearCart;

internal sealed class ClearCartCommandHandler(IApplicationDbContext context, IUserContext userContext) : ICommandHandler<ClearCartCommand>
{
    public async Task<Result> Handle(ClearCartCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        Domain.Cart.Cart? cart = await context.Cart.Include(c => c.Items).FirstOrDefaultAsync(x => x.UserId == userId && x.Id == command.CartId, cancellationToken);

        if (cart is null)
        {
            return Result.Success();
        }

        /// update cart.Items to IsSoftDeleted to true;
        await context.CartItems.Where(x => x.CartId == command.CartId).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsSoftDeleted, true), cancellationToken);

        return Result.Success();
    }
}
