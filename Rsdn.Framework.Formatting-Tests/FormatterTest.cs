using System;

using NUnit.Framework;

namespace Rsdn.Framework.Formatting.Tests
{
	[TestFixture]
	public class FormatterTest
	{
		[Test, TestCaseSource(typeof (FormatterTestCaseSource))]
		public string[] Format(string markup)
		{
			var formatter = new TextFormatter();

			var output = formatter.Format(markup);
			var result = $"<html>\r\n\t<body>\r\n{output}\r\n\t</body>\r\n</html>";

			return result.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}