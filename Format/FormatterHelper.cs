using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CodeJam.Strings;

namespace Rsdn.Framework.Formatting
{
	/// <summary>
	/// Helper methods for use with formatter.
	/// </summary>
	public static partial class FormatterHelper
	{
		/// <param name="sb">Исходный текст.</param>
		extension(StringBuilder sb)
		{
			private StringBuilder TrimLeft(char[] trimChars)
			{
				var spacesLen = 0;
				for (var i = 0; i < sb.Length; i++)
				{
					if (Array.IndexOf(trimChars, sb[i]) < 0)
						break;
					spacesLen++;
				}
				if (spacesLen > 0)
					sb.Remove(0, spacesLen);

				return sb;
			}

			private StringBuilder TrimRight(char[] trimChars)
			{
				var spacesLen = 0;
				for (var i = sb.Length - 1; i >= 0; i--)
				{
					if (Array.IndexOf(trimChars, sb[i]) < 0)
						break;
					spacesLen++;
				}
				if (spacesLen > 0)
					sb.Remove(sb.Length - spacesLen, spacesLen);

				return sb;
			}

			/// <summary>
			/// Trim sides of string.
			/// </summary>
			public StringBuilder Trim(char[] trimChars) =>
				sb == null
					? throw new ArgumentNullException(nameof(sb))
					: trimChars == null
						? throw new ArgumentNullException(nameof(trimChars))
						: sb.TrimLeft(trimChars).TrimRight(trimChars);

			/// <summary>
			/// Replace parts of StringBuilder by Regex.
			/// </summary>
			public StringBuilder Replace(string pattern, string replacement)
			{
				return sb == null
					? throw new ArgumentNullException(nameof(sb))
					: pattern == null
						? throw new ArgumentNullException(nameof(pattern))
						: replacement == null
							? throw new ArgumentNullException(nameof(replacement))
							: new StringBuilder(Regex.Replace(sb.ToString(), pattern, replacement));
			}

			/// <summary>
			/// Заменяет служебные символы HTML на их аналоги исключая '"'.
			/// </summary>
			/// <returns>Результат.</returns>
			public StringBuilder ReplaceTagsWQ() =>
				sb.IsEmpty()
					? sb
					: _ampersandDetector
						.Replace(sb, "&amp;")
						.Replace(">", "&gt;")
						.Replace("<", "&lt;");
		}

		/// <summary>
		/// Returns true is StringBuilder is empty.
		/// </summary>
		public static bool IsEmpty(this StringBuilder? sb) => sb == null || sb.Length == 0;

		extension(Regex regex)
		{
			/// <summary>
			/// Replace parts of StringBuilder by Regex.
			/// </summary>
			public StringBuilder Replace(StringBuilder input,
				string replacement) =>
				regex == null 
					? throw new ArgumentNullException(nameof(regex))
					: input == null
						? throw new ArgumentNullException(nameof(input))
						: replacement == null
							? throw new ArgumentNullException(nameof(replacement))
							: new StringBuilder(regex.Replace(input.ToString(), replacement));

			/// <summary>
			/// Replace parts of StringBuilder by Regex.
			/// </summary>
			public StringBuilder Replace(StringBuilder input,
				MatchEvaluator evaluator)
			{
				return regex == null
					? throw new ArgumentNullException(nameof(regex))
					: input == null
						? throw new ArgumentNullException(nameof(input))
						: evaluator == null
							? throw new ArgumentNullException(nameof(evaluator))
							: new StringBuilder(regex.Replace(input.ToString(), evaluator));
			}
		}

