using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace Rsdn.Framework.Formatting.Tests.HtmlComparer
{
	/// <summary>
	/// Компаратор HTML-документов с нормализацией.
	/// Игнорирует незначимые различия: порядок атрибутов, пробелы, регистр тегов/атрибутов.
	/// </summary>
	public class HtmlComparer
	{
		private readonly HtmlParser _parser = new();

		/// <summary>
		/// Сравнить два HTML-документа
		/// </summary>
		/// <param name="expected">Ожидаемый HTML</param>
		/// <param name="actual">Фактический HTML</param>
		/// <returns>Результат сравнения</returns>
		public HtmlCompareResult Compare(string expected, string actual)
		{
			var expectedDoc = _parser.ParseDocument(expected);
			var actualDoc = _parser.ParseDocument(actual);

			var differences = new List<HtmlDifference>();
			CompareNodes(expectedDoc.Body, actualDoc.Body, differences, "/html/body");

			return new HtmlCompareResult
			{
				AreEqual = differences.Count == 0,
				Differences = differences,
				ExpectedHtml = expected,
				ActualHtml = actual
			};
		}

		private void CompareNodes(IElement? expected, IElement? actual, List<HtmlDifference> differences, string path)
		{
			// Оба null - равны
			if (expected == null && actual == null)
				return;

			// Один null, другой нет
			if (expected == null)
			{
				differences.Add(new HtmlDifference(
					HtmlDifferenceKind.MissingElement,
					path,
					$"ожидается null, получен <{actual!.TagName.ToLowerInvariant()}>",
					GetElementContext(actual)));
				return;
			}
			if (actual == null)
			{
				differences.Add(new HtmlDifference(
					HtmlDifferenceKind.MissingElement,
					path,
					$"ожидается <{expected.TagName.ToLowerInvariant()}>, получен null",
					GetElementContext(expected)));
				return;
			}

			// Сравниваем имена тегов (без учёта регистра)
			var expectedTag = expected.TagName.ToLowerInvariant();
			var actualTag = actual.TagName.ToLowerInvariant();

			if (expectedTag != actualTag)
			{
				differences.Add(new HtmlDifference(
					HtmlDifferenceKind.DifferentTag,
					path,
					$"разные теги - ожидается <{expectedTag}>, получен <{actualTag}>",
					GetElementContext(expected),
					GetElementContext(actual)));
				return;
			}

			var currentPath = $"{path}/{expectedTag}";

			// Сравниваем атрибуты (нормализованные)
			var expectedAttrs = GetNormalizedAttributes(expected);
			var actualAttrs = GetNormalizedAttributes(actual);

			// Проверяем отсутствующие/разные атрибуты
			foreach (var attr in expectedAttrs)
			{
				if (!actualAttrs.TryGetValue(attr.Key, out var actualValue))
				{
					differences.Add(new HtmlDifference(
						HtmlDifferenceKind.MissingAttribute,
						currentPath,
						$"отсутствует атрибут '{attr.Key}' (ожидается '{attr.Value}')",
						GetElementContext(expected)));
				}
				else if (attr.Value != actualValue)
				{
					differences.Add(new HtmlDifference(
						HtmlDifferenceKind.DifferentAttributeValue,
						currentPath,
						$"атрибут '{attr.Key}' - ожидается '{attr.Value}', получен '{actualValue}'",
						GetElementContext(expected),
						GetElementContext(actual)));
				}
			}

			// Проверяем лишние атрибуты
			foreach (var attr in actualAttrs)
			{
				if (!expectedAttrs.ContainsKey(attr.Key))
				{
					differences.Add(new HtmlDifference(
						HtmlDifferenceKind.ExtraAttribute,
						currentPath,
						$"лишний атрибут '{attr.Key}'='{attr.Value}'",
						null,
						GetElementContext(actual)));
				}
			}

			// Сравниваем текстовое содержимое (нормализованное)
			var expectedText = GetNormalizedText(expected);
			var actualText = GetNormalizedText(actual);

			if (expectedText != actualText)
			{
				// Различие в тексте - добавляем только если это не просто пробелы
				if (!string.IsNullOrWhiteSpace(expectedText) || !string.IsNullOrWhiteSpace(actualText))
				{
					differences.Add(new HtmlDifference(
						HtmlDifferenceKind.DifferentText,
						currentPath,
						$"текст - ожидается '{Truncate(expectedText, 100)}', получен '{Truncate(actualText, 100)}'",
						GetElementContext(expected),
						GetElementContext(actual)));
				}
			}

			// Сравниваем дочерние элементы
			var expectedChildren = GetElementChildren(expected);
			var actualChildren = GetElementChildren(actual);

			var maxChildren = Math.Max(expectedChildren.Count, actualChildren.Count);
			for (int i = 0; i < maxChildren; i++)
			{
				var expectedChild = i < expectedChildren.Count ? expectedChildren[i] : null;
				var actualChild = i < actualChildren.Count ? actualChildren[i] : null;

				CompareNodes(expectedChild, actualChild, differences, $"{currentPath}[{i}]");
			}
		}

		/// <summary>
		/// Получить контекст элемента (его HTML представление с родителем)
		/// </summary>
		private static string? GetElementContext(IElement? element)
		{
			if (element == null)
				return null;

			var sb = new StringBuilder();
			
			// Добавляем родительский элемент для контекста
			if (element.ParentElement != null && element.ParentElement.TagName != "HTML" && element.ParentElement.TagName != "BODY")
			{
				var parentHtml = element.ParentElement.OuterHtml;
				if (parentHtml.Length > 200)
					parentHtml = parentHtml.Substring(0, 200) + "...";
				sb.AppendLine("Родитель: " + parentHtml);
			}

			// Добавляем сам элемент
			var elementHtml = element.OuterHtml;
			if (elementHtml.Length > 300)
				elementHtml = elementHtml.Substring(0, 300) + "...";
			sb.Append("Элемент: " + elementHtml);

			return sb.ToString();
		}

		/// <summary>
		/// Получить нормализованные атрибуты (имя в нижнем регистре, отсортированы)
		/// </summary>
		private static Dictionary<string, string> GetNormalizedAttributes(IElement element)
		{
			var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (var attr in element.Attributes)
			{
				var name = attr.Name.ToLowerInvariant();
				var value = attr.Value.Trim();
				result[name] = value;
			}

			return result;
		}

		/// <summary>
		/// Получить нормализованный текст (без лишних пробелов)
		/// </summary>
		private static string GetNormalizedText(IElement element)
		{
			var text = new StringBuilder();
			GetTextContent(element, text);
			return NormalizeWhitespace(text.ToString());
		}

		private static void GetTextContent(IElement element, StringBuilder text)
		{
			foreach (var node in element.ChildNodes)
			{
				if (node is IText textNode)
				{
					text.Append(textNode.Text);
				}
				else if (node is IElement childElement)
				{
					GetTextContent(childElement, text);
				}
			}
		}

		/// <summary>
		/// Нормализовать пробелы
		/// </summary>
		private static string NormalizeWhitespace(string? text)
		{
			if (string.IsNullOrEmpty(text))
				return "";

			// Заменяем все пробельные символы на один пробел
			var result = new StringBuilder();
			bool inWhitespace = false;

			foreach (var c in text)
			{
				if (char.IsWhiteSpace(c))
				{
					if (!inWhitespace)
					{
						result.Append(' ');
						inWhitespace = true;
					}
				}
				else
				{
					result.Append(c);
					inWhitespace = false;
				}
			}

			return result.ToString().Trim();
		}

		/// <summary>
		/// Получить только дочерние элементы (не текстовые узлы)
		/// </summary>
		private static List<IElement> GetElementChildren(IElement element)
		{
			return element.Children.ToList();
		}

		/// <summary>
		/// Обрезать строку до указанной длины
		/// </summary>
		private static string Truncate(string? text, int maxLength)
		{
			if (string.IsNullOrEmpty(text))
				return "";

			if (text.Length <= maxLength)
				return text;

			return text.Substring(0, maxLength) + "...";
		}
	}
}