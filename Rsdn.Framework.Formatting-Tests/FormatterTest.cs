using System;
using NUnit.Framework;

namespace Rsdn.Framework.Formatting.Tests
{
	internal delegate void TestDelegate(string srcPath, string goldPath);

	[TestFixture]
	public class FormatterTest
	{
				[Test, TestCaseSource(typeof(FormatterTestCaseSource))]
				public string[] Format(string markup)
				{
						var formatter = new TextFormatter();

						var output = formatter.Format(markup);
						var result = string.Format("<html>\r\n\t<body>\r\n{0}\r\n\t</body>\r\n</html>", output);

						return result.Split(new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				}
	}
}