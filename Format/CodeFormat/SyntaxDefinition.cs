using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rsdn.Framework.Formatting.CodeFormat
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
        public List<PatternDefinition> Patterns { get; set; } = [];

        /// <summary>
        /// Получить отображаемое имя (или имя, если отображаемое не задано).
        /// </summary>
        [JsonIgnore]
        public string DisplayNameOrName => DisplayName ?? Name;
    }
}