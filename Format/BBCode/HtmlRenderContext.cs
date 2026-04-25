using System.Text;
using JetBrains.Annotations;

namespace Rsdn.Framework.Formatting.BBCode;

/// <summary>
/// Контекст рендеринга HTML.
/// Иммутабельный объект, передаваемый через аргументы методов визитора.
/// </summary>
[PublicAPI]
public class HtmlRenderContext
{
	/// <summary>
	/// StringBuilder для накопления вывода
	/// </summary>
	public StringBuilder Output { get; }

	/// <summary>
	/// Глубина вложенности в preformatted блоках (pre, code)
	/// </summary>
	public int PreformattedDepth { get; }

	/// <summary>
	/// Находимся ли внутри preformatted блока
	/// </summary>
	public bool InsidePreformatted => PreformattedDepth > 0;

	/// <summary>
	/// Только что был выведен блочный элемент (h1-h6, blockquote, pre, div, etc.)
	/// В этом случае следующий перенос строки не должен давать <br />
	/// </summary>
	public bool AfterBlockElement { get; }

	/// <summary>
	/// Создать контекст с указанным StringBuilder
	/// </summary>
	public HtmlRenderContext(StringBuilder output)
	{
		Output = output;
		PreformattedDepth = 0;
		AfterBlockElement = false;
	}

	/// <summary>
	/// Приватный конструктор для создания производных контекстов
	/// </summary>
	private HtmlRenderContext(StringBuilder output, int preformattedDepth, bool afterBlockElement)
	{
		Output = output;
		PreformattedDepth = preformattedDepth;
		AfterBlockElement = afterBlockElement;
	}

	/// <summary>
	/// Войти в preformatted блок (увеличить глубину)
	/// </summary>
	public HtmlRenderContext EnterPreformatted()
		=> new(Output, PreformattedDepth + 1, AfterBlockElement);

	/// <summary>
	/// Выйти из preformatted блока (уменьшить глубину)
	/// </summary>
	public HtmlRenderContext ExitPreformatted()
		=> new(Output, PreformattedDepth - 1, AfterBlockElement);

	/// <summary>
	/// Установить флаг "после блочного элемента"
	/// </summary>
	public HtmlRenderContext SetAfterBlock()
		=> new(Output, PreformattedDepth, true);

	/// <summary>
	/// Сбросить флаг "после блочного элемента"
	/// </summary>
	public HtmlRenderContext ClearAfterBlock()
		=> new(Output, PreformattedDepth, false);
}
