using System.IO;
using System.Text;

using NUnit.Framework;

namespace Rsdn.Framework.Formatting.Tests
{
	public static class TestHelper
	{
		private static void CompareSamples(string goldPath, string result)
		{
			var fail = false;
			string resLine = "", goldLine = "";
			var lineNumber = 0;
			if (!File.Exists(goldPath))
				fail = true;
			else
				using (var resReader = new StringReader(result))
				using (var goldReader = new StreamReader(goldPath))
					while ((resLine = resReader.ReadLine()) != null)
					{
						lineNumber++;
						goldLine = goldReader.ReadLine();
						if (resLine != goldLine)
						{
							fail = true;
							break;
						}
					}

			if (!fail)
				return;

			File.WriteAllText(goldPath + ".actual", result);
			var sb = new StringBuilder("Gold data differs from result.\r\n");
			if (lineNumber > 0)
				sb.AppendFormat("LineNumber = {0}\r\n", lineNumber);
			if (resLine != "")
				sb.AppendFormat("ActualLine = '{0}'\r\n", resLine);
			if (goldLine != "")
				sb.AppendFormat("GoldLine   = '{0}'\r\n", goldLine);
			Assert.Fail(sb.ToString());
		}

		public static void TestFormat(string srcPath, string goldPath)
		{
			var formatter = new TextFormatter();
			string result;
			using (var reader = new StreamReader(srcPath))
				result = string.Format("<html>\r\n\t<body>\r\n{0}\r\n\t</body>\r\n</html>", formatter.Format(reader.ReadToEnd()));

			CompareSamples(goldPath, result);
		}

		public static void TestQuote(string srcPath, string goldPath)
		{
			string result;
			using (var reader = new StreamReader(srcPath))
				result = Format.Forum.GetEditMessage(reader.ReadToEnd(), "CaptainFlint");

			CompareSamples(goldPath, result);
		}
	}
}