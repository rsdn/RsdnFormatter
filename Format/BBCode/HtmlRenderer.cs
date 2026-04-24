using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Rsdn.Framework.Formatting.BBCode.Nodes;

namespace Rsdn.Framework.Formatting.BBCode
{
	/// <summary>
	/// HTML рендерер для AST узлов BBCode
	/// </summary>
	public class HtmlRenderer : INodeVisitor<HtmlRenderContext>, IQuoteLineVisitor
	{
		// Маппинг языковых тегов на "code"
		private static readonly Dictionary<string, string> _languageTagAliases = new(StringComparer.OrdinalIgnoreCase)
		{
			{ "c", "code" },
			{ "cs", "code" },
			{ "csharp", "code" },
			{ "c#", "code" },
			{ "vb", "code" },
			{ "vbnet", "code" },
			{ "cpp", "code" },
			{ "c++", "code" },
			{ "java", "code" },
			{ "js", "code" },
			{ "javascript", "code" },
			{ "ts", "code" },
			{ "typescript", "code" },
			{ "python", "code" },
			{ "py", "code" },
			{ "sql", "code" },
			{ "xml", "code" },
			{ "html", "code" },
			{ "css", "code" },
			{ "php", "code" },
			{ "ruby", "code" },
			{ "go", "code" },
			{ "rust", "code" },
			{ "asm", "code" },
			{ "assembly", "code" },
			{ "ccode", "code" },
			{ "cscode", "code" },
			{ "vbcode", "code" },
			{ "pascal", "code" },
			{ "delphi", "code" },
		};

		/// <summary>
		/// Отрендерить документ в HTML
		/// </summary>
		public string Render(DocumentNode document)
		{
			var sb = new StringBuilder();
			var ctx = new HtmlRenderContext(sb);
			document.Accept(this, ctx);
			
			var result = sb.ToString();
			
			// Убираем лишний <br /> в конце
			if (result.EndsWith("<br />\n"))
				result = result.Substring(0, result.Length - 7);
			else if (result.EndsWith("<br />"))
				result = result.Substring(0, result.Length - 6);
				
			return result;
		}

		public void Visit(DocumentNode node, HtmlRenderContext ctx)
		{
			// Обрабатываем детей с учётом контекста (для цитат)
			var children = node.Children;
			for (int i = 0; i < children.Count; i++)
			{
				var child = children[i];
				
				if (child is QuoteLineNode quoteNode)
				{
					// Проверяем, что идёт после цитаты
					var nextIsQuote = (i + 1 < children.Count && children[i + 1] is QuoteLineNode);
					RenderQuoteLineWithContext(quoteNode, ctx, nextIsQuote);
				}
				else
				{
					child.Accept(this, ctx);
				}
			}
		}
		
		/// <summary>
		/// Отрендерить строку цитирования с учётом контекста
		/// </summary>
		private void RenderQuoteLineWithContext(QuoteLineNode node, HtmlRenderContext ctx, bool nextIsQuote)
		{
			var level = node.Level;
			var prefix = node.Prefix;
			var text = node.Text;
			var hasLeadingNewline = node.HasLeadingNewline;
			
			// Убираем перенос строки в конце текста
			text = text?.TrimEnd('\r', '\n') ?? "";
			
			// Если перед цитатой была пустая строка, добавляем <br /> внутри span
			if (hasLeadingNewline)
			{
				// <span class='lineQuote levelN'><br />A> text</span><br />
				ctx.Output.AppendFormat("<span class='lineQuote level{0}'><br />\n{1}{2}</span><br />\n", 
					level,
					HttpUtility.HtmlEncode(prefix),
					HttpUtility.HtmlEncode(text));
			}
			else
			{
				// <span class='lineQuote levelN'>A> text</span><br />
				ctx.Output.AppendFormat("<span class='lineQuote level{0}'>{1}{2}</span><br />\n", 
					level,
					HttpUtility.HtmlEncode(prefix),
					HttpUtility.HtmlEncode(text));
			}
		}

