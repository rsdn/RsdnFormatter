using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Rsdn.Framework.Formatting.CodeFormat;

namespace Rsdn.Framework.Formatting;

[PublicAPI]
partial class FormatterHelper
{
	private static readonly Dictionary<string, CodeLangInfo> _langInfos = new();

	private static readonly Dictionary<string, Lazy<CodeHighlighter>> _codeHighlighters =
		new(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, string?> _codeTags =
		new(StringComparer.OrdinalIgnoreCase)
		{
			{ "csharp", "CSharp" },
			{ "cs", "CSharp" },
			{ "c#", "CSharp" },

			{ "nemerle", "Nemerle" },
			{ "nitra", "Nitra" },

			{ "asm", "Assembler" },

			{ "ccode", "C" },
			{ "c", "C" },
			{ "cpp", "C" },

			{ "objc", "ObjC" },

			{ "idl", "IDL" },
			{ "midl", "IDL" },

			{ "java", "Java" },

			{ "il", "MSIL" },
			{ "msil", "MSIL" },

			{ "pascal", "Pascal" },
			{ "delphi", "Pascal" },

			{ "vb", "VisualBasic" },

			{ "sql", "SQL" },

			{ "perl", "Perl" },

			{ "php", "PHP" },

			{ "xml", "XSL" },
			{ "xsl", "XSL" },

			{ "erlang", "Erlang" },
			{ "erl", "Erlang" },

			{ "haskell", "Haskell" },
			{ "hs", "Haskell" },

			{ "lisp", "Lisp" },

			{ "ml", "Ocaml" },
			{ "ocaml", "Ocaml" },

			{ "prolog", "Prolog" },

			{ "py", "Python" },
			{ "python", "Python" },

			{ "rb", "Ruby" },
			{ "ruby", "Ruby" },

			{ "rust", "Rust" },

			{ "code", null },
			{ "pre", null }
		};

	static FormatterHelper()
	{
		// Загружаем JSON-файлы синтаксиса из встроенных ресурсов
		var asm = typeof(FormatterHelper).Assembly;
		var resourceNames = asm.GetManifestResourceNames();

		foreach (var resourceName in resourceNames)
		{
			if (!resourceName.Contains(".Syntax.") || !resourceName.EndsWith(".json"))
				continue;

			using var stream = asm.GetManifestResourceStream(resourceName);
			if (stream == null) continue;

			using var reader = new StreamReader(stream);
			var json = reader.ReadToEnd();
			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};
			options.Converters.Add(new JsonStringEnumConverter());
			var syntax = JsonSerializer.Deserialize<SyntaxDefinition>(json, options);

			if (syntax == null || string.IsNullOrEmpty(syntax.Name))
				continue;

			if (_langInfos.ContainsKey(syntax.Name))
				continue;

			_langInfos.Add(syntax.Name, new CodeLangInfo(syntax.Name, syntax.DisplayNameOrName));

			// Создаём Lazy для отложенной инициализации
			var capturedSyntax = syntax;
			_codeHighlighters.Add(
				syntax.Name,
				new Lazy<CodeHighlighter>(() => new CodeHighlighter(capturedSyntax)));
		}
	}

	/// <summary>
	/// Returns all supported language infos.
	/// </summary>
	public static IEnumerable<CodeLangInfo> GetLangInfos()
	{
		return _langInfos.Values;
	}

	/// <summary>
	/// Returns code highlighter by language name.
	/// </summary>
	public static CodeHighlighter GetCodeHighlighter(string name) =>
		name == null
			? throw new ArgumentNullException(nameof(name))
			: !_codeHighlighters.TryGetValue(name, out var highlighter)
				? throw new ArgumentException("Unsupported language")
				: highlighter.Value;

	/// <summary>
	/// Returns code highlighter by language info.
	/// </summary>
	public static CodeHighlighter GetCodeHighlighter(this CodeLangInfo info)
	{
		return info == null
			? throw new ArgumentNullException(nameof(info))
			: GetCodeHighlighter(info.Name);
	}

	/// <summary>
	/// Markup code with html tags.
	/// </summary>
	public static string MarkupCode(string langName, string source)
	{
		return
			source == null
				? throw new ArgumentNullException(nameof(source))
				: GetCodeHighlighter(langName).Highlight(source);
	}

	/// <summary>
	/// Markup code with html tags.
	/// </summary>
	public static string MarkupCode(
		this CodeLangInfo langInfo,
		string source)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		return GetCodeHighlighter(langInfo).Highlight(source);
	}

	/// <summary>
	/// Returns all known tag names.
	/// </summary>
	public static IEnumerable<string> GetCodeTagNames()
	{
		return _codeTags.Keys;
	}

	/// <summary>
	/// Returns code highlighter by tag name.
	/// </summary>
	public static CodeHighlighter? GetCodeHighlighterByTag(string tagName)
	{
		if (tagName == null)
			throw new ArgumentNullException(nameof(tagName));

		if (!_codeTags.TryGetValue(tagName, out var name) || name == null)
			return null;

		return GetCodeHighlighter(name);
	}

	/// <summary>
	/// Markup code with html tags.
	/// </summary>
	public static string MarkupCodeByTag(string tagName, string source)
	{
		var highlighter = GetCodeHighlighterByTag(tagName);
		return highlighter == null ? source : highlighter.Highlight(source);
	}

	#region Backward Compatibility

	// Для обратной совместимости со старым API
	private static readonly Dictionary<string, Lazy<CodeFormatter>> _codeFormatters =
		new(StringComparer.OrdinalIgnoreCase);

	#endregion
}