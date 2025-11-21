using System.Security.Cryptography;
using System.Text;

namespace PROG_MVC_POE_P2.Helpers;

public static class PasswordHelper
{
    public static (string hash, string salt) HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[16];
        rng.GetBytes(saltBytes);
        var salt = Convert.ToBase64String(saltBytes);

        using var sha = SHA256.Create();
        var combined = Encoding.UTF8.GetBytes(password + salt);
        var hashBytes = sha.ComputeHash(combined);
        var hash = Convert.ToBase64String(hashBytes);
        return (hash, salt);
    }

    public static bool VerifyPassword(string password, string salt, string hash)
    {
        using var sha = SHA256.Create();
        var combined = Encoding.UTF8.GetBytes(password + salt);
        var computed = Convert.ToBase64String(sha.ComputeHash(combined));
        return computed == hash;
    }
}
