using System.Text.RegularExpressions;

namespace Companies.Domain;

public sealed class User
{
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public long Id { get; private set; }
    public Guid PublicId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private User()
    {
    }

    public static User Create(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash is required.", nameof(passwordHash));

        return new User
        {
            Email = NormalizeEmail(email),
            PasswordHash = passwordHash,
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !EmailPattern.IsMatch(email))
            throw new ArgumentException("Email is invalid.", nameof(email));

        return email.Trim().ToLowerInvariant();
    }
}
