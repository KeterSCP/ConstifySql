namespace ConstifySql.Tests.Utils;

internal static class StringExtensions
{
    public static string UseLfNewLine(this string input)
    {
        return input.Replace("\r\n", "\n");
    }
}