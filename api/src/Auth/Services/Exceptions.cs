namespace Auth.Services;

// Mensaje deliberadamente genérico: email inexistente y contraseña incorrecta nunca se
// distinguen hacia el cliente (ver tasks/plan-auth.md, "Fallo de login").
public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Invalid credentials.")
    {
    }
}

// La IP rechazada viaja en el mensaje para logs técnicos del servidor — nunca se expone al
// cliente. El texto de cara al usuario ("Ya existe una sesión activa...") lo decide el
// IExceptionHandler de Api, no esta excepción (ver tasks/plan-auth.md).
public sealed class SessionConflictException : Exception
{
    public SessionConflictException(long userId, string rejectedIp)
        : base($"User {userId} already has an active session from a different IP; rejected login from {rejectedIp}.")
    {
    }
}
