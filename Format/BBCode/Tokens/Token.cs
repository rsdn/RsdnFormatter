using System;

namespace Rsdn.Framework.Formatting.BBCode.Tokens
{
    /// <summary>
    /// Токен BBCode. Хранит диапазоны в исходном тексте вместо строк для избежания аллокаций.
    /// </summary>
    public readonly struct Token
    {
        /// <summary>
        /// Тип токена
        /// </summary>
        public TokenType Type { get; }
        
        /// <summary>
        /// Диапазон имени тега (для тегов)
        /// </summary>
        public TextRange TagNameRange { get; }
        
        /// <summary>
        /// Диапазон значения атрибута (для [url=value], [cut=value])
        /// </summary>
        public TextRange AttributeRange { get; }
        
        /// <summary>
        /// Диапазон текстового содержимого (для текстовых токенов)
        /// </summary>
        public TextRange TextRange { get; }
        
        /// <summary>
        /// Позиция начала токена в исходном тексте
        /// </summary>
        public int Position { get; }

        private Token(TokenType type, TextRange textRange, TextRange tagNameRange, TextRange attributeRange, int position, int quoteLevel = 0)
        {
            Type = type;
            TextRange = textRange;
            TagNameRange = tagNameRange;
            AttributeRange = attributeRange;
            Position = position;
            QuoteLevel = quoteLevel;
        }

        /// <summary>
        /// Создать текстовый токен
        /// </summary>
        public static Token TextToken(TextRange textRange, int position) =>
            new Token(TokenType.Text, textRange, default, default, position);

        /// <summary>
        /// Создать открывающий тег
        /// </summary>
        public static Token OpenTagToken(TextRange tagNameRange, TextRange attributeRange, int position) =>
            new Token(TokenType.OpenTag, default, tagNameRange, attributeRange, position);

        /// <summary>
        /// Создать закрывающий тег
        /// </summary>
        public static Token CloseTagToken(TextRange tagNameRange, int position) =>
            new Token(TokenType.CloseTag, default, tagNameRange, default, position);

        /// <summary>
        /// Создать пустой тег
        /// </summary>
        public static Token VoidTagToken(TextRange tagNameRange, int position) =>
            new Token(TokenType.VoidTag, default, tagNameRange, default, position);

        /// <summary>
        /// Создать токен конца текста
        /// </summary>
        public static Token EofToken(int position) =>
            new Token(TokenType.EndOfText, default, default, default, position);

        /// <summary>
        /// Создать токен префикса цитирования (A>, BB>>, и т.д.)
        /// </summary>
        /// <param name="textRange">Диапазон префикса (например "A>" или "BB>>")</param>
        /// <param name="level">Уровень цитирования (количество '>')</param>
        /// <param name="position">Позиция в исходном тексте</param>
        public static Token QuotePrefixToken(TextRange textRange, int level, int position) =>
            new Token(TokenType.QuotePrefix, textRange, default, default, position, level);

        // Уровень цитирования для токена QuotePrefix

        /// <summary>
        /// Уровень цитирования (количество '>') для токена QuotePrefix
        /// </summary>
        public int QuoteLevel { get; }

        /// <summary>
        /// Получить текст токена из исходного текста
        /// </summary>
        public ReadOnlySpan<char> GetText(ReadOnlySpan<char> source)
        {
            if (TextRange.IsEmpty)
                return default;
            
            return Type is TokenType.Text or TokenType.QuotePrefix
                ? source.Slice(TextRange.Start, TextRange.Length) 
                : default;
        }

        /// <summary>
        /// Получить имя тега из исходного текста
        /// </summary>
        public ReadOnlySpan<char> GetTagName(ReadOnlySpan<char> source)
        {
            return Type is TokenType.OpenTag or TokenType.CloseTag or TokenType.VoidTag && !TagNameRange.IsEmpty
                ? source.Slice(TagNameRange.Start, TagNameRange.Length) 
                : default;
        }

        /// <summary>
        /// Получить атрибут из исходного текста
        /// </summary>
        public ReadOnlySpan<char> GetAttribute(ReadOnlySpan<char> source)
        {
            return Type == TokenType.OpenTag && !AttributeRange.IsEmpty 
                ? source.Slice(AttributeRange.Start, AttributeRange.Length) 
                : default;
        }

        /// <summary>
        /// Получить имя тега в нижнем регистре (для сравнения)
        /// </summary>
        public string GetTagNameLower(ReadOnlySpan<char> source)
        {
            var span = GetTagName(source);
            return span.IsEmpty ? string.Empty : span.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Получить атрибут как строку
        /// </summary>
        public string? GetAttributeString(ReadOnlySpan<char> source)
        {
            var span = GetAttribute(source);
            return span.IsEmpty ? null : span.ToString();
        }

        /// <summary>
        /// Получить текст как строку
        /// </summary>
        public string? GetTextString(ReadOnlySpan<char> source)
        {
            var span = GetText(source);
            return span.IsEmpty ? null : span.ToString();
        }

        public override string ToString()
        {
            return Type switch
            {
                TokenType.Text => $"Text: [{TextRange.Start}..{TextRange.End}]",
                TokenType.OpenTag => $"OpenTag: [{TagNameRange.Start}..{TagNameRange.End}]",
                TokenType.CloseTag => $"CloseTag: [/{TagNameRange.Start}..{TagNameRange.End}]",
                TokenType.VoidTag => $"VoidTag: [{TagNameRange.Start}..{TagNameRange.End}]",
                TokenType.EndOfText => "EOF",
                _ => $"Unknown: {Type}"
            };
        }
    }
}