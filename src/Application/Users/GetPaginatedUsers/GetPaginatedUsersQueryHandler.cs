using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Users.Dto;
using Domain.Address;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.GetPaginatedUsers;

internal sealed class GetPaginatedUsersQueryHandler(IApplicationDbContext context) : IQueryHandler<GetPaginatedUsersQuery, PaginatedResult<UserDto>>
{
    public async Task<Result<PaginatedResult<UserDto>>> Handle(GetPaginatedUsersQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Users.User> query = context.Users.AsNoTracking().AsQueryable().Include(x => x.Addresses)
            .Where(x => (!request.DateFrom.HasValue || x.CreatedAt >= request.DateFrom.Value) &&
                   (!request.DateTo.HasValue || x.CreatedAt <= request.DateTo.Value));

        int totalItems = await query.CountAsync(cancellationToken);

        List<UserDto> allUsers = await query.OrderByDescending(m => m.CreatedAt)
            .Skip((request.pageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(user => new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                IsVerified = user.isVerifed,
                RoleId = user.RoleId,
                UserRole = user.RoleId.HasValue ? UserRole.FromValue(user.RoleId.Value)!.Name : "User",
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                CreatedById = user.CreatedById,
                Addresses = (user.Addresses ?? Enumerable.Empty<Address>()).Select(address => new AddressDto
                {
                    Id = address.Id,
                    UserId = address.UserId ?? Guid.NewGuid(),
                    Label = address.Label,
                    Street = address.Street,
                    City = address.City,
                    State = address.State,
                    Country = address.Country,
                    PostalCode = address.PostalCode,
                    Latitude = address.Latitude,
                    Longitude = address.Longitude,
                    LatitudeRaw = address.LatitudeRaw,
                    LongitudeRaw = address.LongitudeRaw,
                    IsDefault = address.IsDefault,
                    CreatedAt = address.CreatedAt,
                    UpdatedAt = address.UpdatedAt
                }).ToList()
            }).ToListAsync(cancellationToken);

        return new PaginatedResult<UserDto>
        {
            Data = allUsers,
            TotalItems = totalItems,
            PageSize = request.PageSize,
            PageNumber = request.pageNumber,
        };
    }
}
