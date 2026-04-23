using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rsdn.Framework.Formatting
{
    /// <summary>
    /// Определение синтаксиса языка для подсветки кода.
    /// </summary>
    public class SyntaxDefinition
    {
        /// <summary>
        /// Внутреннее имя языка (идентификатор).
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// Отображаемое имя языка.
        /// </summary>
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Опции регулярных выражений для всего языка.
        /// </summary>
        [JsonPropertyName("options")]
        public string? Options { get; set; }

        /// <summary>
        /// Список паттернов для подсветки.
        /// </summary>
        [JsonPropertyName("patterns")]
        public List<PatternDefinition> Patterns { get; set; } = new();

        /// <summary>
        /// Получить отображаемое имя (или имя, если отображаемое не задано).
        /// </summary>
        [JsonIgnore]
        public string DisplayNameOrName => DisplayName ?? Name;
    }

    /// <summary>
    /// Определение паттерна подсветки.
    /// </summary>
    public class PatternDefinition
    {
        /// <summary>
        /// Имя группы (определяет CSS-класс: kw, str, com).
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        /// <summary>
        /// Тип паттерна: keyword, regex.
        /// </summary>
        [JsonPropertyName("type")]
        public PatternType Type { get; set; } = PatternType.Regex;

        /// <summary>
        /// Префикс для ключевых слов (например, \b для границ слова).
        /// </summary>
        [JsonPropertyName("prefix")]
        public string? Prefix { get; set; }

        /// <summary>
        /// Постфикс для ключевых слов.
        /// </summary>
        [JsonPropertyName("postfix")]
        public string? Postfix { get; set; }

        /// <summary>
        /// Список ключевых слов (для type = keyword).
        /// </summary>
        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; } = new();

        /// <summary>
        /// Список регулярных выражений (для type = regex).
        /// </summary>
        [JsonPropertyName("expressions")]
        public List<string> Expressions { get; set; } = new();

        /// <summary>
        /// Флаг игнорирования регистра для ключевых слов.
        /// </summary>
        [JsonPropertyName("ignoreCase")]
        public bool IgnoreCase { get; set; }
    }

    /// <summary>
    /// Тип паттерна подсветки.
    /// </summary>
    public enum PatternType
    {
        /// <summary>
        /// Паттерн задаётся списком ключевых слов.
        /// Оптимизируется через Trie-структуру.
        /// </summary>
        Keyword,

        /// <summary>
        /// Паттерн задаётся регулярным выражением(ями).
        /// </summary>
        Regex
    }
}