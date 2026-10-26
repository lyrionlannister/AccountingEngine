using System.Threading.RateLimiting;

namespace Api.RateLimiting;

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
