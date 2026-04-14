using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Restaurant.Dto;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Restaurant.DeleteMenuCategory;
internal sealed class DeleteMenuCategoryCommandHandlers(IDateTimeProvider dateTimeProvider, IApplicationDbContext context, IUserContext userContext) : ICommandHandler<DeleteMenuCategoryCommand>
{
    public async Task<Result> Handle(DeleteMenuCategoryCommand command, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;
        Domain.Category.Category? category = await context.Category.FirstOrDefaultAsync(x => x.Id == command.CategoryId, cancellationToken);

        if (category == null)
        {
            return Result.Failure<CategoryDto>(Domain.Common.CommonErrors.CustomErrorMessage("Category does not exist!"));
        }

        if (category.CreatedBy != userId.ToString())
        {
            return Result.Failure<CategoryDto>(Domain.Common.CommonErrors.CustomErrorMessage("You do not have permission to modify this category!"));
        }

        category.IsSoftDeleted = true;
        category.UpdatedBy = userId.ToString();
        category.UpdatedAt = dateTimeProvider.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
