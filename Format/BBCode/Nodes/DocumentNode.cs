using System.Collections.Generic;

namespace Rsdn.Framework.Formatting.BBCode.Nodes;

/// <summary>
/// Корневой узел документа
/// </summary>
public class DocumentNode : Node
{
	/// <summary>
	/// Дочерние узлы
	/// </summary>
	public List<Node> Children { get; } = [];

	public override void Accept<TContext>(INodeVisitor<TContext> visitor, TContext ctx)
	{
		visitor.Visit(this, ctx);
	}

	public override void Accept(INodeVisitor visitor)
	{
		visitor.Visit(this);
	}

	public override string ToString() => "Document";
}