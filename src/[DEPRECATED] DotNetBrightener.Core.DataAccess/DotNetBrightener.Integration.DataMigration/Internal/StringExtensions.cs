using System.Diagnostics;
using System.Text.RegularExpressions;

internal static class StringExtensions
{
    [DebuggerStepThrough]
    public static bool IsMatch(this string  input, string pattern,
                               RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline)
    {
        return Regex.IsMatch(input, pattern, options);
    }

    [DebuggerStepThrough]
    public static bool IsMatch(this string  input, string pattern, out Match match,
                               RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline)
    {
        match = Regex.Match(input, pattern, options);
        return match.Success;
    }
}