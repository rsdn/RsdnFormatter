using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

using NUnit.Framework;

namespace Rsdn.Framework.Formatting.Tests
{
	internal delegate void TestDelegate(string srcPath, string goldPath);

	[TestFixture]
	public class FormatterTest
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		private void CallTest(TestDelegate testFunc)
		{
			var name = new StackTrace().GetFrame(1).GetMethod().Name;
			var asmPath =
				Path.Combine(
					Path.GetDirectoryName(
						new Uri(GetType().Assembly.CodeBase).AbsolutePath),
						"../../TestData");
			testFunc(
				Path.Combine(asmPath, name + ".txt"),
				Path.Combine(asmPath, name + ".gold"));
		}

		private void TestFormat()
		{
			CallTest(TestHelper.TestFormat);
		}

		private void TestQuote()
		{
			CallTest(TestHelper.TestQuote);
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
		[Test] public void SubSup() { TestFormat(); }
		[Test] public void Msg2408361() { TestFormat(); }
		[Test] public void ObjC() { TestFormat(); }
		[Test] public void MakeQuote() { TestQuote(); }
	}
}