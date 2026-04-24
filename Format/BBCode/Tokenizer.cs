using System;
using System.Collections.Generic;
using Rsdn.Framework.Formatting.BBCode.Tokens;

namespace Rsdn.Framework.Formatting.BBCode;

/// <summary>
/// Лексер BBCode - разбивает текст на токены.
/// Реализован как ref struct для работы с ReadOnlySpan без аллокаций.
/// </summary>
public ref struct Tokenizer(ReadOnlySpan<char> text)
{
    private readonly ReadOnlySpan<char> _text = text;

    /// <summary>
    /// Текущая позиция в тексте
    /// </summary>
    public int Position { get; private set; } = 0;

    /// <summary>
    /// Длина текста
    /// </summary>
    public int Length => _text.Length;

    /// <summary>
    /// Есть ли ещё токены
    /// </summary>
    public bool HasMore => Position < _text.Length;

    /// <summary>
    /// Прочитать следующий токен
    /// </summary>
    public Token ReadNext()
    {
        if (Position >= _text.Length)
            return Token.EofToken(Position);

        var ch = _text[Position];

        // Начало тега
        return ch == '['
            ? ReadTag()
            : ReadText();
    }

    /// <summary>
    /// Прочитать все токены в список
    /// </summary>
    public List<Token> ReadAll()
    {
        var tokens = new List<Token>();
        Token token;
        do
        {
            token = ReadNext();
            tokens.Add(token);
        } while (token.Type != TokenType.EndOfText);

        return tokens;
    }

    private Token ReadTag()
    {
        var startPos = Position;

        // Пропускаем '['
        Position++;

        if (Position >= _text.Length)
            return Token.TextToken(new TextRange(startPos, Position), startPos);

        var ch = _text[Position];

        // Закрывающий тег: [/tag]
        if (ch == '/')
        {
            Position++; // пропускаем '/'
            var tagNameStart = Position;
            var tagNameEnd = ReadTagNameEnd();

            if (tagNameEnd == tagNameStart)
                return Token.TextToken(new TextRange(startPos, Position), startPos);

            // Пропускаем пробелы перед ']'
            SkipWhitespace();

            if (Position >= _text.Length || _text[Position] != ']')
                return Token.TextToken(new TextRange(startPos, Position), startPos);

            Position++; // пропускаем ']'
            return Token.CloseTagToken(new TextRange(tagNameStart, tagNameEnd), startPos);
        }

        // Открывающий тег: [tag] или [tag=attr]
        var nameStart = Position;
        var nameEnd = ReadTagNameEnd();

        if (nameEnd == nameStart)
            return Token.TextToken(new TextRange(startPos, Position), startPos);

        // Пропускаем пробелы
        SkipWhitespace();

        TextRange attrRange = default;

        // Атрибут: [tag=value] или [tag value]
        if (Position < _text.Length && _text[Position] == '=')
        {
            Position++; // пропускаем '='
            attrRange = ReadAttributeRange();
        }
        // Проверяем, есть ли атрибут после пробела: [tag value]
        else if (Position < _text.Length && _text[Position] != ']')
        {
            // Читаем атрибут (это может быть слово после имени тега)
            attrRange = ReadAttributeRange();
        }

        // Пропускаем пробелы перед ']'
        SkipWhitespace();

        if (Position >= _text.Length || _text[Position] != ']')
            return Token.TextToken(new TextRange(startPos, Position), startPos);

        Position++; // пропускаем ']'

        var tagNameRange = new TextRange(nameStart, nameEnd);
        var tagName = _text.Slice(nameStart, nameEnd - nameStart);

        // Проверяем, является ли тег void (самозакрывающимся)
        return IsVoidTag(tagName)
            ? Token.VoidTagToken(tagNameRange, startPos)
            : Token.OpenTagToken(tagNameRange, attrRange, startPos);
    }

    private Token ReadText()
    {
        var start = Position;

        while (Position < _text.Length)
        {
            var ch = _text[Position];
            if (ch == '[')
                break;
            
            // Проверяем префикс цитирования в начале строки
            // Префикс имеет вид: A>, B>, BB>>, AAA>>>, и т.д.
            // Он должен быть в начале строки (после \n или в начале документа)
            if (IsQuotePrefixStart())
            {
                // Если уже накопили текст - возвращаем его
                if (Position > start)
                    return Token.TextToken(new TextRange(start, Position), start);
                
                // Читаем префикс цитирования
                return ReadQuotePrefix();
            }
            
            Position++;
        }

        return Token.TextToken(new TextRange(start, Position), start);
    }

    /// <summary>
    /// Проверить, находимся ли мы в позиции где может начинаться префикс цитирования
    /// (начало строки или начало документа)
    /// Паттерн: одна или более букв A-Z, за которыми следует один или более '>'
    /// </summary>
    private bool IsQuotePrefixStart()
    {
        if (Position >= _text.Length)
            return false;
        
        var ch = _text[Position];
        
        // Префикс цитирования начинается с буквы A-Z
        if (ch is < 'A' or > 'Z')
            return false;
        
        // Проверяем, что это начало строки
        if (Position > 0)
        {
            var prevChar = _text[Position - 1];
            if (prevChar != '\n' && prevChar != '\r')
                return false;
        }
        
        // Теперь проверяем, что за буквами следует '>'
        // Считаем количество букв
        var pos = Position;
        while (pos < _text.Length && _text[pos] is >= 'A' and <= 'Z')
            pos++;
        
        // Должен быть хотя бы один '>' после букв
        return pos < _text.Length && _text[pos] == '>';
    }

    /// <summary>
    /// Прочитать префикс цитирования: A>, BB>>, AAA>>>, и т.д.
    /// </summary>
    private Token ReadQuotePrefix()
    {
        var start = Position;
        
        // Читаем буквы (A, B, AA, BB, AAA, и т.д.)
        while (Position < _text.Length)
        {
            var ch = _text[Position];
            if (ch is < 'A' or > 'Z')
                break;
            Position++;
        }
        
        // Должны быть хотя бы одна буква
        if (Position == start)
            return Token.TextToken(new TextRange(start, Position), start);
        
        // Теперь должны быть '>=' (один или более)
        var levelStart = Position;
        while (Position < _text.Length && _text[Position] == '>')
        {
            Position++;
        }
        
        // Должен быть хотя бы один '>'
        var level = Position - levelStart;
        if (level == 0)
        {
            // Это не префикс цитирования - возвращаем как обычный текст
            return Token.TextToken(new TextRange(start, Position), start);
        }
        
        // Проверяем, что после '>' идёт пробел или конец строки/документа
        // (это отличает цитирование от обычного текста типа "A>B")
        if (Position < _text.Length)
        {
            var nextChar = _text[Position];
            if (nextChar != ' ' && nextChar != '\t' && nextChar != '\n' && nextChar != '\r')
            {
                // Это не префикс цитирования - возвращаем как обычный текст
                return Token.TextToken(new TextRange(start, Position), start);
            }
        }
        
        return Token.QuotePrefixToken(new TextRange(start, Position), level, start);
    }

    private int ReadTagNameEnd()
    {
        // Имя тега может содержать буквы, цифры, и некоторые специальные символы
        while (Position < _text.Length)
        {
            var ch = _text[Position];

            // Допустимые символы в имени тега
            // '*' - специальный случай для [*] (list item)
            // '#' - для тегов типа [c#]
            if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == ':' || ch == '*' || ch == '#')
            {
                Position++;
            }
            else
            {
                break;
            }
        }

        return Position;
    }

    private TextRange ReadAttributeRange()
    {
        var start = Position;

        // Атрибут может быть в кавычках
        if (Position < _text.Length && (_text[Position] == '"' || _text[Position] == '\''))
        {
            var quote = _text[Position];
            Position++; // пропускаем открывающую кавычку

            while (Position < _text.Length && _text[Position] != quote)
            {
                Position++;
            }

            if (Position < _text.Length && _text[Position] == quote)
                Position++; // пропускаем закрывающую кавычку

            // Возвращаем содержимое без кавычек
            return new TextRange(start + 1, Position - 1);
        }

        // Атрибут без кавычек - читаем до ']' или пробела
        while (Position < _text.Length)
        {
            var ch = _text[Position];
            if (ch == ']' || char.IsWhiteSpace(ch))
                break;
            Position++;
        }

        return new TextRange(start, Position);
    }

    private void SkipWhitespace()
    {
        while (Position < _text.Length && char.IsWhiteSpace(_text[Position]))
        {
            Position++;
        }
    }

    private static bool IsVoidTag(ReadOnlySpan<char> tagName) =>
        tagName.Length switch
        {
            // Быстрое сравнение для часто используемых тегов
            1 => tagName[0] == '*',
            2 => (tagName[0] == 'h' || tagName[0] == 'H') && (tagName[1] == 'r' || tagName[1] == 'R'),
            _ => false
        };
}