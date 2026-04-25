using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rsdn.Framework.Formatting.Tests.HtmlComparer;

/// <summary>
/// Результат сравнения HTML
/// </summary>
public class HtmlCompareResult
{
	/// <summary>
	/// Равны ли документы
	/// </summary>
	public bool AreEqual { get; set; }

	/// <summary>
	/// Список различий
	/// </summary>
	public List<HtmlDifference> Differences { get; set; } = [];

	/// <summary>
	/// Ожидаемый HTML
	/// </summary>
	public string ExpectedHtml { get; set; } = "";

	/// <summary>
	/// Фактический HTML
	/// </summary>
	public string ActualHtml { get; set; } = "";

	public override string ToString()
	{
		if (AreEqual)
			return "HTML documents are equal";

		var sb = new StringBuilder();
		sb.AppendLine("HTML documents differ:");
		sb.AppendLine(new string('=', 60));

		var diffLines = GetDifferingLinesWithContext();
		sb.Append(diffLines);

		return sb.ToString();
	}

	/// <summary>
	/// Получить различающиеся строки с контекстом (строка до, строка с различием, строка после)
	/// </summary>
	private string GetDifferingLinesWithContext()
	{
		var expectedLines = ExpectedHtml.Split(["\r\n", "\n"], StringSplitOptions.None);
		var actualLines = ActualHtml.Split(["\r\n", "\n"], StringSplitOptions.None);

		var diffLineIndices = new HashSet<int>();
		var sb = new StringBuilder();

		// Находим строки с различиями
		var maxLines = Math.Max(expectedLines.Length, actualLines.Length);
		for (int i = 0; i < maxLines; i++)
		{
			var expectedLine = i < expectedLines.Length ? expectedLines[i] : "";
			var actualLine = i < actualLines.Length ? actualLines[i] : "";

			if (expectedLine == actualLine)
				continue;
			// Добавляем саму строку и контекст (строку до и после)
			if (i > 0) diffLineIndices.Add(i - 1);
			diffLineIndices.Add(i);
			if (i < maxLines - 1) diffLineIndices.Add(i + 1);
		}

		// Сортируем индексы и группируем подряд идущие строки
		var sortedIndices = diffLineIndices.OrderBy(i => i).ToList();
		
		if (sortedIndices.Count == 0)
			return "Нет построчных различий (возможно различия в структуре HTML)";

		int? lastPrintedIndex = null;
		var inGroup = false;

		foreach (var idx in sortedIndices)
		{
			// Если разрыв между строками больше 1, добавляем разделитель
			if (lastPrintedIndex.HasValue && idx > lastPrintedIndex.Value + 1)
			{
				sb.AppendLine("...");
				inGroup = false;
			}

			var expectedLine = idx < expectedLines.Length ? expectedLines[idx] : "";
			var actualLine = idx < actualLines.Length ? actualLines[idx] : "";

			if (expectedLine == actualLine)
			{
				// Строка контекста (без различий)
				sb.AppendLine($"  {idx + 1,4}: {expectedLine}");
			}
			else
			{
				// Строка с различием
				sb.AppendLine($"- {idx + 1,4}: {expectedLine}");
				sb.AppendLine($"+ {idx + 1,4}: {actualLine}");
			}

			lastPrintedIndex = idx;
		}

		return sb.ToString();
	}
}