namespace Auth.Services;

// Mensaje deliberadamente genérico: email inexistente y contraseña incorrecta nunca se
// distinguen hacia el cliente (ver tasks/plan-auth.md, "Fallo de login").
public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Invalid credentials.")
    {
    }
}

// UserId/RejectedIp quedan como propiedades (no solo en el mensaje) para que el logging de
// auditoría del IExceptionHandler de Api los use sin parsear texto — nunca se expone al
// cliente. El texto de cara al usuario ("Ya existe una sesión activa...") lo decide el
// IExceptionHandler de Api, no esta excepción (ver tasks/plan-auth.md).
public sealed class SessionConflictException : Exception
{
    public long UserId { get; }
    public string RejectedIp { get; }

    public SessionConflictException(long userId, string rejectedIp)
        : base($"User {userId} already has an active session from a different IP; rejected login from {rejectedIp}.")
    {
        UserId = userId;
        RejectedIp = rejectedIp;
    }
}