		// Теги, которые являются блочными (после них не нужен <br />)
		private static readonly HashSet<string> _blockTags = new(StringComparer.OrdinalIgnoreCase)
		{
			"h1", "h2", "h3", "h4", "h5", "h6",
			"blockquote", "quote", "div", "pre", "code",
			"ul", "ol", "li", "table", "tr", "td", "th",
			"p", "hr", "cut"
		};

		public void Visit(TextNode node, HtmlRenderContext ctx)
		{
			var text = node.Text;
			if (string.IsNullOrEmpty(text))
				return;
			
			// Внутри preformatted блоков не добавляем <br />
			if (ctx.InsidePreformatted)
			{
				ctx.Output.Append(HttpUtility.HtmlEncode(text));
				return;
			}
			
			// Проверяем, нужно ли пропустить первый <br />
			// (если текст начинается с \n и перед ним был блочный элемент)
			var skipFirstBr = false;
			if (text.StartsWith("\n") || text.StartsWith("\r\n"))
			{
				skipFirstBr = LastOutputWasBlockElement(ctx.Output);
			}
			
			// Разбиваем текст на строки и добавляем <br /> после каждой (кроме последней)
			var lines = text.Split('\n');
			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				// Нормализуем \r в конце строки
				if (line.EndsWith("\r"))
					line = line.Substring(0, line.Length - 1);
				
				// Пропускаем пустую строку в начале если skipFirstBr
				if (i == 0 && skipFirstBr && string.IsNullOrEmpty(line))
					continue;
				
				ctx.Output.Append(HttpUtility.HtmlEncode(line));
				
				// Добавляем <br /> после каждой строки кроме последней
				if (i < lines.Length - 1)
				{
					// Не добавляем <br /> если это первая строка и она пустая после блочного элемента
					if (i == 0 && skipFirstBr)
						continue;
					ctx.Output.Append("<br />\n");
				}
			}
		}

		/// <summary>
		/// Проверить, заканчивается ли вывод закрывающим тегом блочного элемента
		/// </summary>
		private static bool LastOutputWasBlockElement(StringBuilder output)
		{
			var text = output.ToString();
			if (string.IsNullOrEmpty(text))
				return false;
			
			// Ищем последний закрывающий тег
			var lastCloseBracket = text.LastIndexOf('>');
			if (lastCloseBracket < 0)
				return false;
			
			// Ищем начало тега
			var tagStart = text.LastIndexOf("</", lastCloseBracket);
			if (tagStart < 0)
				return false;
			
			// Извлекаем имя тега
			var tagName = text.Substring(tagStart + 2, lastCloseBracket - tagStart - 2).Trim();
			
			return _blockTags.Contains(tagName);
		}

		public void Visit(TagNode node, HtmlRenderContext ctx)
		{
			var tagName = node.TagName;

			// Проверяем алиасы языковых тегов (c#, cs, vb, etc.)
			if (_languageTagAliases.TryGetValue(tagName, out var actualTag))
			{
				// Это языковой тег - рендерим как code с языком в атрибуте
				var codeNode = new TagNode(actualTag, tagName);
				foreach (var child in node.Children)
					codeNode.Children.Add(child);
				node = codeNode;
				tagName = actualTag;
			}

			switch (tagName)
			{
				case "b": RenderBold(node, ctx); break;
				case "i": RenderItalic(node, ctx); break;
				case "u": RenderUnderline(node, ctx); break;
				case "s": RenderStrike(node, ctx); break;
				case "sub": RenderSubscript(node, ctx); break;
				case "sup": RenderSuperscript(node, ctx); break;
				case "url": RenderUrl(node, ctx); break;
				case "email": RenderEmail(node, ctx); break;
				case "img": RenderImage(node, ctx); break;
				case "quote": RenderQuote(node, ctx); break;
				case "q": RenderInlineQuote(node, ctx); break;
				case "cut": RenderCut(node, ctx); break;
				case "code": RenderCode(node, ctx); break;
				case "list": RenderList(node, ctx); break;
				case "table": RenderTable(node, ctx); break;
				case "tr": RenderTableRow(node, ctx); break;
				case "td": RenderTableCell(node, ctx); break;
				case "th": RenderTableHeader(node, ctx); break;
				case "h1":
				case "h2":
				case "h3":
				case "h4":
				case "h5":
				case "h6":
					RenderHeader(node, ctx); break;
				default:
					// Неизвестный тег - рендерим содержимое как есть
					RenderChildren(node, ctx);
					break;
			}
		}

