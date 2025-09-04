using System.Text;
using System.Text.RegularExpressions;

namespace ThoamAuth.ServerPoliciesAndSettings.SecurityMeasures;

public class SecurityMeasures
{
    private static readonly Dictionary<string, string> Illegals = new()
    {
        // Chars
        { "\\","%5C" },
        {"-","%2D" },
        {"/","%2F" },
        {"!","%21" },
        {"&","%26" },
        {"'","%27" },
        {"=","%3D" },
        {"`","%60" },
        {"*","%2A" },
        // Words
        { "SELECT","ERRORWORD1" },
        {"UNION","ERRORWORD2" },
        {"UPDATE","ERRORWORD3" },
        {"DELETE","ERRORWORD4" },
        {"SCRIPT","ERRORWORD5" },
        {"TABLE","ERRORWORD6" },
    };
    public static string EscapeString(string ToEscape)
    {

        string pattern = String.Join("|", Illegals.Keys);
        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        string result = regex.Replace(ToEscape, match =>
        {
            string key = match.Value.ToUpper();

            return Illegals.ContainsKey(key) ? Illegals[key] : match.Value;
        });

        return result;
        
    }
    public static void Test(string Illegal)
    {
        Console.WriteLine(EscapeString(Illegal != "" ? Illegal : "\\\\/////!!!!!------SELECT SELECT SELCT UNION THOAMS"));
    }
}