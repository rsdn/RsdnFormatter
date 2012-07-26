using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

using NUnit.Framework;

namespace Rsdn.Framework.Formatting.Tests
{
	[TestFixture]
	public class FormatterTest
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		private void TestFormat()
		{
			var name = new StackTrace().GetFrame(1).GetMethod().Name;
			var asmPath =
				Path.Combine(
					Path.GetDirectoryName(
						new Uri(GetType().Assembly.CodeBase).AbsolutePath),
						"../../TestData");
			TestHelper.TestFormat(
				Path.Combine(asmPath, name + ".txt"),
				Path.Combine(asmPath, name + ".gold"));
		}

		[Test] public void SimpleFormatting() { TestFormat(); }
		[Test] public void Heading() { TestFormat(); }
		[Test] public void Quotation() { TestFormat(); }
		[Test] public void RsdnLink() { TestFormat(); }
		[Test] public void Smiles() { TestFormat(); }
		[Test] public void Urls() { TestFormat(); }
		[Test] public void XSS() { TestFormat(); }
		[Test] public void Sql() { TestFormat(); }
		[Test] public void Cut() { TestFormat(); }
		[Test] public void Cpp() { TestFormat(); }
	}
}