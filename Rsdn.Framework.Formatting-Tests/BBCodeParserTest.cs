using System;
using NUnit.Framework;
using Rsdn.Framework.Formatting.BBCode;
using Rsdn.Framework.Formatting.BBCode.Nodes;
using Rsdn.Framework.Formatting.BBCode.Tokens;

namespace Rsdn.Framework.Formatting.Tests
{
    [TestFixture]
    public class BBCodeParserTest
    {
        #region Tokenizer Tests

        [Test]
        public void Tokenizer_SimpleText_ReturnsTextToken()
        {
            var tokenizer = new Tokenizer("Hello World".AsSpan());
            var token = tokenizer.ReadNext();

            Assert.AreEqual(TokenType.Text, token.Type);
            Assert.AreEqual("Hello World", token.GetTextString("Hello World".AsSpan()));
        }

        [Test]
        public void Tokenizer_OpenTag_ReturnsOpenTagToken()
        {
            var text = "[b]";
            var tokenizer = new Tokenizer(text.AsSpan());
            var token = tokenizer.ReadNext();

            Assert.AreEqual(TokenType.OpenTag, token.Type);
            Assert.AreEqual("b", token.GetTagNameLower(text.AsSpan()));
        }

        [Test]
        public void Tokenizer_CloseTag_ReturnsCloseTagToken()
        {
            var text = "[/b]";
            var tokenizer = new Tokenizer(text.AsSpan());
            var token = tokenizer.ReadNext();

            Assert.AreEqual(TokenType.CloseTag, token.Type);
            Assert.AreEqual("b", token.GetTagNameLower(text.AsSpan()));
        }

        [Test]
        public void Tokenizer_TagWithAttribute_ReturnsOpenTagWithAttribute()
        {
            var text = "[url=https://example.com]";
            var tokenizer = new Tokenizer(text.AsSpan());
            var token = tokenizer.ReadNext();

            Assert.AreEqual(TokenType.OpenTag, token.Type);
            Assert.AreEqual("url", token.GetTagNameLower(text.AsSpan()));
            Assert.AreEqual("https://example.com", token.GetAttributeString(text.AsSpan()));
        }

        [Test]
        public void Tokenizer_VoidTag_ReturnsVoidTagToken()
        {
            var text = "[*]";
            var tokenizer = new Tokenizer(text.AsSpan());
            var token = tokenizer.ReadNext();

            Assert.AreEqual(TokenType.VoidTag, token.Type);
            Assert.AreEqual("*", token.GetTagNameLower(text.AsSpan()));
        }

        [Test]
        public void Tokenizer_HrTag_ReturnsVoidTagToken()
        {
            var text = "[hr]";
            var tokenizer = new Tokenizer(text.AsSpan());
            var token = tokenizer.ReadNext();

            Assert.AreEqual(TokenType.VoidTag, token.Type);
            Assert.AreEqual("hr", token.GetTagNameLower(text.AsSpan()));
        }

        [Test]
        public void Tokenizer_MultipleTokens_ReturnsAllTokens()
        {
            var text = "[b]Hello[/b]";
            var tokenizer = new Tokenizer(text.AsSpan());
            var tokens = tokenizer.ReadAll();

            Assert.AreEqual(4, tokens.Count); // [b], Hello, [/b], EOF
            Assert.AreEqual(TokenType.OpenTag, tokens[0].Type);
            Assert.AreEqual(TokenType.Text, tokens[1].Type);
            Assert.AreEqual(TokenType.CloseTag, tokens[2].Type);
            Assert.AreEqual(TokenType.EndOfText, tokens[3].Type);
        }

        [Test]
        public void Tokenizer_AttributeWithQuotes_HandlesQuotes()
        {
            var text = "[url=\"https://example.com\"]";
            var tokenizer = new Tokenizer(text.AsSpan());
            var token = tokenizer.ReadNext();

            Assert.AreEqual(TokenType.OpenTag, token.Type);
            Assert.AreEqual("https://example.com", token.GetAttributeString(text.AsSpan()));
        }

        #endregion

        #region Parser Tests

        [Test]
        public void Parser_SimpleText_ReturnsDocumentWithTextNode()
        {
            var parser = new Parser("Hello World");
            var doc = parser.Parse();

            Assert.AreEqual(1, doc.Children.Count);
            Assert.IsInstanceOf<TextNode>(doc.Children[0]);
            Assert.AreEqual("Hello World", ((TextNode)doc.Children[0]).Text);
        }

        [Test]
        public void Parser_BoldTag_ReturnsTagNode()
        {
            var parser = new Parser("[b]Bold text[/b]");
            var doc = parser.Parse();

            Assert.AreEqual(1, doc.Children.Count);
            var tagNode = doc.Children[0] as TagNode;
            Assert.IsNotNull(tagNode);
            Assert.AreEqual("b", tagNode!.TagName);
            Assert.AreEqual(1, tagNode.Children.Count);
            Assert.IsInstanceOf<TextNode>(tagNode.Children[0]);
        }

