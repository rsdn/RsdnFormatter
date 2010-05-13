using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Rsdn.Framework.Formatting.JetBrains.Annotations;

namespace Rsdn.Framework.Formatting
{
	partial class FormatterHelper
	{
		private static readonly Dictionary<string, CodeLangInfo> _langInfos =
			new Dictionary<string, CodeLangInfo>();
		private static readonly Dictionary<string, CodeFormatter> _codeFormatters =
			new Dictionary<string, CodeFormatter>(StringComparer.OrdinalIgnoreCase);

		private static readonly Dictionary<string, string> _codeTags =
			new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
				{
					{"csharp", "CSharp"},
					{"cs", "CSharp"},
					{"c#", "CSharp"},

					{"Nemerle", "Nemerle"},

					{"asm", "Assembler"},

					{"ccode", "C"},
					{"c", "C"},
					{"cpp", "C"},

					{"idl", "IDL"},
					{"midl", "IDL"},

					{"java", "Java"},

					{"il", "MSIL"},
					{"msil", "MSIL"},

					{"pascal", "Pascal"},
					{"delphi", "Pascal"},

					{"vb", "VisualBasic"},

					{"sql", "SQL"},

					{"perl", "Perl"},

					{"php", "PHP"},

					{"xml", "XSL"},
					{"xsl", "XML"},

					{"erlang", "Erlang"},
					{"erl", "Erlang"},

					{"haskell", "Haskell"},
					{"hs", "Haskell"},

					{"lisp", "Lisp"},

					{"ml", "Ocaml"},
					{"ocaml", "Ocaml"},

					{"prolog", "Prolog"},

					{"py", "Python"},
					{"python", "Python"},

					{"rb", "Ruby"},
					{"ruby", "Ruby"},

					{"code", null},
					{"pre", null},
				};

		static FormatterHelper()
		{
			var asm = typeof (FormatterHelper).Assembly;
			var resources =
				asm
					.GetManifestResourceNames()
					.Where(
						name =>
							name.StartsWith("Rsdn.Framework.Formatting.CodeFormat.Patterns")
							&& name.EndsWith(".xml"))
					.Select(x => asm.GetManifestResourceStream(x));
			foreach (var stream in resources)
			{
				var xDoc = XDocument.Load(new StreamReader(stream));
				var info = RetrieveLangInfo(xDoc.Root);
				if (_langInfos.ContainsKey(info.Name))
					throw new ApplicationException("Duplicate language definition");
				_langInfos.Add(info.Name, info);

				stream.Seek(0, SeekOrigin.Begin);
				_codeFormatters.Add(info.Name, new CodeFormatter(stream));
			}
		}

		private static CodeLangInfo RetrieveLangInfo(XElement element)
		{
			if (element.Name.LocalName != "language")
				throw new ApplicationException("invalid format");
			var name = element.Attribute(XName.Get("name"));
			if (name == null)
				throw new ApplicationException("name not specified");
			var displayName = element.Attribute(XName.Get("display-name"));
			return
				new CodeLangInfo(
					name.Value,
					displayName != null ? displayName.Value : name.Value);
		}

		/// <summary>
		/// Returns all supported language infos.
		/// </summary>
		[NotNull]
		public static IEnumerable<CodeLangInfo> GetLangInfos()
		{
			return _langInfos.Values;
		}

		/// <summary>
		/// Returns code formatter by language name.
		/// </summary>
		public static CodeFormatter GetCodeFormatter([NotNull] string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			CodeFormatter cf;
			if (!_codeFormatters.TryGetValue(name, out cf))
				throw new ArgumentException("Unsupported language");
			return cf;
		}

		/// <summary>
		/// Returns code formatter by language info.
		/// </summary>
		[NotNull]
		public static CodeFormatter GetCodeFormatter([NotNull] this CodeLangInfo info)
		{
			if (info == null) throw new ArgumentNullException("info");
			return GetCodeFormatter(info.Name);
		}

		/// <summary>
		/// Markup code with xml.
		/// </summary>
		[NotNull]
		public static string MarkupCode([NotNull] string langName, [NotNull] string source)
		{
			if (source == null) throw new ArgumentNullException("source");
			return GetCodeFormatter(langName).Transform(source);
		}

		/// <summary>
		/// Markup code with xml.
		/// </summary>
		[NotNull]
		public static string MarkupCode(
			[NotNull] this CodeLangInfo langInfo,
			[NotNull] string source)
		{
			if (source == null) throw new ArgumentNullException("source");
			return GetCodeFormatter(langInfo).Transform(source);
		}

		/// <summary>
		/// Returns all known tag names.
		/// </summary>
		public static IEnumerable<string> GetCodeTagNames()
		{
			return _codeTags.Keys;
		}

		/// <summary>
		/// Returns code formatter by tag name.
		/// </summary>
		[CanBeNull]
		public static CodeFormatter GetCodeFormatterByTag([NotNull] string tagName)
		{
			if (tagName == null) throw new ArgumentNullException("tagName");
			string name;
			if (!_codeTags.TryGetValue(tagName, out name) || name == null)
				return null;
			return GetCodeFormatter(name);
		}

		/// <summary>
		/// Markup code with xml.
		/// </summary>
		[NotNull]
		public static string MarkupCodeByTag([NotNull] string tagName, [NotNull] string source)
		{
			var cf = GetCodeFormatterByTag(tagName);
			return cf == null ? source : cf.Transform(source);
		}
	}
}