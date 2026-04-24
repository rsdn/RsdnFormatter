#!/usr/bin/env dotnet-script
#r "C:/Work/pers/RsdnFormatter/Format/bin/Debug/netstandard2.0/Rsdn.Framework.Formatting.dll"

using Rsdn.Framework.Formatting;
using Rsdn.Framework.Formatting.BBCode;

var formatter = new TextFormatter();

var input = @"Normal text
[b]Bold text[/b]
[i]Italic text[/i]
[s]Strikeout text[/s]
[u]Underlined text[/u]

[c#]
public static text ""qqq""
[b]public static text ""qqq""[/b]
[i]public static text ""qqq""[/i]
[s]public static text ""qqq""[/s]
[u]public static text ""qqq""[/u]
[/c#]";

var output = formatter.FormatBBCode(input);
Console.WriteLine("=== OUTPUT ===");
Console.WriteLine(output);
Console.WriteLine("=== END ===");