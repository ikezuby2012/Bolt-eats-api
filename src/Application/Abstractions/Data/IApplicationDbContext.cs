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
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
