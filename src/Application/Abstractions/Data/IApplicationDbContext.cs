using Domain.Address;
using Domain.Auth;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Domain.Restaurant.Restaurant> Restaurants { get; }
    DbSet<Domain.Category.Category> Category { get; }
    DbSet<Domain.MenuItem.MenuItem> MenuItem { get; }
    DbSet<Domain.Review.Review> Review { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
