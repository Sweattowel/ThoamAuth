using System.Threading.Tasks;
using System.Windows.Markup;
using ThoamAuth.Helpers.Encryption;

namespace ThoamAuth.Helpers.AdminHelper;

public class AdminHelperClass
{
    private static string[]? SecretCode;
    public static void InitCode()
    {
        Console.WriteLine("Welcome to ThoamAuth \n Please Enter a secure server password");

        string ?CodeEntered;

        do
        {
            Console.WriteLine("Key must not be null or empty \n Should ideally have a length of 10+ Characters");
            CodeEntered = Console.ReadLine();
            
        } while (string.IsNullOrEmpty(CodeEntered));

        SecretCode = CreateSecretCode(CodeEntered);
    }
    public static string[] CreateSecretCode(string NewPass)
    {
        if (string.IsNullOrEmpty(NewPass))
        {
            string? Default = Environment.GetEnvironmentVariable("DEFAULT_SECRET_CODE");

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