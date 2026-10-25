namespace Users.Services;

public sealed class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email) : base($"A user with email '{email}' already exists.")
    {
    }
}
