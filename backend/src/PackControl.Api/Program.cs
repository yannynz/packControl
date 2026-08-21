using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json.Serialization;
using PackControl.Api.Infrastructure;
using PackControl.Application.Abstractions;
using PackControl.Infrastructure;
using PackControl.Infrastructure.Persistence;
using PackControl.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.Local.json", optional: true, reloadOnChange: false);

builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();
builder.Services.AddPackControlInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "packcontrol.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            },
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:4200"];

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var statusCode = exception is InvalidOperationException
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;

        context.Response.StatusCode = statusCode;
        await Results.Problem(
            title: exception is InvalidOperationException ? "Operacao invalida" : "An error occurred while processing your request.",
            detail: exception?.Message,
            statusCode: statusCode).ExecuteAsync(context);
    });
});
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health/live", () => Results.Ok(new { status = "ok", service = "PackControl.Api" }))
    .AllowAnonymous();
app.MapGet("/health/ready", async (PlatformHealthService healthService, CancellationToken cancellationToken) =>
    {
        var report = await healthService.CheckAsync(cancellationToken);
        return report.Status == "ok"
            ? Results.Ok(report)
            : Results.Json(report, statusCode: StatusCodes.Status503ServiceUnavailable);
    })
    .AllowAnonymous();
app.MapGet("/health", async (PlatformHealthService healthService, CancellationToken cancellationToken) =>
    {
        var report = await healthService.CheckAsync(cancellationToken);
        return report.Status == "ok"
            ? Results.Ok(report)
            : Results.Json(report, statusCode: StatusCodes.Status503ServiceUnavailable);
    })
    .AllowAnonymous();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var stateStore = scope.ServiceProvider.GetRequiredService<AppStateStore>();
    var statePersistence = scope.ServiceProvider.GetRequiredService<IAppStatePersistence>();
    await statePersistence.LoadAsync(stateStore, CancellationToken.None);

    var seeder = scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>();
    await seeder.EnsureSeededAsync(CancellationToken.None);
    await statePersistence.SaveAsync(stateStore, CancellationToken.None);
}

app.Run();

public partial class Program;
