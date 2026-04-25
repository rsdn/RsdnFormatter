namespace Rsdn.Framework.Formatting.CodeFormat;

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