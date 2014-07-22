using System.Text;

using NUnit.Framework;

namespace Rsdn.Framework.Formatting.Tests
{
	[TestFixture]
	public class FormatterHelperTest
	{
		[Test]
		public void Trim()
		{
			var trimChars = new[] {' ', '\r', '\n', '\t'};
			Assert.AreEqual("test", new StringBuilder("test").Trim(trimChars).ToString());
			Assert.AreEqual("test", new StringBuilder("  test").Trim(trimChars).ToString());
			Assert.AreEqual("test", new StringBuilder("test  ").Trim(trimChars).ToString());
			Assert.AreEqual("test", new StringBuilder("  test  ").Trim(trimChars).ToString());
			Assert.AreEqual("test", new StringBuilder(" \r\ntest\r\n").Trim(trimChars).ToString());
			Assert.AreEqual("test", new StringBuilder("\ttest\t   ").Trim(trimChars).ToString());
		}
	}
}