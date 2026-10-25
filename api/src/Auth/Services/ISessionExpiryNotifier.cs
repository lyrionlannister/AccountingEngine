namespace Auth.Services;

// Lo implementa Api vía SignalR (Auth nunca referencia Microsoft.AspNetCore.SignalR —
// sigue siendo transporte, CLAUDE.md §3.7). Lo llama RedisSessionExpiryListener cuando una
// key "session:{userId}:{sessionId}" expira — ver tasks/plan-auth.md, Phase 3.
public interface ISessionExpiryNotifier
{
    Task NotifyExpiredAsync(long userId, Guid sessionId, CancellationToken cancellationToken);
}
