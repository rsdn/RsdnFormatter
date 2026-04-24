namespace Rsdn.Framework.Formatting.BBCode.Tokens;

/// <summary>
/// Тип токена BBCode
/// </summary>
public enum TokenType
{
	/// <summary>
	/// Обычный текст
	/// </summary>
	Text,
        
	/// <summary>
	/// Открывающий тег: [b], [i], [url=...]
	/// </summary>
	OpenTag,
        
	/// <summary>
	/// Закрывающий тег: [/b], [/i], [/url]
	/// </summary>
	CloseTag,
        
	/// <summary>
	/// Пустой тег (void): [*], [hr]
	/// </summary>
	VoidTag,
        
	/// <summary>
	/// Конец текста
	/// </summary>
	EndOfText,
	
	/// <summary>
	/// Префикс цитирования: A>, BB>>, и т.д.
	/// Используется для обозначения уровня цитирования
	/// </summary>
	QuotePrefix
}
