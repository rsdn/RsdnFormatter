using NUnit.Framework;

namespace Rsdn.Framework.Formatting.Tests;

/// <summary>
/// Тесты для нового BBCode парсера (FormatBBCode)
/// Используют те же тестовые данные что и старые тесты
/// </summary>
[TestFixture]
public class BBCodeFormatterTest
{
	private static readonly HtmlComparer.HtmlComparer _htmlComparer = new();

	[Test, TestCaseSource(typeof(BBCodeFormatterTestCaseSource))]
	public void FormatBBCode(string markup, string expectedHtml)
	{
		var formatter = new TextFormatter();

		var output = formatter.FormatBBCode(markup);
		var actualHtml = $"<html>\r\n\t<body>\r\n{output}\r\n\t</body>\r\n</html>";

		var result = _htmlComparer.Compare(expectedHtml, actualHtml);
		Assert.That(result.AreEqual, () => result.ToString());
	}
}