using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Rsdn.Framework.Formatting.CodeFormat;

/// <summary>
/// Определение паттерна подсветки.
/// </summary>
[PublicAPI]
public class PatternDefinition
{
	/// <summary>
	/// Имя группы (определяет CSS-класс: kw, str, com).
	/// </summary>
	[JsonPropertyName("name")]
	public string Name => "";

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
	public List<string> Keywords { get; set; } = [];

	/// <summary>
	/// Список регулярных выражений (для type = regex).
	/// </summary>
	[JsonPropertyName("expressions")]
	public List<string> Expressions { get; set; } = [];

	/// <summary>
	/// Флаг игнорирования регистра для ключевых слов.
	/// </summary>
	[JsonPropertyName("ignoreCase")]
	public bool IgnoreCase { get; set; }
}