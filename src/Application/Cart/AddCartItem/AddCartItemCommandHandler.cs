using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Cart.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Cart.AddCartItem;

internal sealed class AddCartItemCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<AddCartItemCommand, CartDto>
{
    public async Task<Result<CartDto>> Handle(AddCartItemCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        Domain.MenuItem.MenuItem? menuItem = await context.MenuItem.Include(m => m.Restaurant).FirstOrDefaultAsync(m => m.Id == command.MenuItemId && m.IsAvailable, cancellationToken);

        if (menuItem is null)
        {
            return Result.Failure<CartDto>(Domain.Common.CommonErrors.CustomErrorMessage("Menu Item not found or available"));
        }

        if (!menuItem.Restaurant.IsActive || !menuItem.Restaurant.IsOpen)
        {
            return Result.Failure<CartDto>(Domain.Common.CommonErrors.CustomErrorMessage("Restaurant is currently closed"));
        }

        Domain.Cart.Cart? existingCart = await context.Cart.Include(x => x.Restaurant).Include(c => c.Items).ThenInclude(x => x.MenuItem).FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (existingCart is not null && existingCart.RestaurantId != menuItem.RestaurantId)
        {
            return Result.Failure<CartDto>(Domain.Common.CommonErrors.CustomErrorMessage($"Your cart has items from {existingCart.Restaurant.Name}. Adding items from {menuItem.Restaurant.Name} will clear your current cart."));
        }

        if (existingCart is null)
        {
            existingCart = new Domain.Cart.Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RestaurantId = menuItem.RestaurantId,
                PromoCode = command.PromoCode,
                CreatedBy = userId.ToString(),
                CreatedAt = dateTimeProvider.UtcNow,
                Items = []
            };

            await context.Cart.AddAsync(existingCart, cancellationToken);
        }

        Domain.Cart.CartItem? existingLine = existingCart.Items
            .FirstOrDefault(i => i.MenuItemId == command.MenuItemId);

        if (existingLine is not null)
        {
            existingLine.Quantity += command.Quantity;

            if (existingLine.Quantity > 20)
            {
                return Result.Failure<CartDto>(Domain.Common.CommonErrors.CustomErrorMessage("Maximum quantity of 20 per item exceeded."));
            }
        }
        else
        {
            existingCart.Items.Add(new Domain.Cart.CartItem
            {
                Id = Guid.NewGuid(),
                CartId = existingCart.Id,
                MenuItemId = menuItem.Id,
                Quantity = command.Quantity,
                UnitPrice = menuItem.DiscountPrice ?? menuItem.Price,
                Notes = command.Notes,
                CreatedBy = userId.ToString(),
                CreatedAt = dateTimeProvider.UtcNow,
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        //await context.Entry(existingCart)
        //    .Collection(c => c.Items)
        //    .Query()
        //    .Include(i => i.MenuItem)
        //    .LoadAsync(cancellationToken);

        return (CartDto)existingCart;

    }
}
