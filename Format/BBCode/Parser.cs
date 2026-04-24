using System;
using System.Collections.Generic;
using Rsdn.Framework.Formatting.BBCode.Nodes;
using Rsdn.Framework.Formatting.BBCode.Tokens;

namespace Rsdn.Framework.Formatting.BBCode
{
    /// <summary>
    /// Парсер BBCode - преобразует токены в AST
    /// </summary>
    public class Parser
    {
        private readonly string _text;
        private List<Tokens.Token> _tokens = new();
        private int _position;

        // Теги, которые не могут содержать другие теги (inline content only)
        private static readonly HashSet<string> InlineOnlyTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "code", "c", "cs", "csharp", "vb", "vbnet", "cpp", "c++",
            "java", "js", "javascript", "ts", "typescript", "python", "py",
            "sql", "xml", "html", "css", "php", "ruby", "go", "rust",
            "asm", "assembly",
            "ccode", "cscode", "vbcode", "pascal", "delphi"
        };

        public Parser(string text)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
        }

        /// <summary>
        /// Разобрать текст в AST
        /// </summary>
        public DocumentNode Parse()
        {
            // Токенизация
            var span = _text.AsSpan();
            var tokenizer = new Tokenizer(span);
            _tokens = tokenizer.ReadAll();
            _position = 0;

            // Парсинг
            var document = new DocumentNode();
            ParseContent(document.Children, null);
            return document;
        }

        private void ParseContent(List<Node> nodes, string? endTag)
        {
            while (_position < _tokens.Count)
            {
                var token = _tokens[_position];

                if (token.Type == TokenType.EndOfText)
                    break;

                // Проверяем, не является ли это закрывающим тегом для родительского элемента
                if (token.Type == TokenType.CloseTag)
                {
                    var tagName = token.GetTagNameLower(_text.AsSpan());
                    if (endTag != null && string.Equals(tagName, endTag, StringComparison.OrdinalIgnoreCase))
                    {
                        _position++; // потребляем закрывающий тег
                        return;
                    }
                    // Несогласованный закрывающий тег - игнорируем
                    _position++;
                    continue;
                }

                if (token.Type == TokenType.Text)
                {
                    var text = token.GetTextString(_text.AsSpan());
                    if (!string.IsNullOrEmpty(text))
                        nodes.Add(new TextNode(text));
                    _position++;
                    continue;
                }

                if (token.Type == TokenType.QuotePrefix)
                {
                    ParseQuotePrefix(nodes);
                    continue;
                }

                if (token.Type == TokenType.VoidTag)
                {
                    var tagName = token.GetTagNameLower(_text.AsSpan());
                    nodes.Add(new VoidNode(tagName));
                    _position++;
                    continue;
                }

                if (token.Type == TokenType.OpenTag)
                {
                    ParseOpenTag(nodes, endTag);
                    continue;
                }

                _position++;
            }
        }

        private void ParseOpenTag(List<Node> nodes, string? parentEndTag)
        {
            var token = _tokens[_position];
            var tagName = token.GetTagNameLower(_text.AsSpan());
            var attribute = token.GetAttributeString(_text.AsSpan());

            _position++; // потребляем открывающий тег

            // Для code-тегов - парсим содержимое как BBCode (для поддержки [b], [i], и т.д.)
            if (InlineOnlyTags.Contains(tagName))
            {
                var codeNode = new TagNode(tagName, attribute);
                ParseContent(codeNode.Children, tagName); // Рекурсивно парсим содержимое
                nodes.Add(codeNode);
                return;
            }

            // Обычный парный тег
            var tagNode = new TagNode(tagName, attribute);
            ParseContent(tagNode.Children, tagName);
            nodes.Add(tagNode);
        }

        /// <summary>
        /// Разобрать префикс цитирования (A>, BB>>, и т.д.)
        /// Каждая строка с префиксом становится span.lineQuote levelN
        /// </summary>
        private void ParseQuotePrefix(List<Node> nodes)
        {
            var token = _tokens[_position];
            var level = token.QuoteLevel;
            var prefix = token.GetTextString(_text.AsSpan()) ?? "";
            
            _position++; // потребляем токен префикса
            
            // Проверяем, был ли предыдущий элемент TextNode с только пустой строкой
            // (это означает, что перед цитатой была пустая строка)
            var hasLeadingNewline = false;
            if (nodes.Count > 0 && nodes[nodes.Count - 1] is TextNode lastTextNode)
            {
                var lastText = lastTextNode.Text;
                // Если предыдущий текст заканчивается на \n\n или \r\n\r\n, 
                // значит перед цитатой была пустая строка
                if (lastText != null)
                {
                    // Проверяем, заканчивается ли текст на двойной перенос
                    if (lastText.EndsWith("\n\n"))
                    {
                        hasLeadingNewline = true;
                        // Удаляем последний \n из текста
                        lastTextNode.Text = lastText.Substring(0, lastText.Length - 1);
                    }
                    else if (lastText.EndsWith("\r\n\r\n"))
                    {
                        hasLeadingNewline = true;
                        // Удаляем последний \r\n из текста
                        lastTextNode.Text = lastText.Substring(0, lastText.Length - 2);
                    }
                    // Также проверяем случай когда текст = "\n" или "\r\n"
                    else if (lastText == "\n" || lastText == "\r\n" || 
                             lastText.Trim() == "")
                    {
                        hasLeadingNewline = true;
                        // Удаляем пустой TextNode
                        nodes.RemoveAt(nodes.Count - 1);
                    }
                }
            }
            
            // Читаем текст до конца строки (до первого \n)
            var lineText = new System.Text.StringBuilder();
            
            while (_position < _tokens.Count)
            {
                var nextToken = _tokens[_position];
                
                // Конец текста - заканчиваем строку
                if (nextToken.Type == TokenType.EndOfText)
                    break;
                
                // Следующий префикс цитирования - заканчиваем строку
                if (nextToken.Type == TokenType.QuotePrefix)
                    break;
                
                // Текст - добавляем до первого \n
                if (nextToken.Type == TokenType.Text)
                {
                    var text = nextToken.GetTextString(_text.AsSpan()) ?? "";
                    
                    // Ищем перенос строки
                    var nlIndex = text.IndexOf('\n');
                    if (nlIndex >= 0)
                    {
                        // Добавляем часть до переноса
                        lineText.Append(text.Substring(0, nlIndex));
                        
                        // Оставшийся текст после \n добавляем как отдельный TextNode
                        var remaining = text.Substring(nlIndex + 1);
                        _position++;
                        
                        // Создаём узел строки цитирования
                        var quoteNode = new QuoteLineNode(level, prefix, lineText.ToString());
                        quoteNode.HasLeadingNewline = hasLeadingNewline;
                        nodes.Add(quoteNode);
                        
                        // Добавляем оставшийся текст
                        if (!string.IsNullOrEmpty(remaining))
                        {
                            // Убираем \r если есть
                            if (remaining.StartsWith("\r"))
                                remaining = remaining.Substring(1);
                            if (!string.IsNullOrEmpty(remaining))
                                nodes.Add(new TextNode(remaining));
                        }
                        return;
                    }
                    
                    lineText.Append(text);
                    _position++;
                    continue;
                }
                
                // Другие токены - пропускаем
                _position++;
            }
            
            // Создаём узел строки цитирования
            var node = new QuoteLineNode(level, prefix, lineText.ToString());
            node.HasLeadingNewline = hasLeadingNewline;
            nodes.Add(node);
        }

        /// <summary>
        /// Прочитать "сырое" содержимое до закрывающего тега (без парсинга)
        /// </summary>
        private string? ReadRawContent(string tagName)
        {
            var start = _position;
            var foundEnd = false;

            while (_position < _tokens.Count)
            {
                var token = _tokens[_position];

                if (token.Type == TokenType.EndOfText)
                    break;

                if (token.Type == TokenType.CloseTag)
                {
                    var closeTagName = token.GetTagNameLower(_text.AsSpan());
                    if (string.Equals(closeTagName, tagName, StringComparison.OrdinalIgnoreCase))
                    {
                        foundEnd = true;
                        break;
                    }
                }

                _position++;
            }

            // Собираем текст из токенов между start и _position
            if (_position > start)
            {
                var contentStart = _tokens[start].Position;
                var contentEnd = _position < _tokens.Count 
                    ? _tokens[_position].Position 
                    : _text.Length;

                // Нужно учесть, что текст включает сами теги
                // Находим реальный конец контента
                if (foundEnd && _position < _tokens.Count)
                {
                    // Контент заканчивается перед закрывающим тегом
                    // Нам нужно найти позицию '[' закрывающего тега
                    var endPos = _tokens[_position].Position;
                    return _text.Substring(contentStart, endPos - contentStart);
                }

                return _text.Substring(contentStart);
            }

            // Потребляем закрывающий тег если найден
            if (foundEnd)
                _position++;

            return null;
        }
    }
}