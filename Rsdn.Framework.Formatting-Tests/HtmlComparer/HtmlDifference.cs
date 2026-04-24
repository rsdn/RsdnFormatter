using System.Text;

namespace Rsdn.Framework.Formatting.Tests.HtmlComparer;

/// <summary>
/// Описание различия в HTML
/// </summary>
public class HtmlDifference(
	HtmlDifferenceKind kind,
	string path,
	string message,
	string? expectedContext = null,
	string? actualContext = null)
{
	public HtmlDifferenceKind Kind { get; } = kind;
	public string Path { get; } = path;
	public string Message { get; } = message;
	public string? ExpectedContext { get; } = expectedContext;
	public string? ActualContext { get; } = actualContext;

	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.Append($"{Path}: {Message}");
			
		if (ExpectedContext != null || ActualContext != null)
		{
			sb.AppendLine();
			sb.AppendLine("  ---");
			if (ExpectedContext != null)
				sb.AppendLine($"  Ожидается:\n  {ExpectedContext.Replace("\n", "\n  ")}");
			if (ActualContext != null)
				sb.AppendLine($"  Фактически:\n  {ActualContext.Replace("\n", "\n  ")}");
			sb.Append("  ---");
		}

		return sb.ToString();
	}
}