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
    };
    private static readonly Dictionary<string, string> IllegalsWords = new()
    {
        {"SELECT","ERRORWORD1" },
        {"UNION","ERRORWORD2" },
        {"UPDATE","ERRORWORD3" },
        {"DELETE","ERRORWORD4" },
        {"SCRIPT","ERRORWORD5" },
    };    
    public static string EscapeString(string ToEscape)
    {
        string[] Words = ToEscape.Split(" ");

        for (int i = 0; i < Words.Length; i++)
        {
            string word = Words[i];
            
            if (IllegalsWords.TryGetValue(word.ToUpperInvariant(), out string? replacementWord))
            {
                Words[i] = replacementWord;

                continue;
            }
            
            string[] Chars = Words[i].Split("");

            for (int j = 0; j < Chars.Length; j++)
            {
                if (IllegalsChars.TryGetValue(Chars[j], out string? replacementChar))
                {
                    Chars[j] = replacementChar;

                    continue;
                }
            }
            Words[i] = string.Join("", Chars);
        };

        return String.Join(" ", Words);
    }
    public static void Test(string Illegal)
    {
        Console.WriteLine(EscapeString(Illegal != "" ? Illegal : "\\\\/////!!!!!------SELECT SELECT SELCT UNION THOAMS"));
    }
}