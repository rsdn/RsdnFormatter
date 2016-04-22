using System;

namespace Rsdn.Framework.Formatting.Resources
{
	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class ContentTypeAttribute : Attribute
	{
		public ContentTypeAttribute(string contentType)
		{
			ContentType = contentType;
		}

		public override string ToString()
		{
			return ContentType;
		}

		public string ContentType { get; }
	}
}