        [Test]
        public void Parser_NestedTags_ReturnsNestedStructure()
        {
            var parser = new Parser("[b][i]text[/i][/b]");
            var doc = parser.Parse();

            Assert.AreEqual(1, doc.Children.Count);
            var boldNode = doc.Children[0] as TagNode;
            Assert.IsNotNull(boldNode);
            Assert.AreEqual("b", boldNode!.TagName);
            
            var italicNode = boldNode.Children[0] as TagNode;
            Assert.IsNotNull(italicNode);
            Assert.AreEqual("i", italicNode!.TagName);
        }

        [Test]
        public void Parser_TagWithAttribute_ReturnsTagWithAttribute()
        {
            var parser = new Parser("[url=https://example.com]Link[/url]");
            var doc = parser.Parse();

            Assert.AreEqual(1, doc.Children.Count);
            var tagNode = doc.Children[0] as TagNode;
            Assert.IsNotNull(tagNode);
            Assert.AreEqual("url", tagNode!.TagName);
            Assert.AreEqual("https://example.com", tagNode.Attribute);
        }

        [Test]
        public void Parser_CodeTag_ParsesBBCodeContent()
        {
            var parser = new Parser("[code][b]bold text[/b][/code]");
            var doc = parser.Parse();

            Assert.AreEqual(1, doc.Children.Count);
            var codeNode = doc.Children[0] as TagNode;
            Assert.IsNotNull(codeNode);
            Assert.AreEqual("code", codeNode!.TagName);
            
            // Внутри code должен быть распарсенный тег [b], не просто текст
            Assert.AreEqual(1, codeNode.Children.Count);
            var boldNode = codeNode.Children[0] as TagNode;
            Assert.IsNotNull(boldNode);
            Assert.AreEqual("b", boldNode!.TagName);
            
            // Внутри [b] должен быть текст
            Assert.AreEqual(1, boldNode.Children.Count);
            var textNode = boldNode.Children[0] as TextNode;
            Assert.IsNotNull(textNode);
            Assert.AreEqual("bold text", textNode!.Text);
        }

        [Test]
        public void Parser_VoidTag_ReturnsVoidNode()
        {
            var parser = new Parser("[*]Item");
            var doc = parser.Parse();

            Assert.AreEqual(2, doc.Children.Count);
            Assert.IsInstanceOf<VoidNode>(doc.Children[0]);
            Assert.AreEqual("*", ((VoidNode)doc.Children[0]).TagName);
        }

        [Test]
        public void Parser_UnmatchedCloseTag_IgnoresTag()
        {
            var parser = new Parser("text[/b]");
            var doc = parser.Parse();

            // Несогласованный закрывающий тег игнорируется
            Assert.AreEqual(1, doc.Children.Count);
            Assert.IsInstanceOf<TextNode>(doc.Children[0]);
        }

        #endregion

        #region HtmlRenderer Tests

        [Test]
        public void Renderer_TextNode_EscapesHtml()
        {
            var doc = new DocumentNode();
            doc.Children.Add(new TextNode("<b>bold</b>"));

            var renderer = new HtmlRenderer();
            var html = renderer.Render(doc);

            // HtmlEncode экранирует < и >
            var expected = "&lt;b&gt;bold&lt;/b&gt;";
            Assert.AreEqual(expected, html);
        }

        [Test]
        public void Renderer_BoldTag_RendersBoldHtml()
        {
            var doc = new DocumentNode();
            var boldNode = new TagNode("b");
            boldNode.Children.Add(new TextNode("Bold"));
            doc.Children.Add(boldNode);

            var renderer = new HtmlRenderer();
            var html = renderer.Render(doc);

            Assert.AreEqual("<b>Bold</b>", html);
        }

        [Test]
        public void Renderer_ItalicTag_RendersItalicHtml()
        {
            var doc = new DocumentNode();
            var italicNode = new TagNode("i");
            italicNode.Children.Add(new TextNode("Italic"));
            doc.Children.Add(italicNode);

            var renderer = new HtmlRenderer();
            var html = renderer.Render(doc);

            Assert.AreEqual("<i>Italic</i>", html);
        }

        [Test]
        public void Renderer_UrlTagWithAttribute_RendersLink()
        {
            var doc = new DocumentNode();
            var urlNode = new TagNode("url", "https://example.com");
            urlNode.Children.Add(new TextNode("Example"));
            doc.Children.Add(urlNode);

            var renderer = new HtmlRenderer();
            var html = renderer.Render(doc);

            Assert.AreEqual("<a target=\"_blank\" href=\"https://example.com\">Example</a>", html);
        }

        [Test]
        public void Renderer_UrlTagWithoutAttribute_RendersLinkFromText()
        {
            var doc = new DocumentNode();
            var urlNode = new TagNode("url");
            urlNode.Children.Add(new TextNode("https://example.com"));
            doc.Children.Add(urlNode);

            var renderer = new HtmlRenderer();
            var html = renderer.Render(doc);

            Assert.AreEqual("<a target=\"_blank\" href=\"https://example.com\">https://example.com</a>", html);
        }

