using Users.Services;

namespace Users.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = PasswordHasher.Hash("correct-password");

        Assert.True(PasswordHasher.Verify("correct-password", hash));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = PasswordHasher.Hash("correct-password");

        Assert.False(PasswordHasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Verify_WithHashFromOlderIterationCount_StillWorks()
    {
        // Simula un hash emitido antes de subir las iteraciones a 600_000 (ver Users/Services/PasswordHasher.cs).
        // El iteration count viaja dentro del propio hash, así que subir la constante no invalida hashes viejos.
        var salt = Convert.ToBase64String(new byte[16]);
        var key = Convert.ToBase64String(
            System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
                "legacy-password",
                new byte[16],
                100_000,
                System.Security.Cryptography.HashAlgorithmName.SHA256,
                32));
        var legacyHash = $"100000.{salt}.{key}";

        Assert.True(PasswordHasher.Verify("legacy-password", legacyHash));
    }

    [Fact]
    public void Hash_CalledTwice_ProducesDifferentStrings()
    {
        var first = PasswordHasher.Hash("same-password");
        var second = PasswordHasher.Hash("same-password");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Verify_WithMalformedHash_ReturnsFalse()
    {
        Assert.False(PasswordHasher.Verify("any-password", "not-a-valid-hash"));
    }
}
