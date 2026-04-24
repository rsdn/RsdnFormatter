namespace Rsdn.Framework.Formatting.Tests.HtmlComparer;

/// <summary>
/// Тип различия в HTML
/// </summary>
public enum HtmlDifferenceKind
{
	MissingElement,
	DifferentTag,
	MissingAttribute,
	ExtraAttribute,
	DifferentAttributeValue,
	DifferentText
}