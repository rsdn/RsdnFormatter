using System;
using System.Text;
using System.Text.RegularExpressions;

using Rsdn.Framework.Formatting.JetBrains.Annotations;

namespace Rsdn.Framework.Formatting
{
	/// <summary>
	/// Helper methods for use with formatter.
	/// </summary>
	public static partial class FormatterHelper
	{
		private static StringBuilder TrimLeft(
			[NotNull] this StringBuilder sb,
			[NotNull] char[] trimChars)
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

		private static StringBuilder TrimRight(
			[NotNull] this StringBuilder sb,
			[NotNull] char[] trimChars)
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
		public static StringBuilder Trim(
			[NotNull] this StringBuilder sb,
			[NotNull] char[] trimChars)
		{
			if (sb == null)
				throw new ArgumentNullException("sb");
			if (trimChars == null)
				throw new ArgumentNullException("trimChars");

			return sb.TrimLeft(trimChars).TrimRight(trimChars);
		}

		/// <summary>
		/// Returns true is StringBuilder is empty.
		/// </summary>
		public static bool IsEmpty([CanBeNull] this StringBuilder sb)
		{
			return sb == null || sb.Length == 0;
		}

		/// <summary>
		/// Replace parts of StringBuilder by Regex.
		/// </summary>
		public static StringBuilder Replace(
			[NotNull] this Regex regex,
			[NotNull] StringBuilder input,
			[NotNull] string replacement)
		{
			if (regex == null)
				throw new ArgumentNullException("regex");
			if (input == null)
				throw new ArgumentNullException("input");
			if (replacement == null)
				throw new ArgumentNullException("replacement");

			return new StringBuilder(regex.Replace(input.ToString(), replacement));
		}

		/// <summary>
		/// Replace parts of StringBuilder by Regex.
		/// </summary>
		public static StringBuilder Replace(
			[NotNull] this Regex regex,
			[NotNull] StringBuilder input,
			[NotNull] MatchEvaluator evaluator)
		{
			if (regex == null)
				throw new ArgumentNullException("regex");
			if (input == null)
				throw new ArgumentNullException("input");
			if (evaluator == null)
				throw new ArgumentNullException("evaluator");

			return new StringBuilder(regex.Replace(input.ToString(), evaluator));
		}

		/// <summary>
		/// Replace parts of StringBuilder by Regex.
		/// </summary>
		public static StringBuilder Replace(
			[NotNull] this StringBuilder input,
			[NotNull] string pattern,
			[NotNull] string replacement)
		{
			if (input == null)
				throw new ArgumentNullException("input");
			if (pattern == null)
				throw new ArgumentNullException("pattern");
			if (replacement == null)
				throw new ArgumentNullException("replacement");

			return new StringBuilder(Regex.Replace(input.ToString(), pattern, replacement));
		}

