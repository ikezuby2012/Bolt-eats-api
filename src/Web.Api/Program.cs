using System.Reflection;
using Application;
using Asp.Versioning;
using Asp.Versioning.Builder;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Web.Api;
using Web.Api.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddSwaggerGenWithAuth();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options => options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDataProtection();

builder.Services
    .AddApplication()
    .AddPresentation()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
builder.Services.AddAntiforgery();

WebApplication app = builder.Build();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionedGroup = app
    .MapGroup("api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

app.MapEndpoints(versionedGroup);

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();

    app.ApplyMigrations();
}

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseCors("AllowAll");

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/api/ping", () => "Hello from Uber Eats API!");

app.MapControllers();

await app.RunAsync();

// REMARK: Required for functional and integration tests to work.
namespace Web.Api
{
    public partial class Program;
}
