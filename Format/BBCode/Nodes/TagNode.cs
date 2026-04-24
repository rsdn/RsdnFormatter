using System;
using System.Collections.Generic;

namespace Rsdn.Framework.Formatting.BBCode.Nodes;

/// <summary>
/// Узел парного тега: [b]...[/b], [url=...]...[/url]
/// </summary>
public class TagNode(string tagName, string? attribute = null) : Node
{
	/// <summary>
	/// Имя тега в нижнем регистре
	/// </summary>
	public string TagName { get; } = tagName.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(tagName));

	/// <summary>
	/// Значение атрибута (может быть null)
	/// </summary>
	public string? Attribute { get; } = attribute;

	/// <summary>
	/// Дочерние узлы
	/// </summary>
	public List<Node> Children { get; } = [];

	public override void Accept<TContext>(INodeVisitor<TContext> visitor, TContext ctx)
	{
		visitor.Visit(this, ctx);
	}

	public override string ToString() => 
		Attribute != null ? $"Tag: [{TagName}={Attribute}]" : $"Tag: [{TagName}]";
}