		public void Visit(VoidNode node, HtmlRenderContext ctx)
		{
			switch (node.TagName)
			{
				case "*": RenderListItem(ctx); break;
				case "hr": RenderHorizontalRule(ctx); break;
			}
		}

		private static void RenderChildren(TagNode node, HtmlRenderContext ctx)
		{
			foreach (var child in node.Children)
			{
				child.Accept(new HtmlRenderer(), ctx);
			}
		}

		#region Tag Renderers

		private static void RenderBold(TagNode node, HtmlRenderContext ctx)
		{
			ctx.Output.Append("<b>");
			RenderChildren(node, ctx);
			ctx.Output.Append("</b>");
		}

		private static void RenderItalic(TagNode node, HtmlRenderContext ctx)
		{
			ctx.Output.Append("<i>");
			RenderChildren(node, ctx);
			ctx.Output.Append("</i>");
		}

		private static void RenderUnderline(TagNode node, HtmlRenderContext ctx)
		{
			ctx.Output.Append("<u>");
			RenderChildren(node, ctx);
			ctx.Output.Append("</u>");
		}

		private static void RenderStrike(TagNode node, HtmlRenderContext ctx)
		{
			ctx.Output.Append("<s>");
			RenderChildren(node, ctx);
			ctx.Output.Append("</s>");
		}

		private static void RenderSubscript(TagNode node, HtmlRenderContext ctx)
		{
			ctx.Output.Append("<sub>");
			RenderChildren(node, ctx);
			ctx.Output.Append("</sub>");
		}

		private static void RenderSuperscript(TagNode node, HtmlRenderContext ctx)
		{
			ctx.Output.Append("<sup>");
			RenderChildren(node, ctx);
			ctx.Output.Append("</sup>");
		}

		private static void RenderUrl(TagNode node, HtmlRenderContext ctx)
		{
			var url = node.Attribute;
			if (!string.IsNullOrEmpty(url))
			{
				// [url=...]текст[/url]
				ctx.Output.AppendFormat("<a target=\"_blank\" href=\"{0}\">", HttpUtility.HtmlAttributeEncode(url));
				RenderChildren(node, ctx);
				ctx.Output.Append("</a>");
			}
			else
			{
				// [url]ссылка[/url] - текст является ссылкой
				var text = GetTextContent(node);
				if (!string.IsNullOrEmpty(text))
				{
					ctx.Output.AppendFormat("<a target=\"_blank\" href=\"{0}\">{1}</a>", 
						HttpUtility.HtmlAttributeEncode(text),
						HttpUtility.HtmlEncode(text));
				}
			}
		}

		private static void RenderEmail(TagNode node, HtmlRenderContext ctx)
		{
			var email = node.Attribute ?? GetTextContent(node);
			if (!string.IsNullOrEmpty(email))
			{
				ctx.Output.AppendFormat("<a href=\"mailto:{0}\">", HttpUtility.HtmlAttributeEncode(email));
				RenderChildren(node, ctx);
				ctx.Output.Append("</a>");
			}
		}

		private static void RenderImage(TagNode node, HtmlRenderContext ctx)
		{
			var attr = node.Attribute;
			var content = GetTextContent(node);
			
			// Проверяем размер (small/large)
			if (string.Equals(attr, "small", StringComparison.OrdinalIgnoreCase) ||
			    string.Equals(attr, "large", StringComparison.OrdinalIgnoreCase))
			{
				// Атрибут - это размер, src берём из содержимого
				if (!string.IsNullOrEmpty(content))
				{
					ctx.Output.AppendFormat("<img border='0' class='{0}' src='{1}' />",
						attr!.ToLowerInvariant(),
						HttpUtility.HtmlAttributeEncode(content));
				}
			}
			else if (!string.IsNullOrEmpty(attr) && !string.IsNullOrEmpty(content))
			{
				// [img=sometext]url[/img] или [img sometext]url[/img]
				// Атрибут игнорируется, src берётся из содержимого
				ctx.Output.AppendFormat("<img border='0' src='{0}' />", HttpUtility.HtmlAttributeEncode(content));
			}
			else
			{
				// [img]url[/img] или [img=url][/img]
				var src = attr ?? content;
				if (!string.IsNullOrEmpty(src))
				{
					ctx.Output.AppendFormat("<img border='0' src='{0}' />", HttpUtility.HtmlAttributeEncode(src));
				}
			}
		}

