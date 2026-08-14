using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Interface;
using Application.Abstractions.Interface.Jobs;
using Application.Abstractions.Services;
using Application.Abstractions.Services.Notification;
using Application.Abstractions.Services.Order;
using Application.Abstractions.Services.Payments;
using Application.Abstractions.Services.Rider;
using Application.Abstractions.Services.UploadMedia;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using Infrastructure.BackgroundJobs;
using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Infrastructure.Hubs;
using Infrastructure.Services;
using Infrastructure.Services.ImageUpload;
using Infrastructure.Services.NotificationService;
using Infrastructure.Services.Payment;
using Infrastructure.Services.Rider;
using Infrastructure.Time;
using Infrastructure.UnitOfWork.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using SharedKernel;
using StackExchange.Redis;

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
            .AddRedisService(configuration)
            .AddFirebaseService(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IImageUploadService, CloudinaryImageUploadService>();

        services.AddTransient<IOtpHandler, OtpHandler>();
        services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();
        services.AddTransient<IRazorViewToString, RazorViewToStringService>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IHttpContextService, HttpContextService>();
        services.AddScoped<IPromoCodeService, PromoCodeService>();
        services.AddScoped<ICartService, CartService>();

        services.AddScoped<IDeliveryFeeService, DeliveryFeeService>();
        services.AddScoped<IDeliveryEstimateService, DeliveryEstimateService>();

        services.AddScoped<IRiderAssignmentService, RiderAssignmentService>();

        services.AddScoped<IRatingCalculator, RatingCalculator>();
        services.AddScoped<ITokenCache, RedisTokenCache>();

        services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        services.AddHttpClient<MonnifyPaymentGateway>();
        services.AddScoped<IPaymentGateway, MonnifyPaymentGateway>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

        services.AddKeyedScoped<IWebhookParser, StripeWebhookParser>("stripe");
        services.AddKeyedScoped<IWebhookParser, MonnifyWebhookParser>("monnify");
        services.AddScoped<ITrackingService, TrackingService>();
        services.AddScoped<IPaymentHubService, PaymentHubService>();
        services.AddScoped<IOrderHubService, OrderHubService>();

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

        var dataSourceBuilder =
        new NpgsqlDataSourceBuilder(connectionString);

        dataSourceBuilder.EnableDynamicJson();
        dataSourceBuilder.UseNetTopologySuite();

        NpgsqlDataSource dataSource = dataSourceBuilder.Build();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(dataSource, npgsqlOptions =>
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

    private static IServiceCollection AddRedisService(this IServiceCollection services, IConfiguration configuration)
    {
        string redisConnection = configuration.GetConnectionString("Redis")!;

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));

        services.AddSignalR().AddStackExchangeRedis(options =>
        {
            options.ConnectionFactory = async writer =>
            {
                ConnectionMultiplexer multiplexer = await ConnectionMultiplexer.ConnectAsync(redisConnection);

                multiplexer.ConnectionFailed += (_, e) =>
                {
                    writer.WriteLine($"Redis connection failed: {e.Exception?.Message}");
                };

                return multiplexer;
            };

            options.Configuration.ChannelPrefix = RedisChannel.Literal("signalr");
        });

        services.AddScoped<IRiderLocationCache, RiderLocationCache>();

        return services;
    }

    private static IServiceCollection AddFirebaseService(this IServiceCollection services, IConfiguration configuration)
    {
        string credentialPath = configuration["Firebase:ServiceAccountPath"]!;

        using var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read);

        var credential = ServiceAccountCredential.FromServiceAccountData(stream);

        FirebaseApp.Create(new AppOptions
        {
            Credential = credential.ToGoogleCredential()
        });

        services
            .AddScoped<IPushNotificationService, FcmPushNotificationService>()
            .AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
