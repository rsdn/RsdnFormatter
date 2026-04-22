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
			Assert.That(new StringBuilder("test").Trim(trimChars).ToString(), Is.EqualTo("test"));
			Assert.That(new StringBuilder("  test").Trim(trimChars).ToString(), Is.EqualTo("test"));
			Assert.That(new StringBuilder("test  ").Trim(trimChars).ToString(), Is.EqualTo("test"));
			Assert.That(new StringBuilder("  test  ").Trim(trimChars).ToString(), Is.EqualTo("test"));
			Assert.That(new StringBuilder(" \r\ntest\r\n").Trim(trimChars).ToString(), Is.EqualTo("test"));
			Assert.That(new StringBuilder("\ttest\t   ").Trim(trimChars).ToString(), Is.EqualTo("test"));
		}
	}
}