		private static void RenderQuote(TagNode node, HtmlRenderContext ctx)
		{
			ctx.Output.Append("<blockquote class=\"quote\">");
			if (!string.IsNullOrEmpty(node.Attribute))
			{
				ctx.Output.AppendFormat("<div class=\"quote-author\">{0}</div>", HttpUtility.HtmlEncode(node.Attribute));
			}
			RenderChildren(node, ctx);
			ctx.Output.Append("</blockquote>");
		}

		private static void RenderInlineQuote(TagNode node, HtmlRenderContext ctx)
		{
			ctx.Output.Append("<q>");
			if (!string.IsNullOrEmpty(node.Attribute))
			{
				ctx.Output.AppendFormat("<cite>{0}</cite>", HttpUtility.HtmlEncode(node.Attribute));
			}
			RenderChildren(node, ctx);
			ctx.Output.Append("</q>");
		}

		private static void RenderCut(TagNode node, HtmlRenderContext ctx)
		{
			var title = node.Attribute ?? "Читать дальше";
			ctx.Output.AppendFormat("<div class=\"cut\"><span class=\"cut-title\">{0}</span>", HttpUtility.HtmlEncode(title));
			RenderChildren(node, ctx);
			ctx.Output.Append("</div>");
		}

		private void RenderCode(TagNode node, HtmlRenderContext ctx)
		{
			var lang = node.Attribute;
			
			ctx.Output.Append("<pre class='c'><code>");
			
			// Рендерим детей с поддержкой BBCode и подсветкой
			RenderCodeChildren(node, ctx, lang);
			
			ctx.Output.Append("</code></pre>");
		}
		
		/// <summary>
		/// Рендерить детей code-блока с поддержкой BBCode и подсветкой
		/// </summary>
		private void RenderCodeChildren(TagNode node, HtmlRenderContext ctx, string? lang)
		{
			CodeHighlighter? highlighter = null;
			if (!string.IsNullOrEmpty(lang))
			{
				highlighter = FormatterHelper.GetCodeHighlighterByTag(lang);
			}
			
			foreach (var child in node.Children)
			{
				if (child is TextNode textNode)
				{
					var text = textNode.Text;
					if (!string.IsNullOrEmpty(text))
					{
						// Заменяем табуляцию на 4 пробела
						text = text.Replace("\t", "    ");
						
						if (highlighter != null)
						{
							// Подсвечиваем текст
							var highlighted = highlighter.Highlight(text);
							highlighted = SetFont(highlighted);
							ctx.Output.Append(highlighted);
						}
						else
						{
							ctx.Output.Append(HttpUtility.HtmlEncode(text));
						}
					}
				}
				else if (child is TagNode tagChild)
				{
					// Рендерим BBCode теги внутри code (b, i, s, u)
					var tagName = tagChild.TagName.ToLowerInvariant();
					if (tagName == "b" || tagName == "i" || tagName == "s" || tagName == "u" ||
					    tagName == "sub" || tagName == "sup")
					{
						ctx.Output.AppendFormat("<{0}>", tagName);
						RenderCodeChildren(tagChild, ctx, lang);
						ctx.Output.AppendFormat("</{0}>", tagName);
					}
					else
					{
						// Другие теги - просто рендерим содержимое
						RenderCodeChildren(tagChild, ctx, lang);
					}
				}
			}
		}
		
