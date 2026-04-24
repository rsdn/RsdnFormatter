using System.Collections.Generic;
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
	public List<HtmlDifference> Differences { get; set; } = new();

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
			
		foreach (var diff in Differences)
		{
			sb.AppendLine(diff.ToString());
			sb.AppendLine();
		}

		sb.AppendLine(new string('=', 60));
		sb.AppendLine("Полный ожидаемый HTML:");
		sb.AppendLine(ExpectedHtml);
		sb.AppendLine();
		sb.AppendLine("Полный фактический HTML:");
		sb.AppendLine(ActualHtml);

		return sb.ToString();
	}
}