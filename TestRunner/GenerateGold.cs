using System.Reflection;
using System.Text;
using Rsdn.Framework.Formatting;

namespace TestRunner;

/// <summary>
/// Utility to generate gold files for tests
/// </summary>
public static class GenerateGold
{
    public static void GenerateAllGoldFiles()
    {
        var formatter = new TextFormatter();
        var testDataPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".",
            "..", "..", "..", "..", "Rsdn.Framework.Formatting-Tests", "TestData");

        Console.WriteLine($"Looking for test files in: {Path.GetFullPath(testDataPath)}");

        var txtFiles = Directory.GetFiles(testDataPath, "*.txt")
            .Where(f => !Path.GetFileName(f).StartsWith("_"))
            .OrderBy(f => f)
            .ToList();

        Console.WriteLine($"Found {txtFiles.Count} test files");

        foreach (var txtFile in txtFiles)
        {
            var goldFile = Path.ChangeExtension(txtFile, ".gold");
            var testName = Path.GetFileNameWithoutExtension(txtFile);

            Console.WriteLine($"Processing: {testName}");

            try
            {
                var input = File.ReadAllText(txtFile, Encoding.UTF8);
                var output = formatter.Format(input);

                // Wrap in HTML like the test does
                var html = $"<html>\r\n\t<body>\r\n{output}\r\n\t</body>\r\n</html>";

                File.WriteAllText(goldFile, html, Encoding.UTF8);
                Console.WriteLine($"  Generated: {Path.GetFileName(goldFile)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ERROR: {ex.Message}");
            }
        }

        Console.WriteLine("Done!");
    }
}