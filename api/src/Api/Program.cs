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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Users;

var builder = WebApplication.CreateBuilder(args);

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("Missing configuration 'Jwt:SigningKey'.");

builder.Services.AddOpenApi();
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddCompaniesModule(builder.Configuration);
builder.Services.AddAccountingModule(builder.Configuration);
builder.Services.AddAuthModule(builder.Configuration);

builder.Services.AddSingleton<JwtTokenIssuer>();
builder.Services.AddScoped<ISessionExpiryNotifier, SignalRSessionExpiryNotifier>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Sin esto, JwtBearerHandler remapea "sub" al tipo de claim legado
        // (nameidentifier), y SessionValidationMiddleware/AuthController, que leen
        // JwtRegisteredClaimNames.Sub tal cual, nunca lo encuentran.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtSigningKey))
        };

        // El WebSocket nativo del navegador no puede mandar el header Authorization —
        // SignalR manda el token por query string en su lugar (patrón estándar de ASP.NET Core).
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

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddExceptionHandler<AuthExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddRateLimiter(options =>
{
    // Por defecto ASP.NET Core rechaza con 503; 429 es el código HTTP correcto para "demasiadas
    // solicitudes" y el que un cliente esperaría interpretar (Retry-After, etc.).
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Partición por IP: un bucket global compartido dejaría que un solo cliente agote el
    // límite para todos los demás (lo contrario de lo que un rate limit de login busca).
    // En memoria — válido mientras "api" sea una sola instancia (ver compose.yaml); migrar a
    // un limiter respaldado por Redis (ya disponible) si se escala horizontalmente.
    options.AddPolicy("login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(15),
            PermitLimit = 10,
            QueueLimit = 0
        }));
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Confía solo en la red dedicada de compose.yaml (nginx), no en el rango por defecto de
    // Docker — ver tasks/plan-auth.md.
    options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("172.28.0.0"), 24));
});

var app = builder.Build();

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
