using System.Linq.Expressions;
using Application.Abstractions.Data;
using Domain.Auth;
using Domain.Cart;
using Domain.Notification;
using Domain.Order;
using Domain.Payment;
using Domain.PromoCode;
using Domain.Rider;
using Domain.Users;
using Infrastructure.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SharedKernel;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventsDispatcher domainEventsDispatcher)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Domain.Address.Address> Addresses => Set<Domain.Address.Address>();
    public DbSet<Domain.Restaurant.Restaurant> Restaurants => Set<Domain.Restaurant.Restaurant>();
    public DbSet<Domain.Category.Category> Category => Set<Domain.Category.Category>();
    public DbSet<Domain.MenuItem.MenuItem> MenuItem => Set<Domain.MenuItem.MenuItem>();
    public DbSet<Domain.Review.Review> Review => Set<Domain.Review.Review>();
    public DbSet<Cart> Cart => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<PromoCode> PromoCode => Set<PromoCode>();
    public DbSet<PromoCodeUsage> PromoCodeUsages => Set<PromoCodeUsage>();
    public DbSet<Order> Order => Set<Order>();
    public DbSet<Payment> Payment => Set<Payment>();
    public DbSet<RiderLocation> RiderLocations => Set<RiderLocation>();

    public DbSet<Notification> Notification => Set<Notification>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    public DbSet<RiderProfile> RiderProfiles => Set<RiderProfile>();

    //public DatabaseFacade Database => base.Database;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);

        foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
            {
                ParameterExpression parameter = Expression.Parameter(entityType.ClrType, "e");
                LambdaExpression filter = Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, nameof(IAuditable.IsSoftDeleted)),
                        Expression.Constant(false)
                    ),
                    parameter
                );

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // When should you publish domain events?
        //
        // 1. BEFORE calling SaveChangesAsync
        //     - domain events are part of the same transaction
        //     - immediate consistency
        // 2. AFTER calling SaveChangesAsync
        //     - domain events are a separate transaction
        //     - eventual consistency
        //     - handlers can fail

        int result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync();

        return result;
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> domainEvents = entity.DomainEvents;

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .ToList();

        await domainEventsDispatcher.DispatchAsync(domainEvents);
    }
}
