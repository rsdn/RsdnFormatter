using Rsdn.Framework.Formatting.BBCode.Nodes;

namespace Rsdn.Framework.Formatting.BBCode
{
	/// <summary>
	/// Базовый визитор для обхода AST дерева BBCode.
	/// Предоставляет виртуальные методы для каждого типа узлов.
	/// </summary>
	public class NodeVisitor : INodeVisitor
	{
		/// <summary>
		/// Обойти документ
		/// </summary>
		public virtual void Visit(DocumentNode node)
		{
			foreach (var child in node.Children)
			{
				child.Accept(this);
			}
		}

		/// <summary>
		/// Обойти текстовый узел
		/// </summary>
		public virtual void Visit(TextNode node)
		{
			// По умолчанию ничего не делает
		}

		/// <summary>
		/// Обойти тег
		/// </summary>
		public virtual void Visit(TagNode node)
		{
			foreach (var child in node.Children)
			{
				child.Accept(this);
			}
		}

		/// <summary>
		/// Обойти void-тег
		/// </summary>
		public virtual void Visit(VoidNode node)
		{
			// По умолчанию ничего не делает
		}

		/// <summary>
		/// Обойти строку цитирования
		/// </summary>
		public virtual void Visit(QuoteLineNode node)
		{
			// По умолчанию ничего не делает
		}
	}
}