		private static readonly Regex _ampersandDetector =
			new Regex(
				@"&(?!#([0-9]+|x[0-9a-f]+);)",
				RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <param name="str">Исходный текст.</param>
		extension(string str)
		{
			/// <summary>
			/// Заменяет служебные символы HTML на их аналоги исключая '"'.
			/// </summary>
			/// <returns>Результат.</returns>
			public string ReplaceTagsWQ() =>
				str.IsNullOrEmpty()
					? str
					: _ampersandDetector
						.Replace(str, "&amp;")
						.Replace(">", "&gt;")
						.Replace("<", "&lt;");

			/// <summary>
			/// Заменяет служебные символы HTML на их аналоги.
			/// </summary>
			/// <returns>Результат.</returns>
			public string ReplaceTags()
			{
				return
					string.IsNullOrEmpty(str)
						? str
						: ReplaceTagsWQ(str).Replace("\"", "&quot;");
			}

			/// <summary>
			/// Подготавливает текст для JScript.
			/// </summary>
			/// <returns>Преобразованная строка.</returns>
			public string EncodeJScriptText() =>
				str.IsNullOrEmpty()
					? str
					: // Просто экранируем кавычки и слеши. 
					// Порядок важен: сначала обрабатываем слеши, если нужно, но для кавычек порядок не критичен,
					// так как заменяемые символы не пересекаются с результатами.
					str
						.Replace("\\", @"\\") // Экранируем обратный слеш
						.Replace("\"", "\\\"") // Экранируем двойную кавычку
						.Replace("'", "\\'"); // Экранируем одинарную кавычку

			/// <summary>
			/// Подготавливает url для предотвращения XSS (Cross Site Scripting)
			/// Используется для кодирования адресов (ссылок, картинок).
			/// </summary>
			/// <returns>Преобразованный url.</returns>
			public string EncodeUriAgainstXSS()
			{
				return MultiReplacer(str, _urlDangerCharsReplacer);
			}

			public string EncodeTextAgainstXSS() => MultiReplacer(str, _htmlDangerCharsReplacer);
		}

		private static string MultiReplacer(string src, Dictionary<char, string> replaceMap)
		{
			var result = new StringBuilder(src.Length);
			foreach (var ch in src)
				if (!replaceMap.TryGetValue(ch, out var repl))
					result.Append(ch);
				else
					result.Append(repl);
			return result.ToString();
		}

		// BASEDON: AngleSharp HtmlMarkupFormatter
		private static Dictionary<char, string> _htmlDangerCharsReplacer = 
			new()
			{
				{'&', "&amp;"},
				{'\u00A0', "&nbsp;"},
				{'>', "&gt;"},
				{'<', "&lt;"},
				{'\"', "&quot;"}
			};

		private static Dictionary<char, string> _urlDangerCharsReplacer =
			new()
			{
				{ ' ', "%20" },
				{ '\t', "%09" },
				{ '\'', "%27" },
				{ '\"', "%22" }
			};

		/// <summary>
		/// Преобразует object в int. 
		/// В случае возникновения исключения возвращается 0.
		/// </summary>
		/// <param name="o">Преобразуемый объект.</param>
		/// <returns>Результат.</returns>
		public static int ToInt(this object o) => o.ToInt(0);

		/// <param name="o">Преобразуемый объект.</param>
		extension(object? o)
		{
			/// <summary>
			/// Преобразует object в int. 
			/// В случае возникновения исключения возвращается errorValue.
			/// </summary>
			/// <param name="errorValue">Значение возвращаемое если произошла ошибка.</param>
			/// <returns>Результат.</returns>
			public int ToInt(int errorValue) =>
				o == null || string.Empty.Equals((string)o)
					? errorValue
					: o is int i
						? i : int.TryParse(o.ToString(), out var value)
							? value
							: errorValue;

			/// <summary>
			/// Преобразует object в double. 
			/// В случае возникновения исключения возвращается 0.
			/// </summary>
			/// <returns>Результат.</returns>
			public double ToDouble() =>
				o switch
				{
					null => 0,
					double d => d,
					_ => double.TryParse(o.ToString(), out var value) ? value : 0
				};

			/// <summary>
			/// Заменяет служебные символы HTML на их аналоги.
			/// </summary>
			/// <returns>Результат.</returns>
			public string? ReplaceTags() => o?.ToString().ReplaceTags();
		}

		/// <summary>
		/// Message tag extractor
		/// </summary>
		private static readonly Regex _tagsExtractor =
			new Regex(@"(?<tag>[^\s"",]+)|""(?<tag>.+?)""", RegexOptions.Compiled);

		/// <param name="tags"></param>
		extension(string tags)
		{
			/// <summary>
			/// Extract tags from string
			/// </summary>
			/// <returns></returns>
			public string[] ExtractTags()
			{
				if (tags.IsNullOrEmpty())
					return [];

				var mc = _tagsExtractor.Matches(tags);
				var exTags = new string[mc.Count];

				for (var i = 0; i < mc.Count; i++)
					exTags[i] = mc[i].Groups["tag"].ToString().ToLowerInvariant();

				return exTags;
			}

			/// <summary>
			/// Format tags
			/// </summary>
			/// <param name="eval">Tag transformer</param>
			public string ExtractTags(MatchEvaluator eval)
			{
				return
					string.IsNullOrEmpty(tags)
						? tags
						: _tagsExtractor.Replace(tags, eval);
			}
		}

		///<summary>
		/// Убирает цитирование из текста сообщения.
		///</summary>
		/// <param name="msg">Сообщение.</param>
		/// <returns>Обработанное сообщение</returns>
		public static string RemoveQuotations(string msg)
		{
			msg = TextFormatter.RemoveTaglineTag(msg);
			msg = TextFormatter.RemoveModeratorTag(msg);
			msg = Regex.Replace(msg, "Здравствуйте.*ы писали:", "");
			msg = msg.ReplaceTags();
			msg = Regex.Replace(msg, TextFormatter.StartCitation + ".*$", "", RegexOptions.Multiline);
			return msg;
		}
	}
}