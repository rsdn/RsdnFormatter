using System;

namespace Rsdn.Framework.Formatting.Resources
{
	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class ContentTypeAttribute : Attribute
	{
		#region Construction
		public ContentTypeAttribute(string contentType)
		{
			ContentType = contentType;
		}
		#endregion

		#region Methods
		public override string ToString()
		{
			return ContentType;
		}
		#endregion

		#region Properties
		public string ContentType { get; private set; }
		#endregion
	}
}
