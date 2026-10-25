using Auth.DTOs;
using Companies.Services;
using Users.Services;

namespace Auth.Services;

public sealed class LoginService
{
    private readonly FindUserCredentialsService _credentials;
    private readonly ISessionStore _sessions;
    private readonly ListMembershipsForUserService _memberships;

    public LoginService(FindUserCredentialsService credentials, ISessionStore sessions, ListMembershipsForUserService memberships)
    {
        _credentials = credentials;
        _sessions = sessions;
        _memberships = memberships;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var userId = await _credentials.VerifyCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (userId is null)
            throw new InvalidCredentialsException();

        // Puede lanzar SessionConflictException — se propaga tal cual, Api la mapea (Phase 4).
        var session = await _sessions.CreateAsync(userId.Value, request.Ip, cancellationToken);

        var memberships = await _memberships.ListActiveAsync(userId.Value, cancellationToken);

        return new LoginResult(userId.Value, session.SessionId, memberships);
    }
}
