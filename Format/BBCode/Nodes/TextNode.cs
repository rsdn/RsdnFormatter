using System;

namespace Rsdn.Framework.Formatting.BBCode.Nodes;

/// <summary>
/// Текстовый узел
/// </summary>
public class TextNode(string text) : Node
{
	/// <summary>
	/// Текстовое содержимое
	/// </summary>
	public string Text { get; set; } = text ?? throw new ArgumentNullException(nameof(text));

	public override void Accept<TContext>(INodeVisitor<TContext> visitor, TContext ctx)
	{
		visitor.Visit(this, ctx);
	}

	public override void Accept(INodeVisitor visitor)
	{
		visitor.Visit(this);
	}

	public override string ToString() => $"Text: \"{Text}\"";
}