		private static readonly Regex _ampersandDetector =
			new Regex(
				@"&(?!#([0-9]+|x[0-9a-f]+);)",
				RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Заменяет служебные символы HTML на их аналоги исключая '"'.
		/// </summary>
		/// <param name="sb">Исходный текст.</param>
		/// <returns>Результат.</returns>
		public static StringBuilder ReplaceTagsWQ(this StringBuilder sb)
		{
			return
				sb.IsEmpty()
					? sb
					: _ampersandDetector
						.Replace(sb, "&amp;")
						.Replace(">", "&gt;")
						.Replace("<", "&lt;");
		}

		/// <summary>
		/// Заменяет служебные символы HTML на их аналоги исключая '"'.
		/// </summary>
		/// <param name="str">Исходный текст.</param>
		/// <returns>Результат.</returns>
		public static string ReplaceTagsWQ(this string str)
		{
			return
				string.IsNullOrEmpty(str)
					? str
					: _ampersandDetector.Replace(str, "&amp;")
						.Replace(">", "&gt;")
						.Replace("<", "&lt;");
		}

		/// <summary>
		/// Заменяет служебные символы HTML на их аналоги.
		/// </summary>
		/// <param name="str">Исходный текст.</param>
		/// <returns>Результат.</returns>
		public static string ReplaceTags(this string str)
		{
			return
				string.IsNullOrEmpty(str)
					? str
					: ReplaceTagsWQ(str).Replace("\"", "&quot;");
		}

		/// <summary>
		/// Заменяет служебные символы HTML на их аналоги.
		/// </summary>
		/// <param name="str">Исходный текст.</param>
		/// <returns>Результат.</returns>
		public static string ReplaceTags(this object str)
		{
			return str == null ? null : ReplaceTags(str.ToString());
		}

		/// <summary>
		/// Подготавливает текст для JScript.
		/// </summary>
		/// <warning>Текст не должен содержать спецмаркеры :quotes:, :apostroph: !</warning>
		/// <param name="str">Исходная строка.</param>
		/// <returns>Преобразованная строка.</returns>
		public static string EncodeJScriptText(this string str)
		{
			return
				string.IsNullOrEmpty(str)
				? str
				: str
					.Replace("\"", ":quotes:")
					.Replace("'", ":apostroph:")
					.Replace(@"\", @"\\")
					.Replace(":quotes:", @"\""")
					.Replace(":apostroph:", @"\'");
		}

		/// <summary>
		/// Подготавливает текст для предотовращения XSS (Cross Site Scripting)
		/// Используется, в основном для кодирования адресов (ссылок, картинок).
		/// </summary>
		/// <param name="value">Исходный текст.</param>
		/// <returns>Преобразованный текст.</returns>
		public static string EncodeAgainstXSS(this string value)
		{
			return
				string.IsNullOrEmpty(value)
					? value
					: value
						.Replace(" ", "%20")
						.Replace("\t", "%09")
						.Replace("\'", "%27");
		}

		/// <summary>
		/// Преобразует object в int. 
		/// В случае возникновения исключения возвращается 0.
		/// </summary>
		/// <param name="o">Преобразуемый объект.</param>
		/// <returns>Результат.</returns>
		public static int ToInt(this object o)
		{
			return ToInt(o, 0);
		}

		/// <summary>
		/// Преобразует object в int. 
		/// В случае возникновения исключения возвращается errorValue.
		/// </summary>
		/// <param name="o">Преобразуемый объект.</param>
		/// <param name="errorValue">Значение возвращаемое если произошла ошибка.</param>
		/// <returns>Результат.</returns>
		public static int ToInt(this object o, int errorValue)
		{
			if (o == null || string.Empty.Equals(o))
				return errorValue;

			if (o is int)
				return (int)o;

			int value;
			return int.TryParse(o.ToString(), out value) ? value : errorValue;
		}

		/// <summary>
		/// Преобразует object в double. 
		/// В случае возникновения исключения возвращается 0.
		/// </summary>
		/// <param name="o">Преобразуемый объект.</param>
		/// <returns>Результат.</returns>
		public static double ToDouble(this object o)
		{
			if (o == null)
				return 0;

			if (o is double)
				return (double)o;

			double value;
			return double.TryParse(o.ToString(), out value) ? value : 0;
		}


		/// <summary>
		/// Message tag extractor
		/// </summary>
		private static readonly Regex _tagsExtractor =
			new Regex(@"(?<tag>[^\s"",]+)|""(?<tag>.+?)""", RegexOptions.Compiled);

		/// <summary>
		/// Extract tags from string
		/// </summary>
		/// <param name="tags"></param>
		/// <returns></returns>
		public static string[] ExtractTags(this string tags)
		{
			if (string.IsNullOrEmpty(tags))
				return new string[0];

			var mc = _tagsExtractor.Matches(tags);
			var exTags = new string[mc.Count];

			for (var i = 0; i < mc.Count; i++)
				exTags[i] = mc[i].Groups["tag"].ToString().ToLowerInvariant();

			return exTags;
		}

		/// <summary>
		/// Format tags
		/// </summary>
		/// <param name="tags"></param>
		/// <param name="eval">Tag transformer</param>
		public static string ExtractTags(this string tags, MatchEvaluator eval)
		{
			return
				string.IsNullOrEmpty(tags)
					? tags
					: _tagsExtractor.Replace(tags, eval);
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