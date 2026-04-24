using NUnit.Framework;

namespace Rsdn.Framework.Formatting.Tests
{
	[TestFixture]
	public class FormatterTest
	{
		private static readonly HtmlComparer.HtmlComparer _htmlComparer = new();

		[Test, TestCaseSource(typeof(FormatterTestCaseSource))]
		public void Format(string markup, string expectedHtml)
		{
			var formatter = new TextFormatter();

			var output = formatter.Format(markup);
			var actualHtml = $"<html>\r\n\t<body>\r\n{output}\r\n\t</body>\r\n</html>";

			var result = _htmlComparer.Compare(expectedHtml, actualHtml);
			Assert.That(result.AreEqual, () => result.ToString());
		}
	}
}