		private static readonly System.Text.RegularExpressions.Regex _rxSetFont01 = 
			new System.Text.RegularExpressions.Regex(@"</(?<tag>kw|str|com)>(\s+)<\k<tag>>");
		
		private static readonly System.Text.RegularExpressions.Regex _rxSetFont02 =
			new System.Text.RegularExpressions.Regex(@"(?s)<(?<tag>kw|str|com)>(?<content>.*?)</\k<tag>>");

		private static string SetFont(string code)
		{
			code = _rxSetFont01.Replace(code, "$1");
			code = _rxSetFont02.Replace(code, "<span class='${tag}'>${content}</span>");
			return code;
		}

		private static void RenderList(TagNode node, HtmlRenderContext ctx)
		{
			var listType = node.Attribute;
			if (listType == "1" || listType == "a" || listType == "A" || listType == "i" || listType == "I")
			{
				ctx.Output.AppendFormat("<ol type=\"{0}\">", listType);
				RenderChildren(node, ctx);
				ctx.Output.Append("</ol>");
			}
			else
			{
				ctx.Output.Append("<ul>");
				RenderChildren(node, ctx);
				ctx.Output.Append("</ul>");
			}
		}

		private static void RenderTable(TagNode node, HtmlRenderContext ctx)
		{
			ctx.Output.Append("<table class=\"formatter\" border=\"0\" cellspacing=\"2\" cellpadding=\"5\">");
			RenderChildren(node, ctx);
			ctx.Output.Append("</table>");
		}

		private static void RenderTableRow(TagNode node, HtmlRenderContext ctx)
		{
			ctx.Output.Append("<tr class=\"formatter\">");
			RenderChildren(node, ctx);
			ctx.Output.Append("</tr>");
		}

		private static void RenderTableCell(TagNode node, HtmlRenderContext ctx)
		{
			ctx.Output.Append("<td class=\"formatter\">");
			RenderChildren(node, ctx);
			ctx.Output.Append("</td>");
		}

		private static void RenderTableHeader(TagNode node, HtmlRenderContext ctx)
		{
			ctx.Output.Append("<th class=\"formatter\">");
			RenderChildren(node, ctx);
			ctx.Output.Append("</th>");
		}

		private static void RenderListItem(HtmlRenderContext ctx)
		{
			ctx.Output.Append("<li />");
		}

		private static void RenderHorizontalRule(HtmlRenderContext ctx)
		{
			ctx.Output.Append("<hr />");
		}

		private static void RenderHeader(TagNode node, HtmlRenderContext ctx)
		{
			ctx.Output.AppendFormat("<{0} class='formatter'>", node.TagName.ToLowerInvariant());
			RenderChildren(node, ctx);
			ctx.Output.AppendFormat("</{0}>\n", node.TagName.ToLowerInvariant());
		}

		/// <summary>
		/// Отрендерить строку цитирования
		/// Формат: <span class="lineQuote levelN">A> text</span><br />
		/// </summary>
		public void VisitQuoteLine(QuoteLineNode node, object ctx)
		{
			var context = (HtmlRenderContext)ctx;
			var level = node.Level;
			var prefix = node.Prefix;
			var text = node.Text;
			
			// Убираем перенос строки в конце текста
			text = text?.TrimEnd('\r', '\n') ?? "";
			
			// <span class='lineQuote levelN'>A> text</span><br />
			// prefix уже содержит '>' (например "A>" или "BB>>")
			// text уже содержит ведущий пробел (после A>)
			context.Output.AppendFormat("<span class='lineQuote level{0}'>{1}{2}</span><br />\n", 
				level,
				HttpUtility.HtmlEncode(prefix),
				HttpUtility.HtmlEncode(text));
		}

		#endregion

		private static string? GetTextContent(TagNode node)
		{
			if (node.Children.Count == 1 && node.Children[0] is TextNode textNode)
			{
				return textNode.Text;
			}

			var sb = new StringBuilder();
			foreach (var child in node.Children)
			{
				if (child is TextNode tn)
					sb.Append(tn.Text);
			}
			return sb.Length > 0 ? sb.ToString() : null;
		}
	}
}