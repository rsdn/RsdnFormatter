using Rsdn.Framework.Formatting;

namespace HtmlGenerator;

/// <summary>
/// TextFormatter with custom image prefix for local documentation
/// </summary>
public class DocTextFormatter : TextFormatter
{
    private readonly string _imagePrefix;

    public DocTextFormatter(string imagePrefix)
    {
        _imagePrefix = imagePrefix;
    }

    protected override string GetImagePrefix()
    {
        return _imagePrefix;
    }
}

class Program
{
    static int Main(string[] args)
    {
        // Default paths (relative to current working directory)
        var rsdnPath = "markup-reference.rsdn";
        var htmlPath = "markup-reference.html";

        // Parse command line arguments
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-i" or "--input" when i + 1 < args.Length:
                    rsdnPath = args[++i];
                    break;
                case "-o" or "--output" when i + 1 < args.Length:
                    htmlPath = args[++i];
                    break;
                case "-h" or "--help":
                    PrintUsage();
                    return 0;
            }
        }

        var cssPath = "../Format/Resources/Text/Formatter.css";
        var imagePrefix = "../Format/Resources/Binary/";

        Console.WriteLine($"Reading: {rsdnPath}");
        
        if (!File.Exists(rsdnPath))
        {
            Console.WriteLine($"Error: File not found: {rsdnPath}");
            return 1;
        }

        var input = File.ReadAllText(rsdnPath);

        Console.WriteLine("Formatting...");
        var formatter = new DocTextFormatter(imagePrefix);
        var html = formatter.FormatBBCode(input);

        // Wrap in HTML with link to formatter.css
        var fullHtml = $@"<!DOCTYPE html>
<html lang=""ru"">
<head>
    <meta charset=""utf-8"">
    <title>Справочник по языку разметки RSDN Formatter</title>

    <link rel=""stylesheet"" href=""{cssPath}"">
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            max-width: 900px;
            margin: 0 auto;
            padding: 20px;
            line-height: 1.6;
            color: #333;
        }}
        h1, h2, h3, h4, h5, h6 {{
            color: #2c3e50;
            margin-top: 1.5em;
        }}
        table {{
            border-collapse: collapse;
            width: 100%;
            margin: 1em 0;
        }}
        th, td {{
            border: 1px solid #ddd;
            padding: 8px 12px;
            text-align: left;
        }}
        th {{
            background-color: #f5f5f5;
            font-weight: bold;
        }}
        tr:nth-child(even) {{
            background-color: #fafafa;
        }}
        hr {{
            border: none;
            border-top: 1px solid #ddd;
            margin: 2em 0;
        }}
    </style>
</head>
<body>
<div class=""m"">
{html}
</div>
</body>
</html>";

        Console.WriteLine($"Writing: {htmlPath}");
        File.WriteAllText(htmlPath, fullHtml);

        Console.WriteLine("Done!");
        Console.WriteLine($"Output size: {html.Length} characters");
        return 0;
    }

    static void PrintUsage()
    {
        Console.WriteLine(@"HtmlGenerator - Generate HTML from RSDN markup

Usage: HtmlGenerator [options]

Options:
  -i, --input <file>   Input RSDN file (default: markup-reference.rsdn)
  -o, --output <file>  Output HTML file (default: markup-reference.html)
  -h, --help           Show this help message

Example:
  HtmlGenerator -i doc.rsdn -o doc.html
");
    }
}