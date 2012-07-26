using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;

namespace Rsdn.Framework.Formatting
{
	/// <summary>
	/// Класс, для раскраски исходников
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
		/// Регулярное выражение, используемое при расераске.
		/// Получается после преобразования исходных данных.
		/// </summary>
		protected Regex ColorerRegex;

		/// <summary>
		/// Массив имен именованых групп в регулярном выражении
		/// Используется при поиске имени группы по ее номеру
		/// </summary>
		protected string[] GroupNames;

		/// <summary>
		/// Число групп в регулярном выражении
		/// </summary>
		protected int CountGroups;

		/// <summary>
		/// Создание экземпляра раскрасивальщика.
		/// </summary>
		/// <param name="xmlSource">Исходный xml-поток</param>
		public CodeFormatter(string name, Stream xmlSource) : this(name, xmlSource, RegexOptions.None) {}

		/// <summary>
		/// Создание экземпляра раскрасивальщика с дополнительными опциями для регулярного выражения.
		/// </summary>
		/// <param name="xmlSource">Исходный xml-поток</param>
		/// <param name="options">Regex опции</param>
		public CodeFormatter(string name, Stream xmlSource, RegexOptions options)
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
				if (root.Attributes["options"] != null)
					regexString.Append(root.Attributes["options"].Value);

				// Выборка шаблонов
				var syntaxis = root.SelectNodes("cc:pattern", namespaceManager);
				Debug.Assert(syntaxis != null);
				for (var i = 0; i < syntaxis.Count; i++)
				{
					if (i > 0)
						regexString.Append('|');
					regexString.AppendFormat("(?<{0}>", syntaxis[i].Attributes["name"].Value);

					var prefix = syntaxis[i].Attributes["prefix"] != null ? syntaxis[i].Attributes["prefix"].Value : null;
					var postfix = syntaxis[i].Attributes["postfix"] != null ? syntaxis[i].Attributes["postfix"].Value : null;

					// Выборка элементов шаблона
					var items = syntaxis[i].SelectNodes("cc:entry", namespaceManager);
					Debug.Assert(items != null);
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
				var numbers = ColorerRegex.GetGroupNumbers();
				var names = ColorerRegex.GetGroupNames();
				GroupNames = new string[numbers.Length];
				for (var i = 0; i < numbers.Length; i++)
					GroupNames[numbers[i]] = names[i];
			}
			catch (XmlException xmlException)
			{
				throw new FormatterException(
					string.Format("Language color pattern source xml stream is not valid:{0} - {1}", name, xmlException.Message),
					xmlException);
			}
			catch (XmlSchemaException schemaException)
			{
				throw new FormatterException(
					string.Format("Language color pattern source xml stream is not valid:{0} - {1}", name, schemaException.Message),
					schemaException);
			}
		}

		/// <summary>
		/// Преобразование текста раскрасивальщиком
		/// </summary>
		/// <param name="sourceText">Исходный текст</param>
		/// <returns>Преобразованный текст</returns>
		public string Transform(string sourceText)
		{
			return ColorerRegex.Replace(sourceText, new MatchEvaluator(ReplaceEvaluator));
		}

		/// <summary>
		/// Функция обработки найденного выражения во время трансформации
		/// </summary>
		/// <param name="match">Соответсвие</param>
		/// <returns>Обработанное соответсвие</returns>
		protected string ReplaceEvaluator(Match match)
		{
			var capturedGroup = "";
			// get captured group's name
			// 0 is all matched expression, start from 1
			for (var i = 1; i < CountGroups; i++)
				if (match.Groups[i].Success)
				{
					capturedGroup = GroupNames[i];
					break;
				}

			return string.Format("<{0}>{1}</{0}>", capturedGroup, match.Value);
		}
	}
}