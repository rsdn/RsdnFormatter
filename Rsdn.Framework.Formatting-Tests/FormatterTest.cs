using System;
using System.IO;

using NUnit.Framework;

namespace Rsdn.Framework.Formatting.Tests
{
	[TestFixture]
	public class FormatterTest
	{
		private void TestFormat(string fileName)
		{
			var asmPath =
				Path.Combine(
					Path.GetDirectoryName(
						new Uri(GetType().Assembly.CodeBase).AbsolutePath),
					"../../TestData");
			TestHelper.TestFormat(
				Path.Combine(asmPath, fileName + ".txt"),
				Path.Combine(asmPath, fileName + ".gold"));
		}

		[Test]
		public void SimpleFormatting()
		{
			TestFormat("SimpleFormatting");
		}
	}
}