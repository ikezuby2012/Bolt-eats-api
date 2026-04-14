using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.DeleteMenuItem;

internal sealed class DeleteMenuItemCommandHandler(IApplicationDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<DeleteMenuItemCommand>
{
    public async Task<Result> Handle(DeleteMenuItemCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Domain.MenuItem.MenuItem? menuItem = await context.MenuItem.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (menuItem == null)
        {
            return Result.Failure<MenuItemDto>(Domain.Common.CommonErrors.CustomErrorMessage("Menu Item does not exist!"));
        }

        if (menuItem.CreatedBy != userId.ToString())
        {
            return Result.Failure<MenuItemDto>(Domain.Common.CommonErrors.CustomErrorMessage("you did not have permission to update this menu item"));
        }

        menuItem.IsSoftDeleted = true;
        menuItem.UpdatedAt = dateTimeProvider.UtcNow;
        menuItem.UpdatedBy = userId.ToString();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
