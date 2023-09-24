using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace Rsdn.Framework.Formatting
{
    public class FormatterImageHelper
    {
        public static bool IsValidAttribute(string name)
        {
            return Regex.IsMatch(name, "width|height", RegexOptions.IgnoreCase);
        }

        public static bool IsValidValue(string val)
        {
            return Regex.IsMatch(val, @"^\d+(px|pt|mm|cm|in|em|rem|vh|vw|%)?$", RegexOptions.IgnoreCase);
        }

        public static IEnumerable<Tuple<string, string>> GetImageAttributes(string attributes)
        {
            yield return Tuple.Create("border", "0");

            var matches = Regex.Matches(attributes, @"(?<name>[\w]+\s*)=\s*(?<value>([\w]+|""[\w]+""|'[\w]+'))");
            foreach (var match in matches.Cast<Match>())
            {
                var name = match.Groups["name"].Value.Trim(' ');
                var value = match.Groups["value"].Value.Trim('\'', '\"');

                if (IsValidAttribute(name) && IsValidValue(value))
                {
                    yield return Tuple.Create(name, value);
                }
            }
        }

        public static string RenderImgAttributes(IEnumerable<Tuple<string, string>> attributes)
        {
            return string.Join(" ", attributes.Select(a => $"{a.Item1}='{a.Item2}'"));
        }
    }
}