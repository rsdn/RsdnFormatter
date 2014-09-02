using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rsdn.Framework.Formatting
{
	partial class Format
	{
		private static readonly IList<Func<StringBuilder, StringBuilder>> _inlineTagReplacers =
			TextFormatter.CreateInlineTagReplacers(false, RsdnTagToXhtmlTag);
		private static readonly Regex _hdrRegex = new Regex(@"^\s*<h[1-6].*</h[1-6]>\s*$", RegexOptions.Compiled);

		private static string RsdnTagToXhtmlTag(string tag)
		{
			switch (tag)
			{
				case "bold" :
					return "strong";
				case "i" :
					return "em";
				default:
					return tag;
			}
		}

		private static StringBuilder ProcessNewLines(StringBuilder src)
		{
			var rdr = new StringReader(src.ToString());
			string line;
			var res = new StringBuilder();
			while ((line = rdr.ReadLine()) != null)
				if (!_hdrRegex.IsMatch(line))
					res.AppendLine("<p>" + line + "</p>"); // 
				else
					res.AppendLine(line);
			return res;
		}

		public static string GetXhtml(string text)
		{
			var res = new StringBuilder(text);
			res = _inlineTagReplacers.Aggregate(res, (current, replacer) => replacer(current));
			res = TextFormatter.HeadersRegex.Replace(res, "<h$2 class='formatter'>$1</h$2>");
			res = ProcessNewLines(res);
			return res.ToString();
		}
	}
}
