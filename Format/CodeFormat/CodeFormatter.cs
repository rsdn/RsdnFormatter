using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using CodeJam;

namespace Rsdn.Framework.Formatting.CodeFormat;

/// <summary>
/// Класс, для раскраски исходников.
/// Загружает правила раскраски из xml-файла
/// </summary>
public class CodeFormatter
{
#pragma warning disable 618
	/// <summary>
	/// Source XML validating schemas (XSD)
	/// </summary>
	private static readonly XmlSchemaCollection _xmlSchemas;
#pragma warning restore 618

	static CodeFormatter()
	{
		//Load the schema collection.
		var resource =
			typeof (CodeFormatter)
				.Assembly
				.GetManifestResourceStream("Rsdn.Framework.Formatting.CodeFormat.Patterns.PatternSchema.xsd");
		Debug.Assert(resource != null);
#pragma warning disable 618
		_xmlSchemas =
			new XmlSchemaCollection
			{
				XmlSchema.Read(resource, null)
			};
#pragma warning restore 618
	}

	/// <summary>
	/// Регулярное выражение, используемое при раскраске.
	/// Получается после преобразования исходных данных.
	/// </summary>
	protected readonly Regex ColorerRegex;

	/// <summary>
	/// Число групп в регулярном выражении
	/// </summary>
	protected int CountGroups;

	/// <summary>
	/// Создание экземпляра раскрасивальщика с дополнительными опциями для регулярного выражения.
	/// </summary>
	/// <param name="name">Имя схемы</param>
	/// <param name="xmlSource">Исходный xml-поток</param>
	/// <param name="options">Regex опции</param>
	public CodeFormatter(string name, Stream xmlSource, RegexOptions options = RegexOptions.None)
	{
		try
		{
			var regexString = new StringBuilder();

#pragma warning disable 618
			var validatingReader =
				new XmlValidatingReader(new XmlTextReader(xmlSource))
				{
					ValidationType = ValidationType.Schema
				};
#pragma warning restore 618

			validatingReader.Schemas.Add(_xmlSchemas);

			var doc = new XmlDocument();
			doc.Load(validatingReader);

			var namespaceManager = new XmlNamespaceManager(doc.NameTable);
			namespaceManager.AddNamespace("cc", "http://rsdn.ru/coloring");

			// Поиск коневого элемента
			var root = doc.SelectSingleNode("cc:language", namespaceManager);

			// Установка regex опций, если есть
			Code.NotNull(root);
			if (root.Attributes?["options"] != null)
				regexString.Append(root.Attributes["options"].Value);

			// Выборка шаблонов
			var syntax = root.SelectNodes("cc:pattern", namespaceManager);
			Debug.Assert(syntax != null);
			Code.NotNull(syntax);
			for (var i = 0; i < syntax.Count; i++)
			{
				if (i > 0)
					regexString.Append('|');
				Code.NotNull(syntax[i]?.Attributes);
				var attrs = syntax[i].Attributes;
				Code.NotNull(attrs);
				regexString.AppendFormat("(?<{0}>", attrs["name"].Value);

				var prefix = attrs["prefix"]?.Value;
				var postfix = attrs["postfix"]?.Value;

				// Выборка элементов шаблона
				var items = syntax[i].SelectNodes("cc:entry", namespaceManager);
				Code.NotNull(items);
				for (var j = 0; j < items.Count; j++)
				{
					if (j > 0)
						regexString.Append('|');
					regexString.Append(prefix).Append(items[j].InnerText).Append(postfix);
				}

				regexString.Append(')');
			}

			// Создание регулярного выражения
			ColorerRegex = new Regex(regexString.ToString(), options);
			// Чтение параметров регулярного выражения
			CountGroups = ColorerRegex.GetGroupNumbers().Length;
		}
		catch (XmlException xmlException)
		{
			throw new FormatterException(
				$"Language color pattern source xml stream is not valid:{name} - {xmlException.Message}",
				xmlException);
		}
		catch (XmlSchemaException schemaException)
		{
			throw new FormatterException(
				$"Language color pattern source xml stream is not valid:{name} - {schemaException.Message}",
				schemaException);
		}
	}
}