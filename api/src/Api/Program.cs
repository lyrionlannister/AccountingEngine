using System.Net;
using System.Threading.RateLimiting;
using Accounting;
using Api.ExceptionHandling;
using Api.Hubs;
using Api.Middleware;
using Api.Security;
using Auth;
using Auth.Services;
using Companies;
using Companies.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Users;

var builder = WebApplication.CreateBuilder(args);

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("Missing configuration 'Jwt:SigningKey'.");
var jwtSigningKeyBytes = Convert.FromBase64String(jwtSigningKey);
if (jwtSigningKeyBytes.Length < 32)
    throw new InvalidOperationException("'Jwt:SigningKey' must decode to at least 32 bytes (256 bits) for HS256.");

builder.Services.AddOpenApi();
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddCompaniesModule(builder.Configuration);
builder.Services.AddAccountingModule(builder.Configuration);
builder.Services.AddAuthModule(builder.Configuration);

builder.Services.AddSingleton<JwtTokenIssuer>();
builder.Services.AddSingleton<Api.RateLimiting.LoginAccountRateLimiter>();
builder.Services.AddScoped<ISessionExpiryNotifier, SignalRSessionExpiryNotifier>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JwtTokenIssuer.Issuer,
            ValidateAudience = true,
            ValidAudience = JwtTokenIssuer.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtSigningKeyBytes)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<Api.Security.CompanyContextFilter>();
builder.Services.AddControllers(options => options.Filters.AddService<Api.Security.CompanyContextFilter>());
builder.Services.AddSignalR();

builder.Services.AddExceptionHandler<AccountingExceptionHandler>();
builder.Services.AddExceptionHandler<UsersExceptionHandler>();
builder.Services.AddExceptionHandler<AuthExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(15),
            PermitLimit = 10,
            QueueLimit = 0
        }));

    options.AddPolicy("register", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromHours(1),
            PermitLimit = 5,
            QueueLimit = 0
        }));
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Parse("172.28.0.10"));
});

var app = builder.Build();

var platformAdminEmail = app.Configuration["PlatformAdmin:Email"];
if (!string.IsNullOrWhiteSpace(platformAdminEmail))
{
    using var platformAdminScope = app.Services.CreateScope();
    await platformAdminScope.ServiceProvider.GetRequiredService<PlatformAdministratorBootstrapService>()
        .EnsureSeededAsync(platformAdminEmail, app.Configuration["PlatformAdmin:Password"], CancellationToken.None);
}

app.UseExceptionHandler();
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseMiddleware<SessionValidationMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SessionHub>("/hubs/sessions");

app.Run();

public partial class Program;
