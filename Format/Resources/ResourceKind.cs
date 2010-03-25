namespace Rsdn.Framework.Formatting.Resources
{
	public enum ResourceKind
	{
		None,

		[ContentType("text/css")]
		Css,

		[ContentType("text/htm")]
		Html,

		[ContentType("text/javascript")]
		JavaScript,

		[ContentType("text/vbscript")]
		VBScript,

		[ContentType("image/gif")]
		Gif,

		[ContentType("image/jpeg")]
		Jpeg,

		[ContentType("image/png")]
		Png
	}
}
