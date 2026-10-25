using Auth.Domain;

namespace Auth.Services;

// userId acompaña a sessionId en los 3 métodos siguientes — nunca solo sessionId. No es
// redundante: el JWT siempre trae ambos claims juntos ("sub" + "sid"), y exigir los dos cierra
// la posibilidad de que un sessionId adivinado/filtrado valide contra un "sub" que no es el
// suyo. También es lo que exige el esquema de keys de Redis (RedisSessionStore), que indexa
// por userId — ver tasks/plan-auth.md.
public interface ISessionStore
{
    Task<UserSession> CreateAsync(long userId, string ip, CancellationToken cancellationToken);

    Task<UserSession?> FindAsync(long userId, Guid sessionId, CancellationToken cancellationToken);

    Task TouchAsync(long userId, Guid sessionId, CancellationToken cancellationToken);

    Task RevokeAsync(long userId, Guid sessionId, CancellationToken cancellationToken);
}
