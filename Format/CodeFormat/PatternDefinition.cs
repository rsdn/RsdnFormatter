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
	public string Name { get; set; } = "";

	/// <summary>
	/// Тип паттерна: keyword, regex.
	/// </summary>
	[JsonPropertyName("type")]
	public PatternType Type { get; set; } = PatternType.Regex;

	/// <summary>
	/// Граница перед ключевым словом.
	/// </summary>
	[JsonPropertyName("prefix")]
	public KeywordBoundary Prefix { get; set; } = KeywordBoundary.None;

	/// <summary>
	/// Граница после ключевого слова.
	/// </summary>
	[JsonPropertyName("postfix")]
	public KeywordBoundary Postfix { get; set; } = KeywordBoundary.None;

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
