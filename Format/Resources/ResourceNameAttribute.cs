using System;

namespace Rsdn.Framework.Formatting.Resources
{
	[AttributeUsage(AttributeTargets.Field)]
	internal sealed class ResourceNameAttribute : Attribute
	{
		#region Construction
		public ResourceNameAttribute(string resName)
		{
			ResourceName = resName;
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return ResourceName;
		}
		#endregion


		#region Properties
		public string ResourceName { get; private set; }
		#endregion
	}
}