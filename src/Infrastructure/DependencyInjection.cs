using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Interface;
using Application.Abstractions.Interface.Jobs;
using Application.Abstractions.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using Infrastructure.BackgroundJobs;
using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Infrastructure.Services;
using Infrastructure.Time;
using Infrastructure.UnitOfWork.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddHangFire(configuration)
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddTransient<IOtpHandler, OtpHandler>();
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
        services.AddTransient<IRazorViewToString, RazorViewToStringService>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IHttpContextService, HttpContextService>();
        services.AddScoped<IPromoCodeService, PromoCodeService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IDeliveryFeeService, DeliveryFeeService>();
        services.AddScoped<IRiderAssignmentService, RiderAssignmentService>();
        services.AddScoped<IRatingCalculator, RatingCalculator>();

        return services;
    }

    private static IServiceCollection AddHangFire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config =>
        config.UsePostgreSqlStorage(options =>
            options.UseNpgsqlConnection(
                configuration.GetConnectionString("Database"))));

        services.AddHangfireServer();

        // Bind your Application interface → Hangfire adapter
        services.AddScoped<Application.Abstractions.Services.IBackgroundJobClient, HangfireBackgroundJobClient>();
        services.AddScoped<IReviewRatingUpdateJob, ReviewRatingUpdateJob>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions
                    .UseNetTopologySuite()
                    .MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        Schemas.Default
                    );
            })
            .UseSnakeCaseNamingConvention()
        );
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!);
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenProvider, TokenProvider>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        services.AddScoped<PermissionProvider>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }
}
