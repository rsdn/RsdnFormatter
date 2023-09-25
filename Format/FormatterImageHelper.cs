using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace Rsdn.Framework.Formatting
{
	/// <summary>
	/// Helper class for image attributes formatting.
	/// </summary>
	public static class FormatterImageHelper
	{
		public struct ImgAttribute
		{
			public string Name;
			public string Value;
		}

		private static readonly Regex _attributesRegex =
			new Regex(@"(?<name>[\w]+\s*)=\s*(?<value>([\w]+|""[\w]+""|'[\w]+'))",
						RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static readonly Regex _attrNameRegex =
			new Regex(@"width|height",
						RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static readonly Regex _valueRegex =
			new Regex(@"^\d+(px|pt|mm|cm|in|em|rem|vh|vw|%)?$",
						RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>
		/// Only width and height attributes are allowed.
		/// </summary>
		public static bool IsValidAttribute(string name)
		{
			return _attrNameRegex.IsMatch(name);
		}

		/// <summary>
		/// restrict values to numbers with basic units
		/// </summary>
		public static bool IsValidValue(string val)
		{
			return _valueRegex.IsMatch(val);
		}

		/// <summary>
		/// collect image attributes from string
		/// </summary>
		public static IEnumerable<ImgAttribute> GetImageAttributes(string attributes)
		{
			yield return new ImgAttribute { Name = "border", Value = "0" };

			if (string.IsNullOrEmpty(attributes))
				yield break;

			var matches = _attributesRegex.Matches(attributes);
			foreach (var match in matches.Cast<Match>())
			{
				var name = match.Groups["name"].Value.Trim(' ');
				var value = match.Groups["value"].Value.Trim('\'', '\"');

				if (IsValidAttribute(name) && IsValidValue(value))
				{
					yield return new ImgAttribute { Name = name, Value = value };
				}
			}
		}

		/// <summary>
		/// render collected image attributes back as string
		/// </summary>
		public static string RenderImgAttributes(IEnumerable<ImgAttribute> attributes)
		{
			return string.Join(" ", attributes.Select(a => $"{a.Name}='{a.Value}'"));
		}
	}
}