using System.Text.RegularExpressions;

namespace PoliisiAppointment;

public static class StringExtension
{
    public static string RegexStringValue(this string input, string pattern)
    {
        var match = Regex.Match(input, pattern);
        return match.Success ? match.Groups["value"].Value : string.Empty;
    }
}