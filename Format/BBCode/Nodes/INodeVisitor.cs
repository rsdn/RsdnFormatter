namespace Rsdn.Framework.Formatting.BBCode.Nodes;

/// <summary>
/// Интерфейс посетителя для рендеринга узлов с контекстом
/// </summary>
/// <typeparam name="TContext">Тип контекста рендеринга</typeparam>
public interface INodeVisitor<in TContext>
{
	void Visit(TextNode node, TContext ctx);
	void Visit(TagNode node, TContext ctx);
	void Visit(VoidNode node, TContext ctx);
	void Visit(DocumentNode node, TContext ctx);
	void Visit(QuoteLineNode node, TContext ctx);
}

/// <summary>
/// Интерфейс посетителя для обхода узлов без контекста
/// </summary>
public interface INodeVisitor
{
	void Visit(TextNode node);
	void Visit(TagNode node);
	void Visit(VoidNode node);
	void Visit(DocumentNode node);
	void Visit(QuoteLineNode node);
}