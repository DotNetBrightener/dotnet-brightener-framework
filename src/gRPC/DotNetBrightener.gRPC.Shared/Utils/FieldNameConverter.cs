using System.Text;

internal static class StringExtensions
{
    public static string ToSnakeCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        StringBuilder result = new StringBuilder();

        for (int i = 0; i < str.Length; i++)
        {
            if (char.IsUpper(str[i]))
            {
                if (i > 0 &&
                    char.IsLower(str[i - 1]))
                {
                    result.Append('_');
                }

                result.Append(char.ToLowerInvariant(str[i]));
            }
            else
            {
                result.Append(str[i]);
            }
        }

        return result.ToString();
    }

    public static string ToTitleCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        StringBuilder result = new StringBuilder();

        for (int i = 0; i < str.Length; i++)
        {
            result.Append(i == 0 ? char.ToUpperInvariant(str[i]) : str[i]);
        }

        return result.ToString();
    }
}