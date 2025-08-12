using System.Security.Cryptography;
using System.Text;

namespace ThoamAuth.Helpers.Encryption;

public class EncryptionHelperClass
{
    public static string[] GenNewHash(string HashAttempt)
    {
        var Salt = GenSalt();
        var Pass = Encrypt(HashAttempt, Salt);

        if (Verify(HashAttempt, Pass, Salt))
        {
            return [Salt, Pass];
        }
        return [];
    }
    public static string Encrypt(string toEncrypt, string Salt)
    {
        var SaltBytes = Encoding.UTF8.GetBytes(Salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(toEncrypt, SaltBytes, 100_000, HashAlgorithmName.SHA3_256);

        var hash = pbkdf2.GetBytes(32);

        return Convert.ToBase64String(hash);
    }
    public static string GenSalt(int size = 32)
    {
        byte[] buffer = new byte[size];

        using RandomNumberGenerator rng = RandomNumberGenerator.Create();

        rng.GetBytes(buffer);

        return Convert.ToBase64String(buffer);
    }
    public static bool Verify(string attempt, string Encrypted, string Salt)
    {
        var HashedAttempt = Encrypt(attempt, Salt);

        var attemptBytes = Convert.FromBase64String(HashedAttempt);
        var encryptBytes = Convert.FromBase64String(Encrypted);

        if (!CryptographicOperations.FixedTimeEquals(attemptBytes, encryptBytes))
        {
            Logs.LogHelperClass.GenerateLog("Failed to Verify User", Models.Logs.LogStateEnum.Warning, Models.Logs.LogImportance.Low);

            return false;
        }
        return true;
    }
}