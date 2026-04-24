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
}