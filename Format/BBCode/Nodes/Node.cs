namespace Rsdn.Framework.Formatting.BBCode.Nodes;

/// <summary>
/// Базовый класс для узлов AST
/// </summary>
public abstract class Node
{
	/// <summary>
	/// Принять посетителя с контекстом (паттерн Visitor)
	/// </summary>
	public abstract void Accept<TContext>(INodeVisitor<TContext> visitor, TContext ctx);
}