using System.Text.RegularExpressions;

namespace Gulla.Episerver.SqlStudio.Extensions
{
    public static class StringExtensions
    {
        public static string TrimSort(this string input)
        {
            return Regex.Replace(input, "^\\[\\d+\\]\\s*", "");
        }
    }
}
