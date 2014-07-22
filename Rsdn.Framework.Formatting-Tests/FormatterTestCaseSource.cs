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
            yield return GetTestCaseData("Cpp");
            yield return GetTestCaseData("Cut");
            yield return GetTestCaseData("ExcessiveBrs");
            yield return GetTestCaseData("Heading");
            yield return GetTestCaseData("Msg2408361");
        }

        private static TestCaseData GetTestCaseData(string name)
        {
            var originalStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(_Dummy), name + ".txt");
            var goldStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(_Dummy), name + ".gold");
            
            Debug.Assert(originalStream != null, "originalStream != null");
            Debug.Assert(goldStream != null, "goldStream != null");

            string original;
            string gold;

            using(var streamReader = new StreamReader(originalStream, Encoding.UTF8))
                original = streamReader.ReadToEnd();

            using(var streamReader = new StreamReader(goldStream, Encoding.UTF8))
                gold = streamReader.ReadToEnd();

            var testCaseData = new TestCaseData(original);

            testCaseData.SetName(name);
            testCaseData.Returns(gold);

            return testCaseData;
        }
    }
}