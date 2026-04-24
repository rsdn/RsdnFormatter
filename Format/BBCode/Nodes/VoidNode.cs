using System;

namespace Rsdn.Framework.Formatting.BBCode.Nodes;

/// <summary>
/// Узел пустого тега (void): [*], [hr]
/// </summary>
public class VoidNode(string tagName) : Node
{
	/// <summary>
	/// Имя тега в нижнем регистре
	/// </summary>
	public string TagName { get; } = tagName.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(tagName));

	public override void Accept<TContext>(INodeVisitor<TContext> visitor, TContext ctx)
	{
		visitor.Visit(this, ctx);
	}

	public override string ToString() => $"Void: [{TagName}]";
}