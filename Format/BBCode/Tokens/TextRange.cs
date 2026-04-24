namespace Rsdn.Framework.Formatting.BBCode.Tokens;

/// <summary>
/// Диапазон в тексте (начало и конец)
/// </summary>
public readonly struct TextRange
{
	public int Start { get; }
	public int End { get; }

	public TextRange(int start, int end)
	{
		Start = start;
		End = end;
	}

	public int Length => End - Start;

	public bool IsEmpty => Start >= End;

	public static TextRange Empty => default;

	public static implicit operator TextRange((int start, int end) tuple) => 
		new TextRange(tuple.start, tuple.end);
}