using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Gulla.Episerver.SqlStudio.Extensions
{
    public static class StringExtensions
    {
        public static string TrimSort(this string input)
        {
            return Regex.Replace(input, "^\\[\\d+\\]\\s*", "");
        }

        public static string ToSqlCommentedLines(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            
            var lines = input.Split('\n');
            if (lines.Any())
            {
                var sb = new StringBuilder();
                for (var i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Trim().StartsWith("--"))
                    {
                        sb.Append(lines[i] + '\n');
                    }
                    else
                    {
                        sb.Append("-- " + lines[i] + '\n');
                    }
                }
                return sb.ToString();
            }
            else if (input.Trim().StartsWith("--"))
            {
                return input;
            }
            else
            {
                return "-- " + input;
            }            
        }
    }
}
