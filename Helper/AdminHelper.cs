using System.Windows.Markup;
using ThoamAuth.Helpers.Encryption;

namespace ThoamAuth.Helpers.AdminHelper;

public class AdminHelperClass
{
    private static readonly string[] SecretCode = CreateSecretCode(Console.ReadLine());
    public static string[] CreateSecretCode(string NewPass)
    {
        if (string.IsNullOrEmpty(NewPass))
        {
            string ?Default = Environment.GetEnvironmentVariable("DEFAULT_SECRET_CODE");

            return EncryptionHelperClass.GenNewHash(Default);    
        }
        return EncryptionHelperClass.GenNewHash(NewPass);
    }
    public static bool VerifySecretCode(string SecretCodeAttempt)
    {
        if (SecretCode[0] == "" || SecretCode[1] == "") { return false; }

        return EncryptionHelperClass.Verify(SecretCodeAttempt, SecretCode[1], SecretCode[0]);
    }
}