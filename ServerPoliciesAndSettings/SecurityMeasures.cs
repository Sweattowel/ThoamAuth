using System.Text;
using System.Text.RegularExpressions;

namespace ThoamAuth.ServerPoliciesAndSettings.SecurityMeasures;

public class SecurityMeasures
{
    private static readonly Dictionary<string, string> IllegalsChars = new()
    {
        {"\\","%5C" },
        {"-","%2D" },
        {"/","%2F" },
        {"!","%21" },
        {"&","%26" },
        {"'","%27" },
        {"=","%3D" },
        {"`","%60" },
        {"*","%2A" },
    };
    private static readonly Dictionary<string, string> IllegalsWords = new()
    {
        {"SELECT","ERRORWORD1" },
        {"UNION","ERRORWORD2" },
        {"UPDATE","ERRORWORD3" },
        {"DELETE","ERRORWORD4" },
        {"SCRIPT","ERRORWORD5" },
        {"TABLE","ERRORWORD6" },
    };
    public static string EscapeString(string ToEscape)
    {
        var sb = new StringBuilder(ToEscape.Length);

        foreach (char c in ToEscape)
        {
            string s = c.ToString();

            if (IllegalsChars.TryGetValue(s, out string? value))
                sb.Append(value);
            else
                sb.Append(c);
        }

        string charReplaced = sb.ToString();

        string pattern = String.Join("|", IllegalsWords.Keys);
        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        string result = regex.Replace(charReplaced, match =>
        {
            string key = match.Value.ToUpper();

            return IllegalsWords.ContainsKey(key) ? IllegalsWords[key] : match.Value;
        });

        return result;
    }
    public static void Test(string Illegal)
    {
        Console.WriteLine(EscapeString(Illegal != "" ? Illegal : "\\\\/////!!!!!------SELECT SELECT SELCT UNION THOAMS"));
    }
}