        [Test]
        public void Renderer_QuoteTag_RendersBlockquote()
        {
            var doc = new DocumentNode();
            var quoteNode = new TagNode("quote", "Author");
            quoteNode.Children.Add(new TextNode("Quote text"));
            doc.Children.Add(quoteNode);

            var renderer = new HtmlRenderer();
            var html = renderer.Render(doc);

            Assert.AreEqual("<blockquote class=\"quote\"><div class=\"quote-author\">Author</div>Quote text</blockquote>", html);
        }

        [Test]
        public void Renderer_ListTag_RendersUnorderedList()
        {
            var doc = new DocumentNode();
            var listNode = new TagNode("list");
            listNode.Children.Add(new VoidNode("*"));
            listNode.Children.Add(new TextNode("Item 1"));
            doc.Children.Add(listNode);

            var renderer = new HtmlRenderer();
            var html = renderer.Render(doc);

            Assert.AreEqual("<ul><li />Item 1</ul>", html);
        }

        [Test]
        public void Renderer_OrderedList_RendersOrderedList()
        {
            var doc = new DocumentNode();
            var listNode = new TagNode("list", "1");
            listNode.Children.Add(new VoidNode("*"));
            listNode.Children.Add(new TextNode("Item 1"));
            doc.Children.Add(listNode);

            var renderer = new HtmlRenderer();
            var html = renderer.Render(doc);

            Assert.AreEqual("<ol type=\"1\"><li />Item 1</ol>", html);
        }

        [Test]
        public void Renderer_CodeTag_RendersPreTag()
        {
            var doc = new DocumentNode();
            var codeNode = new TagNode("code", "csharp");
            codeNode.Children.Add(new TextNode("var x = 1;"));
            doc.Children.Add(codeNode);

            var renderer = new HtmlRenderer();
            var html = renderer.Render(doc);

            // Код должен быть подсвечен
            Assert.IsTrue(html.StartsWith("<pre class='c'><code>"));
            Assert.IsTrue(html.Contains("<span class='kw'>var</span>"));
            Assert.IsTrue(html.EndsWith("</code></pre>"));
        }

        [Test]
        public void Renderer_HrTag_RendersHrTag()
        {
            var doc = new DocumentNode();
            doc.Children.Add(new VoidNode("hr"));

            var renderer = new HtmlRenderer();
            var html = renderer.Render(doc);

            Assert.AreEqual("<hr />", html);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Integration_FullWorkflow_ParsesAndRenders()
        {
            var bbcode = "[b]Bold [i]and italic[/i][/b]";
            
            var parser = new Parser(bbcode);
            var doc = parser.Parse();

            var renderer = new HtmlRenderer();
            var html = renderer.Render(doc);

            Assert.AreEqual("<b>Bold <i>and italic</i></b>", html);
        }

        [Test]
        public void Integration_ComplexDocument_ParsesAndRenders()
        {
            var bbcode = "[b]Title[/b]\n[list][*]Item 1[*]Item 2[/list]";
            
            var parser = new Parser(bbcode);
            var doc = parser.Parse();

            var renderer = new HtmlRenderer();
            var html = renderer.Render(doc);

            Assert.IsTrue(html.Contains("<b>Title</b>"));
            Assert.IsTrue(html.Contains("<ul>"));
            Assert.IsTrue(html.Contains("<li />"));
        }

        #endregion

        #region TextFormatter Integration Tests

        [Test]
        public void TextFormatter_FormatBBCode_SimpleText_ReturnsHtml()
        {
            var formatter = new TextFormatter();
            var html = formatter.FormatBBCode("Hello World");

            Assert.AreEqual("Hello World", html);
        }

        [Test]
        public void TextFormatter_FormatBBCode_BoldTag_ReturnsBoldHtml()
        {
            var formatter = new TextFormatter();
            var html = formatter.FormatBBCode("[b]Bold text[/b]");

            Assert.AreEqual("<b>Bold text</b>", html);
        }

        [Test]
        public void TextFormatter_FormatBBCode_NestedTags_ReturnsNestedHtml()
        {
            var formatter = new TextFormatter();
            var html = formatter.FormatBBCode("[b][i]Bold and italic[/i][/b]");

            Assert.AreEqual("<b><i>Bold and italic</i></b>", html);
        }

        [Test]
        public void TextFormatter_FormatBBCode_UrlTag_ReturnsLink()
        {
            var formatter = new TextFormatter();
            var html = formatter.FormatBBCode("[url=https://example.com]Example[/url]");

            Assert.AreEqual("<a target=\"_blank\" href=\"https://example.com\">Example</a>", html);
        }

        [Test]
        public void TextFormatter_FormatBBCode_EmptyText_ReturnsEmpty()
        {
            var formatter = new TextFormatter();
            var html = formatter.FormatBBCode("");

            Assert.AreEqual("", html);
        }

        [Test]
        public void TextFormatter_FormatBBCode_NullText_ReturnsEmpty()
        {
            var formatter = new TextFormatter();
            var html = formatter.FormatBBCode(null);

            Assert.AreEqual("", html);
        }

        #endregion
    }
}
