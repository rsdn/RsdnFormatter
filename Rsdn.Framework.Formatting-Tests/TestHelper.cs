using System.IO;

using NUnit.Framework;

namespace Rsdn.Framework.Formatting.Tests
{
	public static class TestHelper
	{
		public static void TestFormat(string srcPath, string goldPath)
		{
			var formatter = new TextFormatter();
			string result;
			using (var reader = new StreamReader(srcPath))
				result = formatter.Format(reader.ReadToEnd());

			var fail = false;
			string resLine = "", goldLine = "";
			if (!File.Exists(goldPath))
				fail = true;
			else
				using (var resReader = new StringReader(result))
				using (var goldReader = new StreamReader(goldPath))
					while ((resLine = resReader.ReadLine()) != null)
					{
						goldLine = goldReader.ReadLine();
						if (resLine != goldLine)
						{
							fail = true;
							break;
						}
					}

			if (fail)
			{
				File.WriteAllText(goldPath + ".actual", result);
				Assert.Fail(
					string.Format(
						"Gold data differs from result.\r\n\tActualLine = '{0}'\r\n\tGoldLine = '{1}'",
						resLine,
						goldLine));
			}
		}
	}
}