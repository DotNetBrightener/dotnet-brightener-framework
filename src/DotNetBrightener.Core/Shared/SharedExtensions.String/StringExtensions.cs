using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System;

internal static class StringExtensions
{
    /// <summary>
    ///     Splits a string into substrings based on the default separators with ";" and ",", and removes the empty entries
    /// </summary>
    /// <param name="inputString">The input string</param>
    /// <returns>
    ///     An array of substrings, or empty array if input string is empty.
    /// </returns>
    public static string[] SplitWithDefault(this string inputString)
    {
        if (string.IsNullOrEmpty(inputString))
            return new string[] { };

        return inputString.Split(new[]
                                 {
                                     ";", ","
                                 },
                                 StringSplitOptions.RemoveEmptyEntries);
    }

    public static string[] SplitWith(this string inputString, string[] separators)
    {
        return inputString.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Indicates whether this string is null or an System.String.Empty string.
    /// </summary>
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    /// <summary>
    /// indicates whether this string is null, empty, or consists only of white-space characters.
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    public static string GenerateRandomCode(int size)
    {
        var          random  = new Random();
        const string input   = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwyxz0123456789";
        var          builder = new StringBuilder();
        for (var i = 0; i < size; i++)
        {
            var ch = input[random.Next(0, input.Length)];
            builder.Append(ch);
        }

        return builder.ToString();
    }

    public static string Slugify(this string input)
    {
        var disallowed = new Regex(@"[/:?#\[\]@!$&'()*+,.;=\s\""\<\>\\\|%]+");

        var cleanedSlug = disallowed.Replace(input, "-").Trim('-', '.');

        var slug = Regex.Replace(cleanedSlug, @"\-{2,}", "-");

        if (slug.Length > 1000)
            slug = slug.Substring(0, 1000).Trim('-', '.');

        slug = slug.RemoveDiacritics();
        return slug;
    }


    public static string RemoveDiacritics(this string name)
    {
        var stFormD = name.Normalize(NormalizationForm.FormD);
        var sb      = new StringBuilder();

        foreach (var t in from t in stFormD
                          let uc = CharUnicodeInfo.GetUnicodeCategory(t)
                          where uc != UnicodeCategory.NonSpacingMark
                          select t)
        {
            sb.Append(t);
        }

        return (sb.ToString().Normalize(NormalizationForm.FormC));
    }

    public static string RemoveSpaces(this string format)
    {
        return Regex.Replace(format, @"\s+", "");
    }

    public static string CamelFriendly(this string camel)
    {
        if (String.IsNullOrWhiteSpace(camel))
            return "";

        var sb = new StringBuilder(camel);

        for (var i = camel.Length - 1; i > 0; i--)
        {
            var current = sb[i];
            if ('A' <= current && current <= 'Z')
            {
                sb.Insert(i, ' ');
            }
        }

        return sb.ToString();
    }

    [DebuggerStepThrough]
    public static string FormatCurrent(this string format, params object[] objects)
    {
        return string.Format(CultureInfo.CurrentCulture, format, objects);
    }

    [DebuggerStepThrough]
    public static bool HasValue(this string value)
    {
        return !string.IsNullOrEmpty(value);
    }


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