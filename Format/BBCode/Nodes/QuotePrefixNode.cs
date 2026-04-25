namespace Rsdn.Framework.Formatting.BBCode.Nodes;

/// <summary>
/// Узел строки цитирования (A> text, BB>> text, и т.д.)
/// Каждая строка с префиксом цитирования становится span с классом levelN
/// </summary>
public class QuoteLineNode(int level, string prefix, string text) : Node
{
	/// <summary>
	/// Уровень цитирования (количество '>')
	/// </summary>
	public int Level { get; } = level;

	/// <summary>
	/// Префикс цитирования (например "A>" или "BB>>")
	/// </summary>
	public string Prefix { get; } = prefix;

	/// <summary>
	/// Текст строки (без префикса)
	/// </summary>
	public string Text { get; set; } = text;

	/// <summary>
	/// Была ли пустая строка перед этой цитатой
	/// (нужно для добавления <br /> внутри span)
	/// </summary>
	public bool HasLeadingNewline { get; set; }

	public override void Accept<TContext>(INodeVisitor<TContext> visitor, TContext ctx)
	{
			visitor.Visit(this, ctx);
	}

	public override void Accept(INodeVisitor visitor)
	{
		visitor.Visit(this);
	}
}