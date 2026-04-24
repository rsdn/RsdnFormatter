namespace Rsdn.Framework.Formatting.BBCode.Nodes;

/// <summary>
/// Узел строки цитирования (A> text, BB>> text, и т.д.)
/// Каждая строка с префиксом цитирования становится span с классом levelN
/// </summary>
public class QuoteLineNode : Node
{
	/// <summary>
	/// Уровень цитирования (количество '>')
	/// </summary>
	public int Level { get; }
	
	/// <summary>
	/// Префикс цитирования (например "A>" или "BB>>")
	/// </summary>
	public string Prefix { get; }
	
	/// <summary>
	/// Текст строки (без префикса)
	/// </summary>
	public string Text { get; }
	
	/// <summary>
	/// Была ли пустая строка перед этой цитатой
	/// (нужно для добавления <br /> внутри span)
	/// </summary>
	public bool HasLeadingNewline { get; set; }

	public QuoteLineNode(int level, string prefix, string text)
	{
		Level = level;
		Prefix = prefix;
		Text = text;
		HasLeadingNewline = false;
	}

	public override void Accept<TContext>(INodeVisitor<TContext> visitor, TContext ctx)
	{
		if (visitor is IQuoteLineVisitor quoteVisitor)
			quoteVisitor.VisitQuoteLine(this, ctx);
		else
			throw new System.NotSupportedException("Visitor must implement IQuoteLineVisitor");
	}
}

/// <summary>
/// Интерфейс посетителя для обработки строк цитирования
/// </summary>
public interface IQuoteLineVisitor
{
	void VisitQuoteLine(QuoteLineNode node, System.Object ctx);
}
