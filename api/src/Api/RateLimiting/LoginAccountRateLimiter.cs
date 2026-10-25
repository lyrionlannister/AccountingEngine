using System.Threading.RateLimiting;

namespace Api.RateLimiting;

// Límite por cuenta, además del límite por IP ya aplicado en Program.cs vía [EnableRateLimiting]:
// sin esto, un atacante con muchas IPs puede intentar fuerza bruta ilimitada contra una sola
// cuenta (10 intentos por IP, sin tope agregado). Vive fuera de Microsoft.AspNetCore.RateLimiting
// porque la partición necesita el email ya parseado del body, que el middleware de rate
// limiting (que corre antes del model binding) todavía no tiene disponible.
public sealed class LoginAccountRateLimiter : IDisposable
{
    private readonly PartitionedRateLimiter<string> _limiter = PartitionedRateLimiter.Create<string, string>(email =>
        RateLimitPartition.GetFixedWindowLimiter(email, _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(15),
            PermitLimit = 10,
            QueueLimit = 0
        }));

    public bool TryAcquire(string email) => _limiter.AttemptAcquire(email.Trim().ToLowerInvariant()).IsAcquired;

    public void Dispose() => _limiter.Dispose();
}
