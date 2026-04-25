using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using NUnit.Framework;

using Rsdn.Framework.Formatting.Tests.TestData;

namespace Rsdn.Framework.Formatting.Tests
{
	public class FormatterTestCaseSource : IEnumerable
	{
		public IEnumerator GetEnumerator()
		{
			// Original tests
			yield return GetTestCaseData("Img");
			yield return GetTestCaseData("Nitra");
			yield return GetTestCaseData("Nemerle");
			yield return GetTestCaseData("Cpp");
			yield return GetTestCaseData("Cut");
			yield return GetTestCaseData("ExcessiveBrs");
			yield return GetTestCaseData("Heading");
			yield return GetTestCaseData("MakeQuote").Ignore("TBD");
			yield return GetTestCaseData("Msg2408361");
			yield return GetTestCaseData("ObjC");
			yield return GetTestCaseData("Quotation");
			yield return GetTestCaseData("Quotation2");
			yield return GetTestCaseData("RsdnLink");
			yield return GetTestCaseData("Rust");
			yield return GetTestCaseData("SimpleFormatting");
			yield return GetTestCaseData("Smiles");
			yield return GetTestCaseData("Sql");
			yield return GetTestCaseData("SubSup");
			yield return GetTestCaseData("Urls");
			yield return GetTestCaseData("XSS");
			yield return GetTestCaseData("LinkJSInjection");

			// New BBCode tests
			yield return GetTestCaseData("Lists");
			yield return GetTestCaseData("Tables");
			yield return GetTestCaseData("Email");
			yield return GetTestCaseData("Quote");
			yield return GetTestCaseData("InlineQuote");
			yield return GetTestCaseData("Hr");
			yield return GetTestCaseData("Msdn");
			yield return GetTestCaseData("Purl");
			yield return GetTestCaseData("ImplicitUrls");
			yield return GetTestCaseData("Moderator");
			yield return GetTestCaseData("Tagline");
			yield return GetTestCaseData("Tt");

			// Additional smiles test
			yield return GetTestCaseData("MoreSmiles");

			// New language tests
			yield return GetTestCaseData("CSharp");
			yield return GetTestCaseData("Java");
			yield return GetTestCaseData("Python");
			yield return GetTestCaseData("Pascal");
			yield return GetTestCaseData("VisualBasic");
			yield return GetTestCaseData("PHP");
			yield return GetTestCaseData("Perl");
			yield return GetTestCaseData("Ruby");
			yield return GetTestCaseData("Assembler");
			yield return GetTestCaseData("IDL");
			yield return GetTestCaseData("MSIL");
			yield return GetTestCaseData("XSL");
			yield return GetTestCaseData("Erlang");
			yield return GetTestCaseData("Haskell");
			yield return GetTestCaseData("Lisp");
			yield return GetTestCaseData("Ocaml");
			yield return GetTestCaseData("Prolog");
		}

		private static TestCaseData GetTestCaseData(string name)
		{
			var asm = Assembly.GetExecutingAssembly();
			var originalStream = asm.GetManifestResourceStream(typeof(_Dummy), name + ".txt");
			var goldStream = asm.GetManifestResourceStream(typeof(_Dummy), name + ".gold");

			Debug.Assert(originalStream != null, $"originalStream != null for {name} test case");
			Debug.Assert(goldStream != null);

			string original;
			string gold;

			using (var streamReader = new StreamReader(originalStream, Encoding.UTF8))
				original = streamReader.ReadToEnd();

			using (var streamReader = new StreamReader(goldStream, Encoding.UTF8))
				gold = streamReader.ReadToEnd();

			// Возвращаем два параметра: markup и expectedHtml
			var testCaseData = new TestCaseData(original, gold);
			testCaseData.SetName(name);

			return testCaseData;
		}
	}
}