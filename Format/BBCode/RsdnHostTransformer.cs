using System;
using System.Linq;
using CodeJam.Strings;
using Rsdn.Framework.Formatting.BBCode.Nodes;

namespace Rsdn.Framework.Formatting.BBCode
{
	/// <summary>
	/// Трансформатор AST для замены старых хостов RSDN на каноничный.
	/// Заменяет rsdn.ru, www.rsdn.ru, rsdn.rsdn.ru и т.д. на rsdn.org с https.
	/// </summary>
	public class RsdnHostTransformer : NodeVisitor
	{
		/// <summary>
		/// Каноничное имя хоста RSDN
		/// </summary>
		public string CanonicalHostName { get; set; } = Format.RsdnDomainName;

		/// <summary>
		/// Старые хосты, которые нужно заменять
		/// </summary>
		private static readonly string[] _oldHosts =
		[
			"rsdn.ru",
			"www.rsdn.ru",
			"rsdn.rsdn.ru",
			"rsdn3.rsdn.ru",
			"gzip.rsdn.ru",
			"svn.rsdn.ru"
		];

		/// <summary>
		/// Трансформировать документ, заменив старые хосты на каноничный
		/// </summary>
		public void Transform(DocumentNode document)
		{
			Visit(document);
		}

		public override void Visit(TagNode node)
		{
			// Обрабатываем URL в атрибуте тега
			if (node.TagName.Equals("url", StringComparison.OrdinalIgnoreCase) ||
			    node.TagName.Equals("img", StringComparison.OrdinalIgnoreCase))
			{
				if (node.Attribute.NotNullNorEmpty()) node.Attribute = ReplaceRsdnHost(node.Attribute);
			}

			// Обрабатываем детей
			base.Visit(node);
		}

		public override void Visit(TextNode node)
		{
			// Заменяем хост в тексте (для неявных ссылок в тексте)
			if (!string.IsNullOrEmpty(node.Text))
			{
				node.Text = ReplaceRsdnHostInText(node.Text);
			}
		}

		public override void Visit(QuoteLineNode node)
		{
			// Заменяем хост в тексте цитаты
			if (!string.IsNullOrEmpty(node.Text))
			{
				node.Text = ReplaceRsdnHostInText(node.Text);
			}
		}

		/// <summary>
		/// Заменить хост RSDN в URL
		/// </summary>
		private string ReplaceRsdnHost(string url)
		{
			if (string.IsNullOrEmpty(url))
				return url;

			foreach (var oldHost in _oldHosts)
			{
				var prefixes = new[] { "https://", "http://", "//" };
				foreach (var prefix in prefixes)
				{
					var fullPrefix = prefix + oldHost;
					if (url.StartsWith(fullPrefix, StringComparison.OrdinalIgnoreCase))
					{
						var rest = url.Substring(fullPrefix.Length);
						return "https://" + CanonicalHostName + rest;
					}
				}
			}

			return url;
		}

		/// <summary>
		/// Заменить хост RSDN в тексте (для неявных ссылок)
		/// </summary>
		private string ReplaceRsdnHostInText(string text)
		{
			if (string.IsNullOrEmpty(text))
				return text;

			foreach (var oldHost in _oldHosts)
			{
				var patterns = new[]
				{
					"https://" + oldHost,
					"http://" + oldHost
				};

				text = patterns
					.Aggregate(
						text,
						(current, pattern) => ReplaceIgnoreCase(
							current,
							pattern,
							"https://" + CanonicalHostName));
			}

			return text;
		}

		/// <summary>
		/// Замена без учёта регистра
		/// </summary>
		private static string ReplaceIgnoreCase(string text, string oldValue, string newValue)
		{
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(oldValue))
				return text;

			var result = new System.Text.StringBuilder();
			var currentIndex = 0;

			while (currentIndex < text.Length)
			{
				var foundIndex = IndexOfIgnoreCase(text, oldValue, currentIndex);
				if (foundIndex < 0)
				{
					result.Append(text.Substring(currentIndex));
					break;
				}

				result.Append(text.Substring(currentIndex, foundIndex - currentIndex));
				result.Append(newValue);
				currentIndex = foundIndex + oldValue.Length;
			}

			return result.ToString();
		}

		/// <summary>
		/// Поиск без учёта регистра
		/// </summary>
		private static int IndexOfIgnoreCase(string text, string value, int startIndex)
		{
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
				return -1;

			var textLower = text.ToLowerInvariant();
			var valueLower = value.ToLowerInvariant();

			return textLower.IndexOf(valueLower, startIndex, StringComparison.Ordinal);
		}
	}
}