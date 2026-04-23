#!/usr/bin/env dotnet-script
// Скрипт для конвертации XML-файлов паттернов в JSON
// Запуск: dotnet script tools/convert_patterns.csx

using System;
using System.IO;
using System.Xml.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

var patternsDir = Path.Combine("Format", "CodeFormat", "Patterns");
var syntaxDir = Path.Combine("Format", "CodeFormat", "Syntax");

if (!Directory.Exists(syntaxDir))
    Directory.CreateDirectory(syntaxDir);

foreach (var xmlFile in Directory.GetFiles(patternsDir, "*.xml"))
{
    try
    {
        var doc = XDocument.Load(xmlFile);
        var root = doc.Root;
        
        if (root?.Name.LocalName != "language")
            continue;

        var syntax = new Dictionary<string, object>
        {
            ["name"] = root.Attribute("name")?.Value ?? Path.GetFileNameWithoutExtension(xmlFile),
            ["displayName"] = root.Attribute("display-name")?.Value,
            ["options"] = root.Attribute("options")?.Value
        };

        var patterns = new List<Dictionary<string, object>>();

        foreach (var pattern in root.Elements())
        {
            if (pattern.Name.LocalName != "pattern")
                continue;

            var patternDict = new Dictionary<string, object>
            {
                ["name"] = pattern.Attribute("name")?.Value ?? ""
            };

            var prefix = pattern.Attribute("prefix")?.Value;
            var postfix = pattern.Attribute("postfix")?.Value;
            var entries = pattern.Elements("entry").Select(e => e.Value).ToList();

            // Определяем тип паттерна
            if (!string.IsNullOrEmpty(prefix) || !string.IsNullOrEmpty(postfix))
            {
                // Это ключевые слова
                patternDict["type"] = "keyword";
                if (prefix != null) patternDict["prefix"] = prefix;
                if (postfix != null) patternDict["postfix"] = postfix;
                patternDict["keywords"] = entries;
            }
            else
            {
                // Это regex
                patternDict["type"] = "regex";
                patternDict["expressions"] = entries;
            }

            patterns.Add(patternDict);
        }

        syntax["patterns"] = patterns;

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(syntax, jsonOptions);
        var outputPath = Path.Combine(syntaxDir, syntax["name"] + ".json");
        File.WriteAllText(outputPath, json);

        Console.WriteLine($"Converted: {xmlFile} -> {outputPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error converting {xmlFile}: {ex.Message}");
    }
}

Console.WriteLine("Done!");