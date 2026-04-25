using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using NUnit.Framework;

using Rsdn.Framework.Formatting.Tests.TestData;

namespace Rsdn.Framework.Formatting.Tests
{
	/// <summary>
	/// Источник тестовых данных для нового BBCode форматтера (FormatBBCode)
	/// Использует те же входные данные что и FormatterTestCaseSource,
	/// но ожидаемые результаты из файлов .gold.new
	/// </summary>
	public class BBCodeFormatterTestCaseSource : IEnumerable
	{
		public IEnumerator GetEnumerator()
		{
			yield return GetTestCaseData("Cpp");
			yield return GetTestCaseData("Cut");
			yield return GetTestCaseData("ExcessiveBrs");
			yield return GetTestCaseData("Heading");
			yield return GetTestCaseData("Img");
			yield return GetTestCaseData("LinkJSInjection");
			yield return GetTestCaseData("MakeQuote").Ignore("TBD");
			yield return GetTestCaseData("Msg2408361");
			yield return GetTestCaseData("Nemerle");
			yield return GetTestCaseData("Nitra");
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
			testCaseData.SetName($"BBCode_{name}");

			return testCaseData;
		}
	}
}